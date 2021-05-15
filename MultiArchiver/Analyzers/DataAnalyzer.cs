using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>
	{
        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }
        public ICollection<IBinaryFileFormat> Formats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);

        public int MaxDataLengthToStore => Math.Max(64, HashAlgorithms.Sum(h => h.HashSize + 64)) - 16;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public ILinkedNode Analyze(ILinkedNode parent, IStreamFactory streamFactory, ILinkedNodeFactory nodeFactory)
        {
            var signatureBuffer = new MemoryStream(Math.Max(MaxDataLengthToStore + 1, Formats.Max(fmt => fmt.HeaderLength)));

            var encodingDetector = EncodingDetectorFactory?.Invoke();

            var isBinary = false;

            var lazyMatch = new Lazy<FileMatch>(() => new FileMatch(Formats, streamFactory, signatureBuffer, MaxDataLengthToStore, encodingDetector, isBinary, nodeFactory), false);
            
            long length = 0;
            var hashes = new List<(IDataHashAlgorithm alg, QueueStream stream, Task<byte[]> data)>();

            using(var stream = streamFactory.Open())
            {
                foreach(var hash in HashAlgorithms)
                {
                    var queue = new QueueStream
                    {
                        AutoFlush = false
                    };
                    hashes.Add((hash, queue, Task.Run(() => {
                        return hash.ComputeHash(queue);
                    })));
                }

                var buffer = new byte[16384];
                int read = -1;
                while(read != 0)
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if(read == 0)
                    {
                        foreach(var hash in hashes)
                        {
                            hash.stream.Dispose();
                        }
                        break;
                    }
                    foreach(var hash in hashes)
                    {
                        hash.stream.Write(buffer, 0, read);
                    }
                    foreach(var hash in hashes)
                    {
                        hash.stream.Flush();
                    }
                    if(!isBinary)
				    {
					    if(Array.IndexOf<byte>(buffer, 0, 0, read) != -1)
					    {
						    isBinary = true;
					    }else{
                            encodingDetector.Write(new ArraySegment<byte>(buffer, 0, read));
					    }
				    }
				    if(signatureBuffer.Length < signatureBuffer.Capacity)
				    {
					    signatureBuffer.Write(buffer, 0, Math.Min(signatureBuffer.Capacity - (int)signatureBuffer.Length, read));
                    }else{
                        _ = lazyMatch.Value;
                    }
                    length += read;
                }

                encodingDetector.End();
            }

            var match = lazyMatch.Value;

            isBinary |= match.IsBinary;

            foreach(var result in match.Results)
            {
                result.Wait();
            }

            var node = match.Node;

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, match.CharsetMatch.Charset);
            }

            var label = $"{(isBinary ? "binary data" : "text")} ({DataTools.SizeSuffix(length, 2)})";

            node.Set(Properties.PrefLabel, label, "en");

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, length, Datatypes.Byte);

            if(!match.IdentifyWithData)
            {
                foreach(var hash in hashes)
                {
                    HashAlgorithm.AddHash(node, hash.alg, hash.data.Result, nodeFactory);
                }
            }

            foreach(var result in match.Results.Where(result => result.IsValid))
            {
                result.Result?.Set(Properties.HasFormat, node);
            }

			return node;
		}

        class FileMatch : IUriFormatter<bool>
        {
            readonly bool isBinary;
            public bool IsBinary => isBinary || CharsetMatch?.Charset == null;
            public IReadOnlyList<FormatResult> Results { get; }
            public ILinkedNode Node { get; }
            public bool IdentifyWithData { get; }

            readonly Lazy<CharsetMatch> LazyCharsetMatch;

            public CharsetMatch CharsetMatch => LazyCharsetMatch?.Value;

            public ArraySegment<byte> Signature { get; }

            public FileMatch(ICollection<IBinaryFileFormat> formats, IStreamFactory streamFactory, MemoryStream signatureBuffer, int maxDataLength, IEncodingDetector encodingDetector, bool isBinary, ILinkedNodeFactory nodeFactory)
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
                        Node.Set(Properties.Chars, strval, Datatypes.String);
                    }
                }else{
                    Node = nodeFactory.NewGuidNode();
                }

                this.isBinary = isBinary;

                if(Signature.Count > 0)
                {
                    Results = formats.Where(fmt => fmt.CheckHeader(signature, isBinary, encodingDetector)).Select(fmt => new FormatResult(streamFactory, fmt, Node, nodeFactory)).ToList();
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
                        return new Uri("data:" + data);
                    default:
                        return new Uri("data:;charset=" + CharsetMatch.Charset + data, UriKind.Absolute);
                }
            }

            private string TryGetString(Encoding encoding, ArraySegment<byte> data)
            {
                if(encoding == null) return null;
                try{
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
			return Analyze(parent, new MemoryStreamFactory(data), analyzer);
		}

        class MemoryStreamFactory : IStreamFactory
        {
            readonly byte[] data;

            public bool IsThreadSafe => true;

            object IPersistentKey.ReferenceKey => data;

            object IPersistentKey.DataKey => null;

            public MemoryStreamFactory(byte[] data)
            {
                this.data = data;
            }

            public Stream Open()
            {
                return new MemoryStream(data, false);
            }
        }
        
		class FormatResult : IComparable<FormatResult>, IGenericFunc<ILinkedNode>
		{
            readonly IBinaryFileFormat format;
            readonly ILinkedNode parent;
            readonly ILinkedNodeFactory nodeFactory;
            readonly Task<ILinkedNode> task;
            readonly IStreamFactory streamFactory;

            public bool IsValid => !task.IsFaulted;

            public int MaxReadBytes => format.HeaderLength;

            public ILinkedNode Result => task?.Result;

            public FormatResult(IStreamFactory streamFactory, IBinaryFileFormat format, ILinkedNode parent, ILinkedNodeFactory nodeFactory)
			{
                this.format = format;
                this.parent = parent;
                this.nodeFactory = nodeFactory;
                this.streamFactory = streamFactory;
                task = StartReading(streamFactory, Reader);
            }

            private ILinkedNode Reader(Stream stream)
            {
                return format.Match(stream, streamFactory, this);
            }

            public void Wait()
            {
                Task.WaitAny(task);
            }

            ILinkedNode IGenericFunc<ILinkedNode>.Invoke<T>(T value)
            {
                return nodeFactory.Create<IFormatObject<T, IBinaryFileFormat>>(parent, new FormatObject<T, IBinaryFileFormat>(format, value, streamFactory));
            }

            private Task<ILinkedNode> StartReading(IStreamFactory streamFactory, Func<Stream, ILinkedNode> reader)
            {
                if(streamFactory.IsThreadSafe)
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
