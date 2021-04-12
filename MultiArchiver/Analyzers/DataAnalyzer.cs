using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Types;
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

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

            foreach(var result in results)
            {
                using(var entity2 = new FormatObject(result.Format, result.Result?.Result))
                {
                    var node2 = nodeFactory.Create(node, entity2);
                    if(node2 != null)
                    {
                        node2.Set(Properties.HasFormat, node);
                    }
                }
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
        
		class FormatResult : IComparable<FormatResult>
		{
            public IFileFormat Format { get; }

            readonly bool isParsed = false;
            public Task<object> Result { get; }

            public int MaxReadBytes => Format.HeaderLength;

            public FormatResult(IStreamFactory streamFactory, IFileFormat format, ILinkedNode parent, ILinkedNodeFactory nodeFactory)
			{
                Format = format;
                if(format is IFileReader reader)
                {
                    isParsed = true;
                    Result = StartReading(streamFactory, s => reader.Match(s, parent, nodeFactory));
                }else if(format is IFileLoader loader)
                {
                    isParsed = true;
                    Result = StartReading(streamFactory, loader.Match);
				}
            }

            private Task<object> StartReading(IStreamFactory streamFactory, Func<Stream, object> reader)
            {
                if(streamFactory.IsThreadSafe)
                {
                    return Task.Run(() => StartReadingInner(streamFactory, reader));
                }else{
                    return Task.FromResult(StartReadingInner(streamFactory, reader));
                }
            }

            private object StartReadingInner(IStreamFactory streamFactory, Func<Stream, object> reader)
            {
                var stream = streamFactory.Open();
                try{
                    try{
                        return reader(stream);
                    }catch(Exception e)
                    {
                        return e;
                    }
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
                return Format.Match(buffer.AsSpan()) && (!isParsed || Result != null);
            }

            public int CompareTo(FormatResult other)
            {
                var a = Result;
                var b = other?.Result;
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
