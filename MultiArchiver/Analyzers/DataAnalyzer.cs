using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Types;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ude;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class DataAnalyzer : IEntityAnalyzer<Stream>, IEntityAnalyzer<byte[]>
	{
        readonly IHashAlgorithm hashAlgorithm;
        readonly IEnumerable<IFileFormat> formats;

		public DataAnalyzer(IHashAlgorithm hashAlgorithm, IEnumerable<IFileFormat> formats)
		{
            this.hashAlgorithm = hashAlgorithm;
            this.formats = formats.Where(format => format != null).OrderByDescending(format => format.HeaderLength);
        }

		public IRdfEntity Analyze(Stream entity, IRdfAnalyzer analyzer)
		{
            var results = formats.Select(FormatResult.Create).ToList();
            var signatureBuffer = new MemoryStream(results.Max(result => result.MaxReadBytes));

            var charsetDetector = new CharsetDetector();
            var isBinary = false;
            byte[] hash = null;

			hash = hashAlgorithm.ComputeHash(new AnalyzingStream(entity, segment => {
				if(!isBinary)
				{
					if(Array.IndexOf<byte>(segment.Array, 0, segment.Offset, segment.Count) != -1)
					{
						isBinary = true;
					}else{
						charsetDetector.Feed(segment.Array, segment.Offset, segment.Count);
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

            charsetDetector.DataEnd();

            if(charsetDetector.Charset == null || charsetDetector.Confidence < Single.Epsilon)
            {
                isBinary = true;
            }

            var node = analyzer.CreateUriNode(hashAlgorithm.FormatUri(hash));

            node.Set(isBinary ? Classes.ContentAsBase64 : Classes.ContentAsText);
            node.Set(Properties.Extent, entity.Length.ToString(), Datatypes.Byte);

            if(!isBinary)
            {
                node.Set(Properties.CharacterEncoding, charsetDetector.Charset);
            }

            node.Set(Properties.DigestAlgorithm, hashAlgorithm.Identifier);
            node.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

			if(results.Count > 0)
			{
				var entity2 = new FormatObject(results[0].Format, results[0].Task?.Result);
				var node2 = analyzer.Analyze(entity2);
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

		public IRdfEntity Analyze(byte[] entity, IRdfAnalyzer analyzer)
		{
			return this.Analyze(new MemoryStream(entity, false), analyzer);
		}
        
		class FormatResult : IComparable<FormatResult>
		{
			readonly IFileLoader loader;
            QueueStream stream;

            public IFileFormat Format { get; }

            public Task<IDisposable> Task { get; }

            public int MaxReadBytes => Format.HeaderLength;

            public FormatResult(IFileFormat format)
			{
                Format = format;
				loader = format as IFileLoader;
				if(loader != null)
				{
					stream = new QueueStream
					{
						ForceClose = true,
                        AutoFlush = false
					};
                    Task = System.Threading.Tasks.Task.Run(() => {
					    try{
						    return loader.Match(stream);
					    }catch{
                            Close();
                            return null;
					    }
                    });
				}
            }

            public static FormatResult Create(IFileFormat format)
            {
                return new FormatResult(format);
            }

            public bool IsValid(MemoryStream header)
            {
                if(!header.TryGetBuffer(out var buffer))
                {
                    buffer = new ArraySegment<byte>(header.ToArray());
                }
                return Format.Match(buffer.AsSpan()) && (loader == null || Task.Result != null);
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
	}
}
