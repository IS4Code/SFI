using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Tools.IO;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ICollection<IBinaryFileFormat> Formats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        public int MaxDataLengthToStore => Math.Max(64, HashAlgorithms.Sum(h => h.HashSize + 64)) - 16;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

        const int fileSizeToWriteToDisk = 524288;

		public ILinkedNode Analyze(ILinkedNode parent, IStreamFactory streamFactory, ILinkedNodeFactory nodeFactory)
        {
            var signatureBuffer = new MemoryStream(Math.Max(MaxDataLengthToStore + 1, Formats.Count == 0 ? 0 : Formats.Max(fmt => fmt.HeaderLength)));

            var encodingDetector = EncodingDetectorFactory?.Invoke();

            var isBinary = false;

            IStreamFactory seekableFactory = null;

            var hashes = new Dictionary<IDataHashAlgorithm, (ChannelWriter<ArraySegment<byte>> writer, Task<byte[]> data)>(ReferenceEqualityComparer<IDataHashAlgorithm>.Default);

            var lazyMatch = new Lazy<FileMatch>(() => new FileMatch(Formats, streamFactory, seekableFactory, signatureBuffer, MaxDataLengthToStore, encodingDetector, isBinary, PrimaryHash != null && hashes.TryGetValue(PrimaryHash, out var primaryHash) ? (PrimaryHash, primaryHash.data) : default, nodeFactory), false);
            
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
                            if(streamFactory.Length >= fileSizeToWriteToDisk)
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
                                var segment = new ArraySegment<byte>(buffer, 0, read);
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
                                    var data = new ArraySegment<byte>(buffer, 0, read);
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
                    if(!memoryStream.TryGetBuffer(out var buffer))
                    {
                        buffer = new ArraySegment<byte>(memoryStream.ToArray());
                    }
                    seekableFactory = new MemoryStreamFactory(buffer, streamFactory);
                }

                match = lazyMatch.Value;

                isBinary |= match.IsBinary;

                foreach(var result in match.Results)
                {
                    result.Wait();
                }
            }finally{
                tmpPath.Dispose();
            }

            var node = match.Node;

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, match.CharsetMatch.Charset);
            }

            var sizeSuffix = DataTools.SizeSuffix(actualLength, 2);

            var label = $"{(isBinary ? "binary data" : "text")} ({sizeSuffix})";

            node.Set(Properties.PrefLabel, label, "en");

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, actualLength, Datatypes.Byte);

            if(!match.IdentifyWithData)
            {
                foreach(var hash in hashes)
                {
                    HashAlgorithm.AddHash(node, hash.Key, hash.Value.data.Result, nodeFactory);
                }
            }

            var results = match.Results.Where(result => result.IsValid);

            var any = false;

            foreach(var result in results.GroupBy(r => r.Result))
            {
                if(result.Key != null)
                {
                    any = true;

                    result.Key.Set(Properties.HasFormat, node);

                    var extension = result.Select(r => r.Extension).FirstOrDefault(e => e != null);
                    if(extension != null)
                    {
                        var formatLabel = result.Select(r => r.Label).FirstOrDefault(l => l != null);
                        result.Key.Set(Properties.PrefLabel, $"{extension.ToUpperInvariant()} object ({formatLabel ?? sizeSuffix})", "en");
                    }
                }
            }

            if(!any && isBinary && DataTools.ExtractSignature(match.Signature) is string magicText)
            {
                var signatureFormat = new ImprovisedSignatureFormat.Format(magicText);
                var formatObj = new FormatObject<ImprovisedSignatureFormat.Format, IBinaryFileFormat>(ImprovisedSignatureFormat.Instance, signatureFormat, streamFactory);
                var formatNode = nodeFactory.Create<IFormatObject<ImprovisedSignatureFormat.Format, IBinaryFileFormat>>(node, formatObj);
                if(formatNode != null)
                {
                    formatNode.Set(Properties.HasFormat, node);
                    formatNode.Set(Properties.PrefLabel, $"{magicText} object ({sizeSuffix})", "en");
                }
            }

			return node;
		}

        class FileMatch : IIndividualUriFormatter<bool>
        {
            readonly bool isBinary;
            public bool IsBinary => isBinary || CharsetMatch?.Charset == null;
            public IReadOnlyList<FormatResult> Results { get; }
            public ILinkedNode Node { get; }
            public bool IdentifyWithData { get; }

            readonly Lazy<CharsetMatch> LazyCharsetMatch;

            public CharsetMatch CharsetMatch => LazyCharsetMatch?.Value;

            public ArraySegment<byte> Signature { get; }

            public FileMatch(ICollection<IBinaryFileFormat> formats, object source, IStreamFactory streamFactory, MemoryStream signatureBuffer, int maxDataLength, IEncodingDetector encodingDetector, bool isBinary, (IDataHashAlgorithm algorithm, Task<byte[]> result) primaryHash, ILinkedNodeFactory nodeFactory)
            {
                if(!signatureBuffer.TryGetBuffer(out var signature))
                {
                    signature = new ArraySegment<byte>(signatureBuffer.ToArray());
                }
                Signature = signature;

                if(Signature.Count == 0)
                {
                    isBinary = true;
                }

                IdentifyWithData = Signature.Count <= maxDataLength;

                if(!isBinary)
                {
                    LazyCharsetMatch = new Lazy<CharsetMatch>(() => new CharsetMatch(encodingDetector), false);
                }

                if(IdentifyWithData)
                {
                    string strval = null;
                    if(!isBinary)
                    {
                        strval = TryGetString(CharsetMatch.Encoding, signature);
                        if(strval == null)
                        {
                            LazyCharsetMatch = null;
                            isBinary = true;
                        }
                    }

                    Node = nodeFactory.Create(this, false);

                    if(isBinary)
                    {
                        Node.Set(Properties.Bytes, Convert.ToBase64String(signature.Array, signature.Offset, signature.Count), Datatypes.Base64Binary);
                    }else{
                        Node.Set(Properties.Chars, DataTools.ReplaceControlCharacters(strval, CharsetMatch.Encoding), Datatypes.String);
                    }

                    streamFactory = new MemoryStreamFactory(signature, streamFactory);
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

                        Node = nodeFactory.Create(Vocabularies.Ad, sb.ToString());
                    }else{
                        Node = nodeFactory.NewGuidNode();
                    }
                }

                this.isBinary = isBinary;

                if(Signature.Count > 0)
                {
                    var nodeCreated = new TaskCompletionSource<ILinkedNode>();
                    Results = formats.Where(fmt => fmt.CheckHeader(signature, isBinary, encodingDetector)).Select(fmt => new FormatResult(streamFactory, source, fmt, nodeCreated, Node, nodeFactory)).ToList();
                }else{
                    Results = Array.Empty<FormatResult>();
                }
            }

            Uri IUriFormatter<bool>.FormatUri(bool _)
            {
                string base64Encoded = ";base64," + Convert.ToBase64String(Signature.Array, Signature.Offset, Signature.Count);
                string uriEncoded = "," + UriTools.EscapeDataBytes(Signature.Array, Signature.Offset, Signature.Count);

                string data = uriEncoded.Length <= base64Encoded.Length ? uriEncoded : base64Encoded;

                switch(CharsetMatch?.Charset.ToLowerInvariant())
                {
                    case null:
                        return new Uri("data:application/octet-stream" + data, UriKind.Absolute);
                    case "ascii":
                    case "us-ascii":
                        return new EncodedUri("data:" + data, UriKind.Absolute);
                    default:
                        return new EncodedUri("data:;charset=" + CharsetMatch.Charset + data, UriKind.Absolute);
                }
            }

            private string TryGetString(Encoding encoding, ArraySegment<byte> data)
            {
                if(encoding == null) return null;
                try{
                    var preamble = encoding.GetPreamble();
                    if(preamble?.Length > 0 && data.AsSpan().StartsWith(preamble))
                    {
                        return encoding.GetString(data.Array, data.Offset + preamble.Length, data.Count - preamble.Length);
                    }
                    return encoding.GetString(data.Array, data.Offset, data.Count);
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

		public ILinkedNode Analyze(ILinkedNode parent, byte[] data, ILinkedNodeFactory analyzer)
		{
			return Analyze(parent, new MemoryStreamFactory(new ArraySegment<byte>(data), data, null), analyzer);
		}
        
		class FormatResult : IComparable<FormatResult>, IResultFactory<ILinkedNode>
		{
            readonly IBinaryFileFormat format;
            readonly ILinkedNode parent;
            readonly ILinkedNodeFactory nodeFactory;
            readonly Task<ILinkedNode> task;
            readonly IStreamFactory streamFactory;
            readonly object source;
            readonly TaskCompletionSource<ILinkedNode> nodeCreated;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => format.HeaderLength;

            public string Extension { get; private set; }
            public string Label { get; private set; }

            public ILinkedNode Result => task?.Result;

            public FormatResult(IStreamFactory streamFactory, object source, IBinaryFileFormat format, TaskCompletionSource<ILinkedNode> nodeCreated, ILinkedNode parent, ILinkedNodeFactory nodeFactory)
			{
                this.format = format;
                this.parent = parent;
                this.nodeFactory = nodeFactory;
                this.streamFactory = streamFactory;
                this.source = source;
                this.nodeCreated = nodeCreated;
                task = StartReading(streamFactory, Reader);
            }

            private ILinkedNode Reader(Stream stream)
            {
                return format.Match(stream, streamFactory, this);
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

            ILinkedNode IResultFactory<ILinkedNode>.Invoke<T>(T value)
            {
                try{
                    var formatObj = new FormatObject<T, IBinaryFileFormat>(format, value, source);
                    while(parent[formatObj] == null)
                    {
                        ILinkedNode node;
                        if(!nodeCreated.Task.Wait(MaxResultWaitTime))
                        {
                            formatObj = new FormatObject<T, IBinaryFileFormat>(ImprovisedFormat<T>.Instance, value, streamFactory);
                            continue;
                        }else{
                            node = nodeCreated.Task.Result;
                        }
                        if(node != null)
                        {
                            var obj = new LinkedObject<T>(node, source, value);
                            node = nodeFactory.Create<ILinkedObject<T>>(parent, obj);
                            Label = obj.Label;
                        }
                        return node;
                    }
                    {
                        var node = nodeFactory.Create<IFormatObject<T, IBinaryFileFormat>>(parent, formatObj);
                        Extension = formatObj.Extension;
                        Label = formatObj.Label;
                        nodeCreated?.TrySetResult(node);
                        return node;
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

                public override TResult Match<TResult>(Stream stream, ResultFactory<T, TResult> resultFactory)
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

        class ImprovisedSignatureFormat : BinaryFileFormat<ImprovisedSignatureFormat.Format>
        {
            public static readonly ImprovisedSignatureFormat Instance = new ImprovisedSignatureFormat();

            private ImprovisedSignatureFormat() : base(0, null, null)
            {

            }


            public override string GetMediaType(Format value)
            {
                return DataTools.GetFakeMediaTypeFromSignature(value.Signature);
            }

            public override string GetExtension(Format value)
            {
                return value.Signature;
            }

            public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
            {
                throw new NotSupportedException();
            }

            public override TResult Match<TResult>(Stream stream, ResultFactory<Format, TResult> resultFactory)
            {
                throw new NotSupportedException();
            }

            public class Format
            {
                public string Signature { get; }

                public Format(string signature)
                {
                    Signature = signature;
                }
            }
        }
    }
}
