﻿using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Tools.IO;
using Microsoft.Extensions.Logging;
using MorseCode.ITask;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Description("An analyzer of arbitrary data, using the 'data-format' and 'data-hash' collections to describe it.")]
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
        [Description("A collection of used hash algorithms whose output is used to describe the data object or to create its URI if it is too long.")]
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
        [Description("A collection of recognized formats.")]
        public ICollection<IBinaryFileFormat> DataFormats { get; } = new SortedMultiSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        /// <summary>
        /// The minimum size at which the data is written to a temporary file on disk
        /// instead of storing in memory, when a seekable stream cannot be produced
        /// otherwise.
        /// </summary>
        [Description("The minimum size at which the data is written to a temporary file on disk instead of storing in memory, when a seekable stream cannot be produced otherwise.")]
        public long FileSizeToWriteToDisk { get; set; } = 524288;

        /// <summary>
        /// The minimum size at which to consider storing data directly in a URI
        /// instead of using one of its hashes; see <see cref="GetMaxDataLengthToStore(long)"/>
        /// for details.
        /// </summary>
        [Description("The minimum size at which to consider storing data directly in a URI instead of using one of its hashes.")]
        public int MinDataLengthToStore { get; set; } = 48;

        /// <summary>
        /// An estimate of the size of a triple in a data store, used in <see cref="GetMaxDataLengthToStore(long)"/>.
        /// </summary>
        [Description("An estimate of the size of a triple in a data store.")]
        public int TripleSizeEstimate { get; set; } = 32;

        /// <summary>
        /// The maximum depth the data is allowed to be as an entity in a hierarchy
        /// in order to attempt to analyze formats.
        /// </summary>
        [Description("The maximum depth the data is allowed to be as an entity in a hierarchy in order to attempt to analyze formats.")]
        public int? MaxDepthForFormats { get; set; } = 200;

        /// <summary>
        /// An instance of <see cref="ILogger"/> to use for logging.
        /// </summary>
        public ILogger? OutputLog { get; set; }

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
        /// A data object is identified either by its actual content as a <c>data:</c> URI,
        /// or through the collection of its hashes as determined by <see cref="HashAlgorithms"/>.
        /// Since there is no need to store the hashes when the data itself is present,
        /// there is a minimum size at which using a <c>data:</c> URI becomes inefficient.
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
                // Node is initialized from the task.
                return await analyzers.Analyze<IDataObject>(match, context.WithNode(node).AsInitialized());
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
                this.context = context.WithMatchContext(c => c.WithServices(this.streamFactory).WithService(encodingDetector));
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
                                if(hash.Accepts(streamFactory))
                                {
                                    var queue = ChannelArrayStream.Create(out var writer, 1);
                                    hashes[hash] = (writer, Task.Run(async () => await hash.ComputeHash(queue, streamFactory)));
                                }
                            }
                        }else{
                            seekableFactory = streamFactory;
                            
                            foreach(var hash in analyzer.HashAlgorithms)
                            {
                                if(hash.Accepts(streamFactory))
                                {
                                    hashes[hash] = (null, Task.Run(async () => {
                                        using var hashStream = streamFactory.Open();
                                        return await hash.ComputeHash(hashStream, streamFactory);
                                    }));
                                }
                            }
                        }

                        bool? couldBeUnicode = null;

                        using var bufferLease = ArrayPool<byte>.Shared.Rent(16384, out var buffer);

                        var activeHashes = (IEnumerable<(ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)>)hashes.Values.Where(v => v.writer != null);

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
                                couldBeUnicode = DataTools.FindBom(buffer.AsSpan(0, read)) > 0;
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

                        foreach(var (writer, _) in activeHashes)
                        {
                            writer.Complete();
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

                public IReadOnlyDictionary<IDataHashAlgorithm, byte[]> Hashes { get; }

                public ValueTask<ILinkedNode?> NodeTask { get; }

                public IReadOnlyList<FormatResult> Results { get; private set; }

                readonly ConcurrentDictionary<IBinaryFormatObject, AnalysisResult> formats = new(ReferenceEqualityComparer<IBinaryFormatObject>.Default);

                public IReadOnlyDictionary<IBinaryFormatObject, AnalysisResult> Formats => formats;

                public bool IsPlain { get; private set; }

                static readonly Type DataMatchType = typeof(DataMatch);

                public object? ReferenceKey => IsComplete ? DataMatchType : this;

                public object? DataKey => IsComplete ? DataTools.GetStringKey(ByteValue) : "";

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
                            StringValue = charsetMatch.Encoding.TryGetString(ByteValue);

                            if(IsComplete && StringValue == null)
                            {
                                // the file is corrupted, revert to binary
                                isBinary = true;
                            }
                        }
                    }

                    var node = context.Node;

                    if(node != null)
                    {
                        NodeTask = new ValueTask<ILinkedNode?>(analysis.analyzer.InitNewNode(node, context));
                    }else if(IsComplete)
                    {
                        node = context.NodeFactory.Create(UriTools.DataUriFormatter, (default(string), charsetMatch?.Charset, ByteValue));
                        NodeTask = new ValueTask<ILinkedNode?>(analysis.analyzer.InitNewNode(node, context));
                    }else{
                        async ValueTask<ILinkedNode?> HashNode()
                        {
                            var nodeFactory = context.NodeFactory;
                            var formatter = analyzer.ContentUriFormatter;
                            var hashes = analysis.hashes;
                            ILinkedNode? node = null;
                            if(formatter != null)
                            {
                                foreach(var algorithm in formatter.SuitableAlgorithms.OfType<IDataHashAlgorithm>())
                                {
                                    if(hashes.TryGetValue(algorithm, out var info))
                                    {
                                        var hashNode = nodeFactory.Create(formatter, (algorithm, await info.data, IsBinary));
                                        if(hashNode != null)
                                        {
                                            node = hashNode;
                                            break;
                                        }
                                    }
                                }
                            }
                            node ??= nodeFactory.CreateUnique();
                            return analysis.analyzer.InitNewNode(node, context);
                        }
                        NodeTask = HashNode();
                    }

                    if(ByteValue.Count != 0 && !(analysis.analyzer.MaxDepthForFormats is int maxDepth && (analysis.context.Depth < 0 || analysis.context.Depth > maxDepth)))
                    {
                        List<IBinaryFileFormat>? removedFormats = null;
                        List<FormatResult>? formatResults = null;
                        foreach(var format in analyzer.DataFormats)
                        {
                            using var scope = analyzer.OutputLog?.BeginScope(format);
                            bool matched;
                            try{
                                matched = format.CheckHeader(ByteValue, isBinary, encodingDetector);
                            }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                            {
                                analyzer.OutputLog?.LogError(e, $"Exception calling {nameof(format.CheckHeader)} on a format.");
                                if(IsFatalComponentException(format, e))
                                {
                                    (removedFormats ??= new()).Add(format);
                                }
                                continue;
                            }
                            if(matched)
                            {
                                var result = new FormatResult(this, StreamFactory, format, NodeTask, context, analysis.analyzers);
                                (formatResults ??= new()).Add(result);
                            }
                        }
                        Results = (IReadOnlyList<FormatResult>?)formatResults ?? Array.Empty<FormatResult>();
                        if(removedFormats != null)
                        {
                            foreach(var removed in removedFormats)
                            {
                                analyzer.DataFormats.Remove(removed);
                            }
                        }
                    }else{
                        Results = Array.Empty<FormatResult>();
                        IsPlain = true;
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
                            if(result.Result is AnalysisResult analysisResult)
                            {
                                match.formats[result] = analysisResult;
                            }
                        }catch(InternalApplicationException)
                        {
                            throw;
                        }catch(Exception e) when(IsFatalComponentException(result.Format, e))
                        {
                            using var scope = analysis.analyzer.OutputLog?.BeginScope(result.Format);
                            analysis.analyzer.OutputLog?.LogError(e, $"Exception calling {nameof(result.Format.Match)} on a format.");
                            analysis.analyzer.DataFormats.Remove(result.Format);
                        }catch{

                        }
                    }

                    return match;
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
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<AnalysisResult?, MatchContext>, IBinaryFormatObject
		{
            readonly DataAnalysis.DataMatch fileMatch;
            public IBinaryFileFormat Format { get; }
            readonly ValueTask<ILinkedNode?> parentTask;
            readonly AnalysisContext context;
            readonly IEntityAnalyzers analyzer;
            readonly Task<AnalysisResult?> task;
            AnalysisResult? taskResult;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => Format.HeaderLength;

            public AnalysisResult? Result => taskResult ?? task?.Result;

            IDataObject IBinaryFormatObject.Data => fileMatch;

            public string? Extension { get; private set; }

            public string? MediaType { get; private set; }

            public string? Label { get; private set; }

            IFileFormat IFormatObject.Format => Format;

            Uri? IUriFormatter<Uri>.this[Uri value] => throw new NotImplementedException();

            object? IIdentityKey.ReferenceKey => fileMatch.ReferenceKey;

            object? IIdentityKey.DataKey => (Format, fileMatch.DataKey);

            public FormatResult(DataAnalysis.DataMatch fileMatch, IStreamFactory streamFactory, IBinaryFileFormat format, ValueTask<ILinkedNode?> parent, AnalysisContext context, IEntityAnalyzers analyzer)
			{
                this.fileMatch = fileMatch;
                this.Format = format;
                this.parentTask = parent;
                this.context = context;
                this.analyzer = analyzer;

                task = StartReading();

                async ValueTask<AnalysisResult?> Reader(Stream stream)
                {
                    var matchContext = this.context.MatchContext;
                    return await format.Match(stream, matchContext, this, matchContext);
                }

                Task<AnalysisResult?> StartReading()
                {
                    async Task<AnalysisResult?> Inner()
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
                            return Task.FromException<AnalysisResult?>(e);
                        }
                    }
                }
            }

            public Task Finish()
            {
                return task;
            }

            async ITask<AnalysisResult?> IResultFactory<AnalysisResult?, MatchContext>.Invoke<T>(T? value, MatchContext matchContext) where T : class
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
                    return taskResult = await analyzer.Analyze(formatObj, streamContext.WithParent(parent));
                }catch(InternalApplicationException)
                {
                    throw;
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
                throw new NotSupportedException();
            }
        }

        class HeaderLengthComparer : IComparer<IBinaryFileFormat>
        {
            public static readonly IComparer<IBinaryFileFormat> Instance = new HeaderLengthComparer();

            private HeaderLengthComparer()
            {

            }

            public int Compare(IBinaryFileFormat x, IBinaryFileFormat y)
            {
                return -x.HeaderLength.CompareTo(y.HeaderLength);
            }
        }
    }
}
