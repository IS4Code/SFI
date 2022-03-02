using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Tools.IO;
using IS4.MultiArchiver.Vocabulary;
using MorseCode.ITask;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>
	{
        public IHashedContentUriFormatter ContentUriFormatter { get; set; }

        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }

        public ICollection<IBinaryFileFormat> DataFormats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        public int FileSizeToWriteToDisk { get; set; } = 524288;

        public int MaxDataLengthToStore => Math.Max(64, HashAlgorithms.Sum(h => h.HashSize + 64)) - 16;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public async ValueTask<AnalysisResult> Analyze(IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var match = await new DataAnalysis(this, streamFactory, context, analyzers).Match();
            var node = match.Node;

            var results = match.Results.Where(result => result.IsValid);

            foreach(var result in results.GroupBy(r => r.Result))
            {
                if(result.Key != null)
                {
                    match.Recognized = true;
                    result.Key.Set(Properties.HasFormat, node);
                }
            }

            return await analyzers.Analyze<IDataObject>(match, context.WithNode(node));
		}

        class DataAnalysis
        {
            readonly DataAnalyzer analyzer;

            readonly MemoryStream signatureBuffer;
            readonly IEncodingDetector encodingDetector;
            readonly IStreamFactory streamFactory;
            readonly AnalysisContext context;
            readonly IEntityAnalyzerProvider analyzers;
            readonly Dictionary<IDataHashAlgorithm, (ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)> hashes = new Dictionary<IDataHashAlgorithm, (ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)>(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);

            IStreamFactory seekableFactory;
            bool isBinary;

            public DataAnalysis(DataAnalyzer analyzer, IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzerProvider analyzers)
            {
                this.analyzer = analyzer;
                signatureBuffer = new MemoryStream(Math.Max(analyzer.MaxDataLengthToStore + 1, analyzer.DataFormats.Select(fmt => fmt.HeaderLength).DefaultIfEmpty(0).Max()));
                encodingDetector = analyzer.EncodingDetectorFactory?.Invoke();

                this.streamFactory = streamFactory;
                this.context = context.WithMatchContext(c => c.WithServices(this.streamFactory));
                this.analyzers = analyzers;
            }

            DataMatch MatchFactory()
            {
                return new DataMatch(analyzer, streamFactory, seekableFactory, signatureBuffer, encodingDetector, isBinary, context, analyzers);
            }

            public async Task<DataMatch> Match()
            {
                var lazyMatch = new Lazy<DataMatch>(MatchFactory, false);
            
                long actualLength = 0;

                DataMatch match;

                var tmpPath = default(FileTools.TemporaryFile);
                try{
                    Stream outputStream = null;
                    try{
                        using(var stream = streamFactory.Open())
                        {
                            if(streamFactory.Access != StreamFactoryAccess.Parallel || !stream.CanSeek)
                            {
                                if(streamFactory.Length >= analyzer.FileSizeToWriteToDisk)
                                {
                                    tmpPath = FileTools.GetTemporaryFile("b");
                                    outputStream = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                                }else{
                                    outputStream = new MemoryStream();
                                }
                            
                                foreach(var hash in analyzer.HashAlgorithms)
                                {
                                    var queue = ChannelArrayStream.Create(out var writer, 1);
                                    hashes[hash] = (writer, Task.Run(() => hash.ComputeHash(queue, streamFactory)));
                                }
                            }else{
                                seekableFactory = streamFactory;
                            
                                foreach(var hash in analyzer.HashAlgorithms)
                                {
                                    hashes[hash] = (null, Task.Run(() => {
                                        using(var hashStream = streamFactory.Open())
                                        {
                                            return hash.ComputeHash(hashStream, streamFactory);
                                        }
                                    }));
                                }
                            }

                            bool? couldBeUnicode = null;

                            var buffer = ArrayPool<byte>.Shared.Rent(16384);

                            var writers = hashes.Values.Select(v => v.writer).Where(w => w != null);

                            try{
                                int read;
                                while((read = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                                {
                                    var segment = buffer.Slice(0, read);
                                    var writing = writers.Select(async writer => {
                                        await writer.WriteAsync(segment);
                                        await writer.WriteAsync(default);
                                        await writer.WaitToWriteAsync();
                                    }).ToArray();

                                    actualLength += read;

                                    if(couldBeUnicode == null)
                                    {
                                        couldBeUnicode = DataTools.FindBom(buffer.AsSpan()) > 0;
                                    }

                                    if(!isBinary)
                                    {
                                        var data = buffer.Slice(0, read);
                                        if(couldBeUnicode == false && DataTools.IsBinary(data))
                                        {
                                            isBinary = true;
                                        }else{
                                            encodingDetector.Write(data);
                                        }
                                    }

                                    outputStream?.Write(buffer, 0, read);

                                    if(signatureBuffer.Length < signatureBuffer.Capacity)
                                    {
                                        signatureBuffer.Write(buffer, 0, Math.Min(signatureBuffer.Capacity - (int)signatureBuffer.Length, read));
                                    }else if(outputStream == null)
                                    {
                                        _ = lazyMatch.Value;
                                    }

                                    await Task.WhenAll(writing);
                                }

                                foreach(var writer in writers)
                                {
                                    writer.Complete();
                                }
                            }finally{
                                ArrayPool<byte>.Shared.Return(buffer);
                            }

                            encodingDetector.End();
                        }
                    }finally{
                        outputStream?.Dispose();
                    }

                    if(tmpPath != null)
                    {
                        seekableFactory = new FileInfoWrapper(new FileInfo(tmpPath), streamFactory);
                    }else if(outputStream is MemoryStream memoryStream)
                    {
                        var buffer = memoryStream.GetData();
                        seekableFactory = new MemoryStreamFactory(buffer, streamFactory);
                    }

                    match = lazyMatch.Value;

                    await Task.WhenAll(match.Results.Select(r => r.Finish()));
                }finally{
                    tmpPath.Dispose();
                }

                match.ActualLength = actualLength;

                if(!match.IsComplete)
                {
                    foreach(var hash in hashes)
                    {
                        match[hash.Key] = hash.Value.data.Result;
                    }
                }

                return match;
            }
        }

        class DataMatch : Dictionary<IDataHashAlgorithm, byte[]>, IDataObject
        {
            readonly bool isBinary;
            public bool IsBinary => isBinary || charsetMatch?.Charset == null;
            public IReadOnlyList<FormatResult> Results { get; }
            public ILinkedNode Node { get; }
            public bool IsComplete { get; }

            readonly CharsetMatch charsetMatch;

            public ArraySegment<byte> ByteValue { get; }

            public string StringValue { get; }

            public IStreamFactory Source { get; }

            public IStreamFactory StreamFactory { get; }

            public long ActualLength { get; set; }

            public string Charset => charsetMatch?.Charset;

            public Encoding Encoding => charsetMatch?.Encoding;

            public bool Recognized { get; set; }

            IReadOnlyDictionary<IDataHashAlgorithm, byte[]> IDataObject.Hashes => this;

            public DataMatch(DataAnalyzer analyzer, IStreamFactory source, IStreamFactory streamFactory, MemoryStream signatureBuffer, IEncodingDetector encodingDetector, bool isBinary, AnalysisContext context, IEntityAnalyzerProvider analyzers)
            {
                Source = source;

                StreamFactory = streamFactory;

                ByteValue = signatureBuffer.GetData();

                if(ByteValue.Count == 0)
                {
                    // empty file is always binary
                    isBinary = true;
                }

                IsComplete = ByteValue.Count <= analyzer.MaxDataLengthToStore;

                if(!isBinary)
                {
                    // obtain the encoding from the detector and try to convert the data
                    charsetMatch = new CharsetMatch(encodingDetector);

                    if(charsetMatch.Encoding != null)
                    {
                        StringValue = TryGetString(charsetMatch.Encoding, ByteValue);

                        if(IsComplete && StringValue == null)
                        {
                            // the file is corrupted, revert to binary
                            isBinary = true;
                        }
                    }
                }

                Node = context.Node;

                if(Node == null)
                {
                    if(IsComplete)
                    {
                        Node = context.NodeFactory.Create(UriTools.DataUriFormatter, (null, encodingDetector?.Charset, ByteValue));
                    }else{
                        Node = GetContentNode(context.NodeFactory, analyzer.ContentUriFormatter);
                    }
                }

                this.isBinary = isBinary;

                if(ByteValue.Count > 0)
                {
                    var nodeCreated = new TaskCompletionSource<ILinkedNode>();
                    Results = analyzer.DataFormats.Where(fmt => fmt.CheckHeader(ByteValue, isBinary, encodingDetector)).Select(fmt => new FormatResult(this, streamFactory, fmt, nodeCreated, Node, context, analyzers)).ToList();
                }else{
                    Results = Array.Empty<FormatResult>();
                }
            }

            private string TryGetString(Encoding encoding, ArraySegment<byte> data)
            {
                try{
                    var preamble = encoding.GetPreamble();
                    if(preamble?.Length > 0 && data.AsSpan().StartsWith(preamble))
                    {
                        return encoding.GetString(data.Slice(preamble.Length));
                    }
                    return encoding.GetString(data);
                }catch(ArgumentException)
                {
                    return null;
                }
            }

            private ILinkedNode GetContentNode(ILinkedNodeFactory nodeFactory, IHashedContentUriFormatter formatter)
            {
                if(formatter != null)
                {
                    foreach(var algorithm in formatter.SupportedAlgorithms.OfType<IDataHashAlgorithm>())
                    {
                        if(TryGetValue(algorithm, out var hash))
                        {
                            var node = nodeFactory.Create(formatter, (algorithm, hash, IsBinary));
                            if(node != null)
                            {
                                return node;
                            }
                        }
                    }
                }
                return nodeFactory.NewGuidNode();
            }
        }

        class CharsetMatch
        {
            public Encoding Encoding { get; }
            public string Charset { get; }
            public double Confidence { get; }

            public CharsetMatch(IEncodingDetector encodingDetector)
            {
                Confidence = encodingDetector.Confidence;
                if(Confidence > 0)
                {
                    Encoding = TryGetEncoding(encodingDetector.Charset);
                    Charset = Encoding?.WebName ?? encodingDetector.Charset;
                }
            }

            private Encoding TryGetEncoding(string charset)
            {
                if(charset == null) return null;
                try{
                    return Encoding.GetEncoding(charset, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                }catch(ArgumentException)
                {
                    return null;
                }
            }
        }

		public ValueTask<AnalysisResult> Analyze(byte[] data, AnalysisContext context, IEntityAnalyzerProvider analyzers)
		{
			return Analyze(new MemoryStreamFactory(new ArraySegment<byte>(data), data, null), context, analyzers);
		}
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<ILinkedNode, (Stream stream, MatchContext matchContext)>
		{
            readonly DataMatch fileMatch;
            readonly IBinaryFileFormat format;
            readonly ILinkedNode parent;
            readonly AnalysisContext context;
            readonly IEntityAnalyzerProvider analyzer;
            readonly Task<ILinkedNode> task;
            readonly IStreamFactory streamFactory;
            readonly TaskCompletionSource<ILinkedNode> nodeCreated;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => format.HeaderLength;

            public string Extension { get; private set; }
            public string Label { get; private set; }

            public ILinkedNode Result => task?.Result;

            public FormatResult(DataMatch fileMatch, IStreamFactory streamFactory, IBinaryFileFormat format, TaskCompletionSource<ILinkedNode> nodeCreated, ILinkedNode parent, AnalysisContext context, IEntityAnalyzerProvider analyzer)
			{
                this.fileMatch = fileMatch;
                this.format = format;
                this.parent = parent;
                this.context = context;
                this.analyzer = analyzer;
                this.streamFactory = streamFactory;
                this.nodeCreated = nodeCreated;
                task = StartReading(streamFactory, Reader);
            }

            private async ValueTask<ILinkedNode> Reader(Stream stream)
            {
                var streamContext = context.MatchContext.WithStream(stream);
                return await format.Match(stream, streamContext, this, (stream, streamContext));
            }

            public async Task Finish()
            {
                try{
                    await task;
                }catch(InternalArchiverException)
                {
                    throw;
                }catch{

                }
            }

            const int MaxResultWaitTime = 1000;

            async ITask<ILinkedNode> IResultFactory<ILinkedNode, (Stream stream, MatchContext matchContext)>.Invoke<T>(T value, (Stream stream, MatchContext matchContext) args)
            {
                var (stream, matchContext) = args;
                var streamContext = context.WithMatchContext(matchContext);
                try{
                    var formatObj = new BinaryFormatObject<T>(fileMatch, format, value);
                    while(parent[formatObj] == null)
                    {
                        ILinkedNode node;
                        var timeout = Task.Delay(MaxResultWaitTime);
                        if(await Task.WhenAny(nodeCreated.Task) == timeout)
                        {
                            formatObj = new BinaryFormatObject<T>(fileMatch, ImprovisedFormat<T>.Instance, value);
                            continue;
                        }else{
                            node = nodeCreated.Task.Result;
                        }
                        if(node != null)
                        {
                            var result = await analyzer.Analyze(value, streamContext.WithNode(node));
                            node = result.Node;
                        }
                        return node;
                    }
                    {
                        var result = await analyzer.Analyze(formatObj, streamContext.WithParent(parent));
                        nodeCreated?.TrySetResult(result.Node);
                        return result.Node;
                    }
                }catch(Exception e)
                {
                    throw new InternalArchiverException(e);
                }
            }

            private Task<ILinkedNode> StartReading(IStreamFactory streamFactory, Func<Stream, ValueTask<ILinkedNode>> reader)
            {
                if(streamFactory.Access == StreamFactoryAccess.Parallel)
                {
                    return Task.Run(() => StartReadingInner(streamFactory, reader));
                }else{
                    try{
                        return StartReadingInner(streamFactory, reader);
                    }catch(Exception e)
                    {
                        return Task.FromException<ILinkedNode>(e);
                    }
                }
            }

            private async Task<ILinkedNode> StartReadingInner(IStreamFactory streamFactory, Func<Stream, ValueTask<ILinkedNode>> reader)
            {
                var stream = streamFactory.Open();
                try{
                    return await reader(stream);
                }finally{
                    try{
                        stream.Dispose();
                    }catch{

                    }
                }
            }

            public int CompareTo(FormatResult other)
            {
                var a = task;
                var b = other?.task;
                if(a == null && b == null) return 0;
                else if(a == null) return 1;
                else if(b == null) return -1;
                var t1 = a.GetType();
                var t2 = b.GetType();
                return t1.IsAssignableFrom(t2) ? 1 : t2.IsAssignableFrom(t1) ? -1 : 0;
            }


            class ImprovisedFormat<T> : BinaryFileFormat<T> where T : class
            {
                public static readonly ImprovisedFormat<T> Instance = new ImprovisedFormat<T>();

                private ImprovisedFormat() : base(0, null, null)
                {

                }

                public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
                {
                    return true;
                }

                public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
                {
                    return true;
                }

                public override string GetMediaType(T value)
                {
                    return DataTools.GetFakeMediaTypeFromType<T>();
                }

                public override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args)
                {
                    throw new NotSupportedException();
                }
            }
        }

        class HeaderLengthComparer : GlobalObjectComparer<IBinaryFileFormat>
        {
            public static readonly IComparer<IBinaryFileFormat> Instance = new HeaderLengthComparer();

            private HeaderLengthComparer()
            {

            }

            protected override int CompareInner(IBinaryFileFormat x, IBinaryFileFormat y)
            {
                return -x.HeaderLength.CompareTo(y.HeaderLength);
            }
        }
    }
}
