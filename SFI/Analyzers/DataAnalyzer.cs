using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Tools.IO;
using IS4.SFI.Vocabulary;
using MorseCode.ITask;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// This analyzer accepts instances of <see cref="IStreamFactory"/> or <see cref="System.Byte"/> arrays.
    /// It uses the available properties to describe properties such as hashes, guess the encoding, and derive more specific data formats.
    /// It produces an instance of <see cref="IDataObject"/> storing the general information derived from the data,
    /// and an instance of <see cref="IBinaryFormatObject{T}"/> for each of the recognized format, for further analysis.
    /// </summary>
    public sealed class DataAnalyzer : EntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>
	{
        /// <summary>
        /// Stores an instance of <see cref="IHashedContentUriFormatter"/> to be used to
        /// assign hash-derived URIs to analyzed data objects. If no formatter is provided,
        /// <see cref="LinkedNodeFactoryExtensions.CreateUnique(ILinkedNodeFactory)"/> is used
        /// to create a unique node.
        /// </summary>
        public IHashedContentUriFormatter? ContentUriFormatter { get; set; }

        /// <summary>
        /// A collection of used hash algorithms, as instances of <see cref="IDataHashAlgorithm"/>,
        /// whose output is used to describe the data object or to create its URI if it is too long.
        /// </summary>
        [ComponentCollection("data-hash")]
        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();

        /// <summary>
        /// A factory of instances of <see cref="IEncodingDetector"/> which is used
        /// to guess the encoding of the data, if stored as text.
        /// </summary>
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }

        /// <summary>
        /// A collection of recognized formats, as instances of <see cref="IBinaryFileFormat"/>.
        /// They are sorted based on descending <see cref="IBinaryFileFormat.HeaderLength"/>.
        /// </summary>
        [ComponentCollection("data-format")]
        public ICollection<IBinaryFileFormat> DataFormats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        /// <summary>
        /// The minimum size at which the data is written to a temporary file on disk
        /// instead of storing in memory, when a seekable stream cannot be produced
        /// otherwise.
        /// </summary>
        public long FileSizeToWriteToDisk { get; set; } = 524288;

        /// <summary>
        /// The minimum size at which to consider storing data directly in a URI
        /// instead of using one of its hashes; see <see cref="GetMaxDataLengthToStore(long)"/>
        /// for details.
        /// </summary>
        public int MinDataLengthToStore { get; set; } = 48;

        /// <summary>
        /// An estimate of the size of a triple in a data store, used in <see cref="GetMaxDataLengthToStore(long)"/>.
        /// </summary>
        public int TripleSizeEstimate { get; set; } = 32;

        /// <summary>
        /// An instance of <see cref="TextWriter"/> to use for logging.
        /// </summary>
        public TextWriter OutputLog { get; set; } = Console.Error;

        /// <summary>
        /// Calculates the maximum allowed length of input above which the node
        /// for the data object should not be identified by the data itself.
        /// The length is not less than <see cref="MinDataLengthToStore"/>.
        /// See remarks for the description of how the estimate is chosen.
        /// </summary>
        /// <param name="dataSize">The size of the input data.</param>
        /// <returns>The minimum length of the input at which using hashes to identify it becomes more efficient.</returns>
        /// <remarks>
        /// <para>
        /// A data object is identified either by its actual content as a "data:" URI,
        /// or through the collection of its hashes as determined by <see cref="HashAlgorithms"/>.
        /// Since there is no need to store the hashes when the data itself is present,
        /// there is a minimum size at which using a "data:" URI becomes inefficient.
        /// This is estimated based on the number of triples required to store one hash,
        /// taken from <see cref="TripleSizeEstimate"/> and <see cref="HashAlgorithm.TriplesPerHash"/>,
        /// the size of URI identifying each has, as returned by <see cref="IHashAlgorithm.EstimateUriSize(int)"/>,
        /// If the <see cref="ContentUriFormatter"/> it specified, <see cref="IHashedContentUriFormatter.EstimateUriSize(IHashAlgorithm, int)"/>
        /// is called to estimate the size of the URI identifying the data using its primary hash.
        /// </para>
        /// <para>
        /// The parameter <paramref name="dataSize"/> is used only when calling <see cref="IHashAlgorithm.GetHashSize(long)"/>,
        /// since some hash algorithms may have a variable hash size depending upon the size of the input.
        /// An estimate of the input size may be provided instead of the precise size.
        /// </para>
        /// </remarks>
        public int GetMaxDataLengthToStore(long dataSize)
        {
            var min = MinDataLengthToStore;
            var hashedSize = 0;
            var formatter = ContentUriFormatter;
            if(formatter != null)
            {
                // The first supported algorithm used by this analyzer will be chosen
                foreach(var algorithm in formatter.SuitableAlgorithms.OfType<IDataHashAlgorithm>())
                {
                    if(HashAlgorithms.Contains(algorithm))
                    {
                        var size = algorithm.GetHashSize(dataSize);
                        if(formatter.EstimateUriSize(algorithm, size) is int uriSize)
                        {
                            hashedSize = uriSize;
                            break;
                        }
                    }
                }
            }

            hashedSize += HashAlgorithms.Sum(h => {
                var hashSize = h.GetHashSize(dataSize);
                // Each hash contributes several triples, its URI, and its value in base64 (a 4/3 increase)
                return HashAlgorithm.TriplesPerHash * TripleSizeEstimate + h.EstimateUriSize(hashSize) + (hashSize + 2) / 3 * 4;
            });

            return Math.Max(min, (hashedSize - "data:;base64,".Length - TripleSizeEstimate) * 3 / 8);
        }

        /// <summary>
        /// Creates a new instance using a factory of <see cref="IEncodingDetector"/>.
        /// </summary>
        /// <param name="encodingDetectorFactory">The factory to use when the detection of input text encoding is needed.</param>
        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(byte[] data, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return Analyze(new MemoryStreamFactory(new ArraySegment<byte>(data), data, null), context, analyzers);
        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return await new DataAnalysis(this, streamFactory, context, analyzers).Match(async match => {
                var node = await match.NodeTask;

                if(node != null)
                {
                    node.SetAsBase();

                    var results = match.Results.Where(result => result.IsValid);

                    foreach(var result in results.GroupBy(r => r.Result))
                    {
                        if(result.Key != null)
                        {
                            match.Recognized = true;
                            node.Set(Properties.HasFormat, result.Key);
                        }
                    }
                }

                return await analyzers.Analyze<IDataObject>(match, context.WithNode(node));
            });
		}

        class DataAnalysis
        {
            readonly DataAnalyzer analyzer;

            readonly int maxDataLengthToStore;
            readonly MemoryStream signatureBuffer;
            readonly IEncodingDetector? encodingDetector;
            readonly IStreamFactory streamFactory;
            readonly AnalysisContext context;
            readonly IEntityAnalyzers analyzers;
            readonly Dictionary<IDataHashAlgorithm, (ChannelWriter<ArraySegment<byte>>? writer, Task<byte[]> data)> hashes = new(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);

            IStreamFactory seekableFactory = null!;
            bool isBinary;

            public DataAnalysis(DataAnalyzer analyzer, IStreamFactory streamFactory, AnalysisContext context, IEntityAnalyzers analyzers)
            {
                this.analyzer = analyzer;
                maxDataLengthToStore = analyzer.GetMaxDataLengthToStore(streamFactory.Length);
                signatureBuffer = new MemoryStream(Math.Max(maxDataLengthToStore + 1, analyzer.DataFormats.Select(fmt => fmt.HeaderLength).DefaultIfEmpty(0).Max()));
                encodingDetector = analyzer.EncodingDetectorFactory?.Invoke();

                this.streamFactory = streamFactory;
                this.context = context.WithMatchContext(c => c.WithServices(this.streamFactory));
                this.analyzers = analyzers;
            }

            public async Task<TResult> Match<TResult>(Func<DataMatch, ValueTask<TResult>> receiver)
            {
                var lazyMatch = new Lazy<Task<DataMatch>>(() => DataMatch.Create(this), false);
            
                long actualLength = 0;

                DataMatch match;

                var tmpPath = default(FileTools.TemporaryFile);
                try{
                    Stream? outputStream = null;
                    try{
                        using var stream = streamFactory.Open();
                        
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

                        var activeHashes = (IEnumerable<(ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)>)hashes.Values.Where(v => v.writer != null);

                        try{
                            int read;
                            while((read = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                var segment = buffer.Slice(0, read);
                                async Task WriteToHasher(ChannelWriter<ArraySegment<byte>> writer)
                                {
                                    await writer.WriteAsync(segment).ConfigureAwait(false);
                                    await writer.WriteAsync(default).ConfigureAwait(false);
                                    await writer.WaitToWriteAsync().ConfigureAwait(false);
                                }
                                var writing = activeHashes.Select(hash => Task.WhenAny(WriteToHasher(hash.writer), hash.data)).ToArray();

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
                                        encodingDetector?.Write(data);
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

                            foreach(var hash in activeHashes)
                            {
                                hash.writer.Complete();
                            }
                        }finally{
                            ArrayPool<byte>.Shared.Return(buffer);
                        }

                        encodingDetector?.End();
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
                    match.ActualLength = actualLength;

                    return await receiver(match);
                }finally{
                    tmpPath.Dispose();
                }
            }

            public class DataMatch : IDataObject
            {
                readonly bool isBinary;
                public bool IsBinary => isBinary || Charset == null;

                public bool IsComplete { get; }

                readonly CharsetMatch? charsetMatch;

                public ArraySegment<byte> ByteValue { get; }

                public string? StringValue { get; }

                public IStreamFactory Source { get; }

                public IStreamFactory StreamFactory { get; }

                public long ActualLength { get; set; }

                public string? Charset => charsetMatch?.Charset;

                public Encoding? Encoding => charsetMatch?.Encoding;

                public bool Recognized { get; set; }

                public IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; }

                public ValueTask<ILinkedNode?> NodeTask { get; }

                public IReadOnlyList<FormatResult> Results { get; private set; }

                readonly Dictionary<IBinaryFormatObject, string?> formats = new(ReferenceEqualityComparer<IBinaryFormatObject>.Default);

                public IReadOnlyDictionary<IBinaryFormatObject, string?> Formats => formats;

                public override string ToString()
                {
                    return $"{(IsBinary ? "Binary" : $"Text ({Charset})")} ({ActualLength} B)";
                }

                private DataMatch(DataAnalysis analysis, IReadOnlyDictionary<IDataHashAlgorithm, byte[]> hashes)
                {
                    Hashes = hashes;

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

                    IsComplete = ByteValue.Count <= analysis.maxDataLengthToStore;

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
                            NodeTask = new ValueTask<ILinkedNode?>(node);
                        }else{
                            async ValueTask<ILinkedNode?> HashNode()
                            {
                                var nodeFactory = context.NodeFactory;
                                var formatter = analyzer.ContentUriFormatter;
                                var hashes = analysis.hashes;
                                if(formatter != null)
                                {
                                    foreach(var algorithm in formatter.SuitableAlgorithms.OfType<IDataHashAlgorithm>())
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
                                return nodeFactory.CreateUnique();
                            }
                            NodeTask = HashNode();
                        }
                    }else{
                        NodeTask = new ValueTask<ILinkedNode?>(node);
                    }

                    if(ByteValue.Count != 0)
                    {
                        var matchingFormats = analyzer.DataFormats.Where(fmt => fmt.CheckHeader(ByteValue, isBinary, encodingDetector));
                        Results = matchingFormats.Select(fmt => new FormatResult(this, StreamFactory, fmt, NodeTask, context, analysis.analyzers)).ToList();
                    }else{
                        Results = Array.Empty<FormatResult>();
                    }
                }

                public static async Task<DataMatch> Create(DataAnalysis analysis)
                {
                    var hashes = new Dictionary<IDataHashAlgorithm, byte[]>(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);
                    var match = new DataMatch(analysis, hashes);

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
                            match.formats[result] = result.Label;
                        }catch(InternalApplicationException)
                        {
                            throw;
                        }catch(Exception e) when(IsFatalFormatException(e))
                        {
                            analysis.analyzer.OutputLog.WriteLine($"{TextTools.GetUserFriendlyName(result.Format.GetType())}: {e.Message}");
                            analysis.analyzer.DataFormats.Remove(result.Format);
                        }catch{

                        }
                    }

                    return match;
                }

                private static bool IsFatalFormatException(Exception e)
                {
                    return
                        e is PlatformNotSupportedException ||
                        e is FileLoadException ||
                        e is FileNotFoundException ||
                        e is TypeLoadException;
                }

                private string? TryGetString(Encoding encoding, ArraySegment<byte> data)
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
            public Encoding? Encoding { get; }
            public string? Charset { get; }
            public double Confidence { get; }

            public CharsetMatch(IEncodingDetector? encodingDetector)
            {
                if(encodingDetector == null) return;
                Confidence = encodingDetector.Confidence;
                if(Confidence > 0)
                {
                    Encoding = TryGetEncoding(encodingDetector.Charset);
                    Charset = Encoding?.WebName ?? encodingDetector.Charset.ToLowerInvariant();
                }
            }

            private Encoding? TryGetEncoding(string charset)
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
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<ILinkedNode?, MatchContext>, IBinaryFormatObject
		{
            readonly DataAnalysis.DataMatch fileMatch;
            public IBinaryFileFormat Format { get; }
            readonly ValueTask<ILinkedNode?> parentTask;
            readonly AnalysisContext context;
            readonly IEntityAnalyzers analyzer;
            readonly Task<ILinkedNode?> task;
            ILinkedNode? taskResult;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => Format.HeaderLength;

            public ILinkedNode? Result => taskResult ?? task?.Result;

            IDataObject IBinaryFormatObject.Data => fileMatch;

            public string? Extension { get; private set; }

            public string? MediaType { get; private set; }

            public string? Label { get; private set; }

            IFileFormat IFormatObject.Format => Format;

            Uri? IUriFormatter<Uri>.this[Uri value] => throw new NotImplementedException();

            public FormatResult(DataAnalysis.DataMatch fileMatch, IStreamFactory streamFactory, IBinaryFileFormat format, ValueTask<ILinkedNode?> parent, AnalysisContext context, IEntityAnalyzers analyzer)
			{
                this.fileMatch = fileMatch;
                this.Format = format;
                this.parentTask = parent;
                this.context = context;
                this.analyzer = analyzer;

                task = StartReading();

                async ValueTask<ILinkedNode?> Reader(Stream stream)
                {
                    var matchContext = this.context.MatchContext;
                    return await format.Match(stream, matchContext, this, matchContext);
                }

                Task<ILinkedNode?> StartReading()
                {
                    async Task<ILinkedNode?> Inner()
                    {
                        var stream = streamFactory.Open();
                        try{
                            taskResult = await Reader(stream);
                        }catch when(taskResult != null)
                        {
                            // An error occurred but Analyze got to run.
                        }finally{
                            try{
                                stream.Dispose();
                            }catch{

                            }
                        }
                        return taskResult;
                    }

                    if(streamFactory.Access == StreamFactoryAccess.Parallel)
                    {
                        return Task.Run(Inner);
                    }else{
                        try{
                            return Inner();
                        }catch(Exception e)
                        {
                            return Task.FromException<ILinkedNode?>(e);
                        }
                    }
                }
            }

            public Task Finish()
            {
                return task;
            }

            async ITask<ILinkedNode?> IResultFactory<ILinkedNode?, MatchContext>.Invoke<T>(T? value, MatchContext matchContext) where T : class
            {
                if(value == null)
                {
                    return null;
                }

                var streamContext = context.WithMatchContext(matchContext);
                var parent = await parentTask;
                try{
                    var formatObj = new BinaryFormatObject<T>(fileMatch, Format, value);
                    Extension = formatObj.Extension;
                    MediaType = formatObj.MediaType;
                    var result = await analyzer.Analyze(formatObj, streamContext.WithParent(parent));
                    taskResult = result.Node;
                    Label = result.Label;
                    return result.Node;
                }catch(Exception e)
                {
                    throw new InternalApplicationException(e);
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

            ValueTask<TResult> IFormatObject.GetValue<TResult, TArgs>(IResultFactory<TResult, TArgs> resultFactory, TArgs args)
            {
                throw new NotImplementedException();
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
