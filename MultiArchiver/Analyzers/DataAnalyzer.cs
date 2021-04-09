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
    public sealed class DataAnalyzer : IEntityAnalyzer<Func<Stream>>, IEntityAnalyzer<byte[]>
	{
        public IHashAlgorithm HashAlgorithm { get; set; }
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }
        public ICollection<IFileFormat> Formats { get; } = new SortedSet<IFileFormat>(HeaderLengthComparer.Instance);

		public DataAnalyzer(IHashAlgorithm hashAlgorithm, Func<IEncodingDetector> encodingDetectorFactory)
		{
            HashAlgorithm = hashAlgorithm;
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public ILinkedNode Analyze(Func<Stream> streamFactory, ILinkedNodeFactory nodeFactory)
		{
            var results = Formats.Select(format => new FormatResult(streamFactory, format, nodeFactory)).ToList();
            var signatureBuffer = new MemoryStream(results.Max(result => result.MaxReadBytes));

            var encodingDetector = EncodingDetectorFactory?.Invoke();
            var isBinary = false;
            byte[] hash = null;
            long length;

            using(var stream = streamFactory())
            {
                length = stream.Length;

			    hash = HashAlgorithm.ComputeHash(new AnalyzingStream(stream, segment => {
				    if(!isBinary)
				    {
					    if(Array.IndexOf<byte>(segment.Array, 0, segment.Offset, segment.Count) != -1)
					    {
						    isBinary = true;
					    }else{
						    encodingDetector.Write(segment);
					    }
				    }
				    if(signatureBuffer.Length < signatureBuffer.Capacity)
				    {
					    signatureBuffer.Write(segment.Array, segment.Offset, Math.Min(signatureBuffer.Capacity - (int)signatureBuffer.Length, segment.Count));
				    }
                }));

                encodingDetector.End();
            }

            if(encodingDetector.Charset == null || encodingDetector.Confidence < Single.Epsilon)
            {
                isBinary = true;
            }

            var node = nodeFactory.Create(HashAlgorithm, hash);

            node.SetClass(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, length.ToString(), Datatypes.Byte);

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, encodingDetector.Charset);
            }

            node.Set(Properties.DigestAlgorithm, HashAlgorithm.Identifier);
            node.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

            foreach(var result in results)
            {
                var entity2 = new FormatObject(result.Format, result.Task);
                var node2 = nodeFactory.Create(entity2);
                if(node2 != null)
                {
                    node2.Set(Properties.HasFormat, node);
                }
            }

			return node;
		}

		public ILinkedNode Analyze(byte[] entity, ILinkedNodeFactory analyzer)
		{
			return Analyze(() => new MemoryStream(entity, false), analyzer);
		}
        
		class FormatResult : IComparable<FormatResult>
		{
            public IFileFormat Format { get; }

            public Task<object> Task { get; }

            public int MaxReadBytes => Format.HeaderLength;

            public FormatResult(Func<Stream> streamFactory, IFileFormat format, ILinkedNodeFactory nodeFactory)
			{
                Format = format;
                if(format is IFileReader reader)
                {
                    Task = StartReading(streamFactory, s => reader.Match(s, nodeFactory));
                }else if(format is IFileLoader loader)
				{
                    Task = StartReading(streamFactory, loader.Match);
				}
            }

            private Task<object> StartReading(Func<Stream> streamFactory, Func<Stream, object> reader)
            {
                return System.Threading.Tasks.Task.Run(() => {
                    using(var stream = streamFactory())
                    {
                        try{
                            return reader(stream);
                        }catch(Exception e)
                        {
                            return e;
                        }
                    }
                });
            }

            public bool IsValid(MemoryStream header)
            {
                if(!header.TryGetBuffer(out var buffer))
                {
                    buffer = new ArraySegment<byte>(header.ToArray());
                }
                return Format.Match(buffer.AsSpan()) && (Task == null || Task.Result != null);
            }

            public int CompareTo(FormatResult other)
            {
                var a = Task?.Result;
                var b = other.Task?.Result;
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
