using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<IStreamFactory>, IEntityAnalyzer<byte[]>
	{
        public ICollection<IDataHashAlgorithm> HashAlgorithms { get; } = new List<IDataHashAlgorithm>();
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }
        public ICollection<IFileFormat> Formats { get; } = new SortedSet<IFileFormat>(HeaderLengthComparer.Instance);

		public DataAnalyzer(Func<IEncodingDetector> encodingDetectorFactory)
		{
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public ILinkedNode Analyze(ILinkedNode parent, IStreamFactory streamFactory, ILinkedNodeFactory nodeFactory)
        {
            var node = nodeFactory.Root[Guid.NewGuid().ToString("D")];
            var results = Formats.Select(format => new FormatResult(streamFactory, format, node, nodeFactory)).ToList();
            var signatureBuffer = new MemoryStream(results.Max(result => result.MaxReadBytes));

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

            if(encodingDetector.Charset == null || encodingDetector.Confidence < Single.Epsilon)
            {
                isBinary = true;
            }

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, length.ToString(), Datatypes.Byte);

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, encodingDetector.Charset);
            }

            foreach(var hash in hashes)
            {
                var hashNode = nodeFactory.Create(hash.alg, hash.data.Result);

                hashNode.SetClass(Classes.Digest);
                
                hashNode.Set(Properties.DigestAlgorithm, hash.alg.Identifier);
                hashNode.Set(Properties.DigestValue, Convert.ToBase64String(hash.data.Result), Datatypes.Base64Binary);

                node.Set(Properties.Broader, hashNode);
            }

            foreach(var result in results)
            {
                result.Wait();
            }

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

            foreach(var result in results)
            {
                result.Result.Set(Properties.HasFormat, node);
            }

			return node;
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
        
		class FormatResult : IComparable<FormatResult>, IFileReadingResultFactory<ILinkedNode>
		{
            readonly IFileFormat format;
            readonly ILinkedNode parent;
            readonly ILinkedNodeFactory nodeFactory;
            readonly Task<ILinkedNode> task;

            public int MaxReadBytes => format.HeaderLength;

            public ILinkedNode Result => task?.Result;

            public FormatResult(IStreamFactory streamFactory, IFileFormat format, ILinkedNode parent, ILinkedNodeFactory nodeFactory)
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

            ILinkedNode IFileReadingResultFactory<ILinkedNode>.Read<T>(T value)
            {
                return nodeFactory.Create<IFormatObject<T>>(parent, new FormatObject<T>(format, value));
            }

            class FormatObject<T> : IFormatObject<T>
            {
                readonly IFileFormat format;
                public string Extension => format is IFileFormat<T> fmt ? fmt.GetExtension(Value) : format.GetExtension(Value);
                public string MediaType => format is IFileFormat<T> fmt ? fmt.GetMediaType(Value) : format.GetMediaType(Value);
                public T Value { get; }

                public FormatObject(IFileFormat format, T value)
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

            public bool IsValid(MemoryStream header)
            {
                if(!header.TryGetBuffer(out var buffer))
                {
                    buffer = new ArraySegment<byte>(header.ToArray());
                }
                return format.Match(buffer.AsSpan()) && !task.IsFaulted;
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

        class HeaderLengthComparer : GlobalObjectComparer<IFileFormat>
        {
            public static readonly IComparer<IFileFormat> Instance = new HeaderLengthComparer();

            private HeaderLengthComparer()
            {

            }

            protected override int CompareInner(IFileFormat x, IFileFormat y)
            {
                return -x.HeaderLength.CompareTo(y.HeaderLength);
            }
        }
	}
}
