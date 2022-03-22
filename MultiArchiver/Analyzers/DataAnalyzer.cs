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

        public TextWriter OutputLog { get; set; } = Console.Error;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

        public ValueTask<AnalysisResult> Analyze(byte[] data, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            return Analyze(new MemoryStreamFactory(new ArraySegment<byte>(data), data, null), context, analyzers);
        }

        public async ValueTask<AnalysisResult> Analyze(IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var match = await new DataAnalysis(this, streamFactory, context, analyzers).Match();
            var node = await match.NodeTask;

            node.SetAsBase();

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

            public async Task<DataMatch> Match()
            {
                var lazyMatch = new Lazy<Task<DataMatch>>(() => DataMatch.Create(this), false);
            
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
                                    hashes[hash] = (writer, Task.Run(async () => await hash.ComputeHash(queue, streamFactory)));
                                }
                            }else{
                                seekableFactory = streamFactory;
                            
                                foreach(var hash in analyzer.HashAlgorithms)
                                {
                                    hashes[hash] = (null, Task.Run(async () => {
                                        using(var hashStream = streamFactory.Open())
                                        {
                                            return await hash.ComputeHash(hashStream, streamFactory);
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

                    match = await lazyMatch.Value;
                }finally{
                    tmpPath.Dispose();
                }

                match.ActualLength = actualLength;

                return match;
            }

            public class DataMatch : IDataObject
            {
                readonly bool isBinary;
                public bool IsBinary => isBinary || Charset == null;

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

                public IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; private set; }

                public ValueTask<ILinkedNode> NodeTask { get; }

                public IReadOnlyList<FormatResult> Results { get; private set; }

                public override string ToString()
                {
                    return $"{(IsBinary ? "Binary" : $"Text ({Charset})")} ({ActualLength} B)";
                }

                private DataMatch(DataAnalysis analysis)
                {
                    Source = analysis.streamFactory;

                    StreamFactory = analysis.seekableFactory;

                    ByteValue = analysis.signatureBuffer.GetData();

                    isBinary = analysis.isBinary;

                    if(ByteValue.Count == 0)
                    {
                        // empty file is always binary
                        isBinary = true;
                    }

                    var analyzer = analysis.analyzer;
                    var context = analysis.context;
                    var encodingDetector = analysis.encodingDetector;

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

                    var node = context.Node;

                    if(node == null)
                    {
                        if(IsComplete)
                        {
                            node = context.NodeFactory.Create(UriTools.DataUriFormatter, (null, charsetMatch?.Charset, ByteValue));
                            NodeTask = new ValueTask<ILinkedNode>(node);
                        }else{
                            async ValueTask<ILinkedNode> HashNode()
                            {
                                var nodeFactory = context.NodeFactory;
                                var formatter = analyzer.ContentUriFormatter;
                                var hashes = analysis.hashes;
                                if(formatter != null)
                                {
                                    foreach(var algorithm in formatter.SupportedAlgorithms.OfType<IDataHashAlgorithm>())
                                    {
                                        if(hashes.TryGetValue(algorithm, out var info))
                                        {
                                            var hashNode = nodeFactory.Create(formatter, (algorithm, await info.data, IsBinary));
                                            if(hashNode != null)
                                            {
                                                return hashNode;
                                            }
                                        }
                                    }
                                }
                                return nodeFactory.NewGuidNode();
                            }
                            NodeTask = HashNode();
                        }
                    }else{
                        NodeTask = new ValueTask<ILinkedNode>(node);
                    }

                    if(ByteValue.Count != 0)
                    {
                        var nodeCreated = new TaskCompletionSource<ILinkedNode>();
                        var matchingFormats = analyzer.DataFormats.Where(fmt => fmt.CheckHeader(ByteValue, isBinary, encodingDetector));
                        Results = matchingFormats.Select(fmt => new FormatResult(this, StreamFactory, fmt, nodeCreated, NodeTask, context, analysis.analyzers)).ToList();
                    }else{
                        Results = Array.Empty<FormatResult>();
                    }
                }

                public static async Task<DataMatch> Create(DataAnalysis analysis)
                {
                    var match = new DataMatch(analysis);

                    var hashes = new Dictionary<IDataHashAlgorithm, byte[]>(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);

                    match.Hashes = hashes;

                    if(!match.IsComplete)
                    {
                        foreach(var (hash, (_, task)) in analysis.hashes)
                        {
                            hashes[hash] = await task;
                        }
                    }

                    foreach(var result in match.Results)
                    {
                        try{
                            await result.Finish();
                        }catch(InternalArchiverException)
                        {
                            throw;
                        }catch(PlatformNotSupportedException e)
                        {
                            analysis.analyzer.OutputLog.WriteLine($"{result.Format.GetType().Name}: {e.Message}");
                            analysis.analyzer.DataFormats.Remove(result.Format);
                        }catch{

                        }
                    }

                    return match;
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
                    Charset = Encoding?.WebName ?? encodingDetector.Charset.ToLowerInvariant();
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
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<ILinkedNode, MatchContext>
		{
            readonly DataAnalysis.DataMatch fileMatch;
            public IBinaryFileFormat Format { get; }
            readonly ValueTask<ILinkedNode> parentTask;
            readonly AnalysisContext context;
            readonly IEntityAnalyzerProvider analyzer;
            readonly Task<ILinkedNode> task;
            readonly TaskCompletionSource<ILinkedNode> nodeCreated;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => Format.HeaderLength;

            public string Extension { get; private set; }
            public string Label { get; private set; }

            public ILinkedNode Result => task?.Result;

            public FormatResult(DataAnalysis.DataMatch fileMatch, IStreamFactory streamFactory, IBinaryFileFormat format, TaskCompletionSource<ILinkedNode> nodeCreated, ValueTask<ILinkedNode> parent, AnalysisContext context, IEntityAnalyzerProvider analyzer)
			{
                this.fileMatch = fileMatch;
                this.Format = format;
                this.parentTask = parent;
                this.context = context;
                this.analyzer = analyzer;
                this.nodeCreated = nodeCreated;

                task = StartReading();

                async ValueTask<ILinkedNode> Reader(Stream stream)
                {
                    var streamContext = this.context.MatchContext.WithStream(stream);
                    return await format.Match(stream, streamContext, this, streamContext);
                }

                Task<ILinkedNode> StartReading()
                {
                    async Task<ILinkedNode> Inner()
                    {
                        var stream = streamFactory.Open();
                        try{
                            return await Reader(stream);
                        }finally{
                            try{
                                stream.Dispose();
                            }catch{

                            }
                        }
                    }

                    if(streamFactory.Access == StreamFactoryAccess.Parallel)
                    {
                        return Task.Run(Inner);
                    }else{
                        try{
                            return Inner();
                        }catch(Exception e)
                        {
                            return Task.FromException<ILinkedNode>(e);
                        }
                    }
                }
            }

            public Task Finish()
            {
                return task;
            }

            const int MaxResultWaitTime = 1000;

            async ITask<ILinkedNode> IResultFactory<ILinkedNode, MatchContext>.Invoke<T>(T value, MatchContext matchContext)
            {
                var streamContext = context.WithMatchContext(matchContext);
                var parent = await parentTask;
                try{
                    var formatObj = new BinaryFormatObject<T>(fileMatch, Format, value);
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
