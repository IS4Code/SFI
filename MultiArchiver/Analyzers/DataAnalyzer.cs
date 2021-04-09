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
    public sealed class DataAnalyzer : IEntityAnalyzer<Stream>, IEntityAnalyzer<byte[]>
	{
        public IHashAlgorithm HashAlgorithm { get; set; }
        public Func<IEncodingDetector> EncodingDetectorFactory { get; set; }
        public ICollection<IFileFormat> Formats { get; } = new SortedSet<IFileFormat>();

		public DataAnalyzer(IHashAlgorithm hashAlgorithm, Func<IEncodingDetector> encodingDetectorFactory)
		{
            HashAlgorithm = hashAlgorithm;
            EncodingDetectorFactory = encodingDetectorFactory;
        }

		public ILinkedNode Analyze(Stream entity, ILinkedNodeFactory nodeFactory)
		{
            var results = Formats.Select(format => new FormatResult(format, nodeFactory)).ToList();
            var signatureBuffer = new MemoryStream(results.Max(result => result.MaxReadBytes));

            var encodingDetector = EncodingDetectorFactory?.Invoke();
            var isBinary = false;
            byte[] hash = null;

			hash = HashAlgorithm.ComputeHash(new AnalyzingStream(entity, segment => {
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
                foreach(var result in results)
                {
                    result.Write(segment.Array, segment.Offset, segment.Count);
                }
                foreach(var result in results)
                {
                    result.Flush();
                }
            }));
            foreach(var result in results)
            {
                result.Close();
            }

            encodingDetector.End();

            if(encodingDetector.Charset == null || encodingDetector.Confidence < Single.Epsilon)
            {
                isBinary = true;
            }

            var node = nodeFactory.Create(HashAlgorithm, hash);

            node.Set(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, entity.Length.ToString(), Datatypes.Byte);

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, encodingDetector.Charset);
            }

            node.Set(Properties.DigestAlgorithm, HashAlgorithm.Identifier);
            node.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

			if(results.Count > 0)
			{
				var entity2 = new FormatObject(results[0].Format, results[0].Task?.Result);
				var node2 = nodeFactory.Create(entity2);
				if(node2 != null)
				{
                    node2.Set(Properties.HasFormat, node);
				}
			}

			foreach(var result in results)
			{
			    result.Task?.Dispose();
			}

			return node;
		}

		public ILinkedNode Analyze(byte[] entity, ILinkedNodeFactory analyzer)
		{
			return Analyze(new MemoryStream(entity, false), analyzer);
		}
        
		class FormatResult : IComparable<FormatResult>
		{
            QueueStream stream;

            public IFileFormat Format { get; }

            public Task<object> Task { get; }

            public int MaxReadBytes => Format.HeaderLength;

            public FormatResult(IFileFormat format, ILinkedNodeFactory nodeFactory)
			{
                Format = format;
                if(format is IFileReader reader)
                {
                    Task = StartReading(stream => reader.Match(stream, nodeFactory));
                }else if(format is IFileLoader loader)
				{
                    Task = StartReading(stream => loader.Match(stream));
				}
            }

            private Task<object> StartReading(Func<Stream, object> reader)
            {
				stream = new QueueStream
				{
					ForceClose = true,
                    AutoFlush = false
				};
                return System.Threading.Tasks.Task.Run(() => {
					try{
						return reader(stream);
					}catch{
                        Close();
                        return null;
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

            public bool Write(byte[] buffer, int offset, int count)
            {
                try{
                    stream?.Write(buffer, offset, count);
                    return true;
                }catch(IOException)
                {

                }catch(ObjectDisposedException)
                {

                }
                stream = null;
                return false;
            }

            public bool Flush()
            {
                try{
                    stream?.Flush();
                    return true;
                }catch(IOException)
                {

                }catch(ObjectDisposedException)
                {

                }
                stream = null;
                return false;
            }

            public bool Close()
            {
                try{
                    stream?.Close();
                    return true;
                }catch(IOException)
                {

                }catch(ObjectDisposedException)
                {

                }
                stream = null;
                return false;
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

        class HeaderLengthComparer : IComparer<IFileFormat>
        {
            public static readonly HeaderLengthComparer Instance = new HeaderLengthComparer();

            private HeaderLengthComparer()
            {

            }

            public int Compare(IFileFormat x, IFileFormat y)
            {
                return -x.HeaderLength.CompareTo(y);
            }
        }
	}
}
