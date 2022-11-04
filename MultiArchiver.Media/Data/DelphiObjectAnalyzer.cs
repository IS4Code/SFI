using IS4.MultiArchiver.Media.Data;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of Delphi objects, as instances of <see cref="DelphiObject"/>.
    /// </summary>
    public class DelphiObjectAnalyzer : MediaObjectAnalyzer<DelphiObject>
    {
        public async override ValueTask<AnalysisResult> Analyze(DelphiObject obj, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            foreach(var (key, value) in FindBlobs(null, obj))
            {
                var infoNode = (await analyzers.Analyze<IFileInfo>(new BlobInfo(key, value), context.WithParent(node))).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.HasPart, infoNode);
                }
            }
            return new AnalysisResult(node, obj.Name);
        }

        IEnumerable<KeyValuePair<string, byte[]>> FindBlobs(string path, DelphiObject obj)
        {
            foreach(var (key, value) in obj)
            {
                string GetPath()
                {
                    return path == null ? key : path + "." + key;
                }

                switch(value)
                {
                    case byte[] arr:
                        yield return new KeyValuePair<string, byte[]>(GetPath(), arr);
                        break;
                    case DelphiObject obj1:
                        foreach(var blob in FindBlobs(GetPath(), obj1))
                        {
                            yield return blob;
                        }
                        break;
                }
            }
        }

        class BlobInfo : IFileInfo
        {
            public string Name { get; }

            public ArraySegment<byte> Data { get; }

            //public string Type { get; }

            public BlobInfo(string key, byte[] data)
            {
                Name = key;
                int typeLen = data[0];
                //Type = System.Text.Encoding.ASCII.GetString(data, 2, typeLen);
                int offset = 1 + typeLen;
                int len = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                if(len == data.Length - offset)
                {
                    Data = data.Slice(offset);
                    return;
                }
                offset = 0;
                len = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                if(len == data.Length - offset)
                {
                    Data = data.Slice(offset);
                    return;
                }
                Data = new ArraySegment<byte>(data);
            }

            public long Length => Data.Array.Length;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => Data.Array;

            public object DataKey => Data.Offset;

            public Stream Open()
            {
                return Data.AsStream(false);
            }

            public bool IsEncrypted => false;

            public string SubName => null;

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.Embedded;

            public override string ToString()
            {
                return $"/{Name}";
            }
        }
    }
}
