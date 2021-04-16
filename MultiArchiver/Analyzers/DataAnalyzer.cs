using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>, IUriFormatter<(string charset, ArraySegment<byte> data)>
	{
        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }
        public ICollection<IBinaryFileFormat> Formats { get; } = new SortedSet<IBinaryFileFormat>(HeaderLengthComparer.Instance);
        public float MinimumCharsetConfidence { get; } = 0.5000001f;

        public int MaxDataLengthToStore => Math.Max(64, HashAlgorithms.Sum(h => h.HashSize + 64)) - 16;

        public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public ILinkedNode Analyze(ILinkedNode parent, IStreamFactory streamFactory, ILinkedNodeFactory nodeFactory)
        {
            var node = nodeFactory.Root[Guid.NewGuid().ToString("D")];
            var results = Formats.Select(format => new FormatResult(streamFactory, format, node, nodeFactory)).ToList();
            var signatureBuffer = new MemoryStream(Math.Max(MaxDataLengthToStore, results.Max(result => result.MaxReadBytes)));

            var encodingDetector = EncodingDetectorFactory?.Invoke();
            var isBinary = false;
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
				    }
                    length += read;
                }

                encodingDetector.End();
            }

            if(length == 0 || encodingDetector.Charset == null || encodingDetector.Confidence < MinimumCharsetConfidence)
            {
                isBinary = true;
            }

            if(isBinary)
            {
                encodingDetector = null;
            }

            foreach(var result in results)
            {
                result.Wait();
            }

            if(!signatureBuffer.TryGetBuffer(out var signature))
            {
                signature = new ArraySegment<byte>(signatureBuffer.ToArray());
            }

            results.RemoveAll(result => !result.IsValid(signature));
			results.Sort();

            bool identifyWithData = length <= MaxDataLengthToStore;

            Encoding encoding = null;
            string charset = null;

            if(!isBinary)
            {
                encoding = TryGetEncoding(encodingDetector.Charset);
                charset = encoding?.WebName ?? encodingDetector.Charset;
                node.Set(Properties.CharacterEncoding, charset);
            }

            var label = $"{(isBinary ? "binary data" : "text")} ({DataTools.SizeSuffix(length, 2)})";

            if(identifyWithData)
            {
                var dataNode = nodeFactory.Create(this, (charset, signature));
                if(results.Count > 0)
                {
                    node.Set(Properties.SameAs, dataNode);
                    node.Set(Properties.PrefLabel, label, "en");
                }
                node = dataNode;
            }
            node.Set(Properties.PrefLabel, label, "en");

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, length.ToString(), Datatypes.Byte);

            if(identifyWithData)
            {
                string strval;
                if(isBinary || (strval = TryGetString(encoding, signature)) == null)
                {
                    node.Set(Properties.Value, Convert.ToBase64String(signature.Array, signature.Offset, signature.Count), Datatypes.Base64Binary);
                }else{
                    node.Set(Properties.Value, strval, Datatypes.String);
                }
            }else{
                foreach(var hash in hashes)
                {
                    HashAlgorithm.AddHash(node, hash.alg, hash.data.Result, nodeFactory);
                }
            }

            foreach(var result in results)
            {
                result.Result?.Set(Properties.HasFormat, node);
            }

			return node;
		}

		public ILinkedNode Analyze(ILinkedNode parent, byte[] data, ILinkedNodeFactory analyzer)
		{
			return Analyze(parent, new MemoryStreamFactory(data), analyzer);
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

        static readonly Regex urlRegex = new Regex(@"%[a-f0-9]{2}|\+", RegexOptions.Compiled);

        Uri IUriFormatter<(string charset, ArraySegment<byte> data)>.FormatUri((string charset, ArraySegment<byte> data) value)
        {
            string base64Encoded = ";base64," + Convert.ToBase64String(value.data.Array, value.data.Offset, value.data.Count);
            string uriEncoded = "," + UriTools.EscapeDataBytes(value.data.Array, value.data.Offset, value.data.Count);

            string data = uriEncoded.Length <= base64Encoded.Length ? uriEncoded : base64Encoded;

            switch(value.charset?.ToLowerInvariant())
            {
                case null:
                    return new Uri("data:application/octet-stream" + data, UriKind.Absolute);
                case "ascii":
                case "us-ascii":
                    return new Uri("data:" + data);
                default:
                    return new Uri("data:;charset=" + value.charset + data, UriKind.Absolute);
            }
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

            public int MaxReadBytes => format.HeaderLength;

            public ILinkedNode Result => task?.Result;

            public FormatResult(IStreamFactory streamFactory, IBinaryFileFormat format, ILinkedNode parent, ILinkedNodeFactory nodeFactory)
			{
                this.format = format;
                this.parent = parent;
                this.nodeFactory = nodeFactory;
                task = StartReading(streamFactory, s => format.Match(s, this));
            }

            public void Wait()
            {
                Task.WaitAny(task);
            }

            ILinkedNode IGenericFunc<ILinkedNode>.Invoke<T>(T value)
            {
                return nodeFactory.Create<IFormatObject<T>>(parent, new FormatObject<T>(format, value));
            }

            class FormatObject<T> : IFormatObject<T>
            {
                readonly IBinaryFileFormat format;
                public string Extension => format is IBinaryFileFormat<T> fmt ? fmt.GetExtension(Value) : format.GetExtension(Value);
                public string MediaType => format is IBinaryFileFormat<T> fmt ? fmt.GetMediaType(Value) : format.GetMediaType(Value);
                public T Value { get; }

                public FormatObject(IBinaryFileFormat format, T value)
                {
                    this.format = format;
                    Value = value;
                }
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

            public bool IsValid(ArraySegment<byte> header)
            {
                return format.Match(header) && !task.IsFaulted;
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
