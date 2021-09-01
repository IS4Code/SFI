using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Tools.IO;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>
	{
        public IDataHashAlgorithm PrimaryHash { get; set; }

        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }

        public ICollection<IBinaryFileFormat> DataFormats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        public int FileSizeToWriteToDisk { get; set; } = 524288;

        public int MaxDataLengthToStore => Math.Max(64, HashAlgorithms.Sum(h => h.HashSize + 64)) - 16;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public AnalysisResult Analyze(IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var match = GetFileMatch(streamFactory, context, analyzers);
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

            return analyzers.Analyze<IDataObject>(match, context.WithNode(node));
		}

        FileMatch GetFileMatch(IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var signatureBuffer = new MemoryStream(Math.Max(MaxDataLengthToStore + 1, DataFormats.Count == 0 ? 0 : DataFormats.Max(fmt => fmt.HeaderLength)));

            var encodingDetector = EncodingDetectorFactory?.Invoke();

            var isBinary = false;

            IStreamFactory seekableFactory = null;

            var hashes = new Dictionary<IDataHashAlgorithm, (ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)>(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);

            context = context.WithMatchContext(c => c.WithServices(streamFactory));

            var lazyMatch = new Lazy<FileMatch>(() => new FileMatch(DataFormats, streamFactory, seekableFactory, signatureBuffer, MaxDataLengthToStore, encodingDetector, isBinary, PrimaryHash != null && hashes.TryGetValue(PrimaryHash, out var primaryHash) ? (PrimaryHash, primaryHash.data) : default, context, analyzers), false);
            
            long actualLength = 0;

            FileMatch match;

            var tmpPath = default(FileTools.TemporaryFile);
            try{
                Stream outputStream = null;
                try{
                    using(var stream = streamFactory.Open())
                    {
                        if(streamFactory.Access != StreamFactoryAccess.Parallel || !stream.CanSeek)
                        {
                            if(streamFactory.Length >= FileSizeToWriteToDisk)
                            {
                                tmpPath = FileTools.GetTemporaryFile("b");
                                outputStream = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                            }else{
                                outputStream = new MemoryStream();
                            }
                            
                            foreach(var hash in HashAlgorithms)
                            {
                                var queue = ChannelArrayStream.Create(out var writer, 1);
                                hashes[hash] = (writer, Task.Run(() => hash.ComputeHash(queue, streamFactory)));
                            }
                        }else{
                            seekableFactory = streamFactory;
                            
                            foreach(var hash in HashAlgorithms)
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
                            while((read = stream.Read(buffer, 0, buffer.Length)) != 0)
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

                                Task.WaitAll(writing);
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

                foreach(var result in match.Results)
                {
                    result.Wait();
                }
            }finally{
                tmpPath.Dispose();
            }

            match.ActualLength = actualLength;

            if(!match.IdentifyWithData)
            {
                foreach(var hash in hashes)
                {
                    match[hash.Key] = hash.Value.data.Result;
                }
            }

            return match;
        }

        class FileMatch : Dictionary<IDataHashAlgorithm, byte[]>, IDataObject, IIndividualUriFormatter<bool>
        {
            readonly bool isBinary;
            public bool IsBinary => isBinary || charsetMatch?.Charset == null;
            public IReadOnlyList<FormatResult> Results { get; }
            public ILinkedNode Node { get; }
            public bool IdentifyWithData { get; }

            readonly Lazy<CharsetMatch> lazyCharsetMatch;
            CharsetMatch charsetMatch => lazyCharsetMatch?.Value;

            public ArraySegment<byte> Signature { get; }

            public IStreamFactory Source { get; }

            public IStreamFactory StreamFactory { get; }

            public long ActualLength { get; set; }

            public string Charset => charsetMatch.Charset;

            public bool Recognized { get; set; }

            IReadOnlyDictionary<IDataHashAlgorithm, byte[]> IDataObject.Hashes => this;

            public FileMatch(ICollection<IBinaryFileFormat> formats, IStreamFactory source, IStreamFactory streamFactory, MemoryStream signatureBuffer, int maxDataLength, IEncodingDetector encodingDetector, bool isBinary, (IDataHashAlgorithm algorithm, Task<byte[]> result) primaryHash, AnalysisContext context, IEntityAnalyzerProvider analyzer)
            {
                Source = source;

                StreamFactory = streamFactory;

                Signature = signatureBuffer.GetData();

                if(Signature.Count == 0)
                {
                    isBinary = true;
                }

                IdentifyWithData = Signature.Count <= maxDataLength;

                if(!isBinary)
                {
                    lazyCharsetMatch = new Lazy<CharsetMatch>(() => new CharsetMatch(encodingDetector), false);
                }

                Node = context.Node;

                if(Node == null)
                {
                    if(IdentifyWithData)
                    {
                        string strval = null;
                        if(!isBinary)
                        {
                            strval = TryGetString(charsetMatch.Encoding, Signature);
                            if(strval == null)
                            {
                                lazyCharsetMatch = null;
                                isBinary = true;
                            }
                        }

                        Node = context.NodeFactory.Create(this, false);

                        if(isBinary)
                        {
                            Node.Set(Properties.Bytes, Signature.ToBase64String(), Datatypes.Base64Binary);
                        }else{
                            Node.Set(Properties.Chars, DataTools.ReplaceControlCharacters(strval, charsetMatch.Encoding), Datatypes.String);
                        }

                        streamFactory = new MemoryStreamFactory(Signature, streamFactory);
                    }else{
                        if(primaryHash.algorithm?.NumericIdentifier is int id)
                        {
                            var hashBytes = primaryHash.result.Result;

                            var identifier = new List<byte>(2 + hashBytes.Length);
                            identifier.AddRange(DataTools.Varint((ulong)id));
                            identifier.AddRange(DataTools.Varint((ulong)hashBytes.Length));
                            identifier.AddRange(hashBytes);

                            var sb = new StringBuilder();
                            DataTools.Base58(identifier, sb);

                            Node = context.NodeFactory.Create(Vocabularies.Ad, sb.ToString());
                        }else{
                            Node = context.NodeFactory.NewGuidNode();
                        }
                    }
                }

                this.isBinary = isBinary;

                if(Signature.Count > 0)
                {
                    var nodeCreated = new TaskCompletionSource<ILinkedNode>();
                    Results = formats.Where(fmt => fmt.CheckHeader(Signature, isBinary, encodingDetector)).Select(fmt => new FormatResult(streamFactory, fmt, nodeCreated, Node, context, analyzer)).ToList();
                }else{
                    Results = Array.Empty<FormatResult>();
                }
            }

            Uri IUriFormatter<bool>.this[bool _] {
                get {
                    string base64Encoded = ";base64," + Signature.ToBase64String();
                    string uriEncoded = "," + UriTools.EscapeDataBytes(Signature);

                    string data = uriEncoded.Length <= base64Encoded.Length ? uriEncoded : base64Encoded;

                    switch(charsetMatch?.Charset.ToLowerInvariant())
                    {
                        case null:
                            return new Uri("data:application/octet-stream" + data, UriKind.Absolute);
                        case "ascii":
                        case "us-ascii":
                            return new EncodedUri("data:" + data, UriKind.Absolute);
                        default:
                            return new EncodedUri("data:;charset=" + charsetMatch.Charset + data, UriKind.Absolute);
                    }
                }
            }

            private string TryGetString(Encoding encoding, ArraySegment<byte> data)
            {
                if(encoding == null) return null;
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

		public AnalysisResult Analyze(byte[] data, AnalysisContext context, IEntityAnalyzerProvider analyzers)
		{
			return Analyze(new MemoryStreamFactory(new ArraySegment<byte>(data), data, null), context, analyzers);
		}
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<ILinkedNode, (Stream stream, MatchContext matchContext)>
		{
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

            public FormatResult(IStreamFactory streamFactory, IBinaryFileFormat format, TaskCompletionSource<ILinkedNode> nodeCreated, ILinkedNode parent, AnalysisContext context, IEntityAnalyzerProvider analyzer)
			{
                this.format = format;
                this.parent = parent;
                this.context = context;
                this.analyzer = analyzer;
                this.streamFactory = streamFactory;
                this.nodeCreated = nodeCreated;
                task = StartReading(streamFactory, Reader);
            }

            private ILinkedNode Reader(Stream stream)
            {
                var streamContext = context.MatchContext.WithStream(stream);
                return format.Match(stream, streamContext, this, (stream, streamContext));
            }

            public void Wait()
            {
                Task.WaitAny(task);
                if(task.IsFaulted)
                {
                    var rethrowable = task.Exception.InnerExceptions.OfType<InternalArchiverException>().Select(e => e.InnerException);
                    var exc = new AggregateException(rethrowable);
                    switch(exc.InnerExceptions.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            ExceptionDispatchInfo.Capture(exc.InnerException).Throw();
                            throw null;
                        default:
                            throw exc;
                    }
                }
            }

            const int MaxResultWaitTime = 1000;

            ILinkedNode IResultFactory<ILinkedNode, (Stream stream, MatchContext matchContext)>.Invoke<T>(T value, (Stream stream, MatchContext matchContext) args)
            {
                var (stream, matchContext) = args;
                var streamContext = context.WithMatchContext(matchContext);
                try{
                    var formatObj = new FormatObject<T>(format, value);
                    while(parent[formatObj] == null)
                    {
                        ILinkedNode node;
                        if(!nodeCreated.Task.Wait(MaxResultWaitTime))
                        {
                            formatObj = new FormatObject<T>(ImprovisedFormat<T>.Instance, value);
                            continue;
                        }else{
                            node = nodeCreated.Task.Result;
                        }
                        if(node != null)
                        {
                            var result = analyzer.Analyze(value, streamContext.WithNode(node));
                            node = result.Node;
                        }
                        return node;
                    }
                    {
                        var result = analyzer.Analyze(formatObj, streamContext.WithParent(parent));
                        nodeCreated?.TrySetResult(result.Node);
                        return result.Node;
                    }
                }catch(Exception e)
                {
                    throw new InternalArchiverException(e);
                }
            }

            private Task<ILinkedNode> StartReading(IStreamFactory streamFactory, Func<Stream, ILinkedNode> reader)
            {
                if(streamFactory.Access == StreamFactoryAccess.Parallel)
                {
                    return Task.Run(() => StartReadingInner(streamFactory, reader));
                }else{
                    try{
                        return Task.FromResult(StartReadingInner(streamFactory, reader));
                    }catch(Exception e)
                    {
                        return Task.FromException<ILinkedNode>(e);
                    }
                }
            }

            private ILinkedNode StartReadingInner(IStreamFactory streamFactory, Func<Stream, ILinkedNode> reader)
            {
                var stream = streamFactory.Open();
                try{
                    return reader(stream);
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

                public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args)
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
