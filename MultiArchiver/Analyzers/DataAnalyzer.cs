using FileSignatures;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Types;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Ude;
using VDS.RDF;

namespace IS4.MultiArchiver.Analyzers
{
    public class DataAnalyzer : RdfBase, IEntityAnalyzer<Stream>, IEntityAnalyzer<byte[]>
	{
		static readonly ThreadLocal<MD5> md5 = new ThreadLocal<MD5>(new Func<MD5>(MD5.Create));

		public DataAnalyzer(INodeFactory nodeFactory) : base(nodeFactory)
		{

		}

		public IUriNode Analyze(Stream entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
		{
			var uriNode = handler.CreateUriNode(new Uri("http://archive.data.is4.site/.well-known/genid/" + Guid.NewGuid().ToString("D")));

			handler.HandleTriple(uriNode, this[Properties.Type], this[Classes.ContentAsBase64]);
			handler.HandleTriple(uriNode, this[Properties.Extent], this[entity.Length.ToString(), Datatypes.Byte]);

            var results = FileFormatLocator.GetFormats().Where(format => format != null).OrderByDescending(format => format.HeaderLength).Select(format => new FormatResult(format)).ToList();
            //var results = FileFormatLocator.GetFormats().Where(format => format is IFileFormatReader).OrderByDescending(format => format.HeaderLength).Select(format => new FormatResult(format)).Skip(6).ToList();
            //var aliveResults = results.Where(result => result.Stream != null);
            var signatureBuffer = new MemoryStream(results.Select(result => result.Format).Max(format => Math.Max(format.HeaderLength == Int32.MaxValue ? 0 : format.HeaderLength, format.Offset + format.Signature.Count)));

            var charsetDetector = new CharsetDetector();
            var isBinary = false;
            byte[] hash = null;

			hash = md5.Value.ComputeHash(new AnalyzingStream(entity, segment => {
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
                /*Parallel.ForEach(results, result => {
                    result.Write(segment.Array, segment.Offset, segment.Count);
                });
                Parallel.ForEach(results, result => {
                    result.Flush();
                });*/
            }));

			charsetDetector.DataEnd();
            foreach(var result in results)
            {
                result.Close();
            }

			handler.HandleTriple(uriNode, this[Properties.DigestAlgorithm], this[Individuals.MD5]);
			handler.HandleTriple(uriNode, this[Properties.DigestValue], this[Convert.ToBase64String(hash), Datatypes.Base64Binary]);

            results.RemoveAll(result => !result.IsValid(signatureBuffer));
			results.Sort();

			if(results.Count > 0)
			{
				var entity2 = new FormatObject(results[0].Format, results[0].Task?.Result);
				var uriNode2 = baseAnalyzer.Analyze(entity2, handler);
				if(uriNode2 != null)
				{
					handler.HandleTriple(uriNode2, this[Properties.HasFormat], uriNode);
				}
			}

			foreach(var result in results)
			{
			    result.Task?.Dispose();
			}

			return uriNode;
		}

		public IUriNode Analyze(byte[] entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
		{
			return this.Analyze(new MemoryStream(entity, false), handler, baseAnalyzer);
		}
        
		class FormatResult : IComparable<FormatResult>
		{
			readonly IFileFormatReader reader;
            QueueStream stream;

            public FileFormat Format { get; }

            public Task<IDisposable> Task { get; }

			public FormatResult(FileFormat format)
			{
                Format = format;
				reader = format as IFileFormatReader;
				if(reader != null)
				{
					stream = new QueueStream
					{
						ForceClose = true,
                        AutoFlush = false
					};
                    Task = System.Threading.Tasks.Task.Run(() => {
					    try{
						    return reader.Read(stream);
					    }catch{
                            Close();
                            return null;
					    }
                    });
				}
            }

            public bool IsValid(Stream header)
            {
                return Format.IsMatch(header) && (reader == null || reader.IsMatch(Task.Result));
            }

            public bool Write(byte[] buffer, int offset, int count)
            {
                try{
                    stream?.Write(buffer, offset, count);
                    return true;
                }catch(IOException)
                {
                    return false;
                }
            }

            public bool Flush()
            {
                try{
                    stream?.Flush();
                    return true;
                }catch(IOException)
                {
                    return false;
                }
            }

            public bool Close()
            {
                try{
                    stream?.Close();
                    return true;
                }catch(IOException)
                {
                    return false;
                }
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
