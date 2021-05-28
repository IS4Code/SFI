using IS4.MultiArchiver.Formats.Data;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class DelphiObjectAnalyzer : BinaryFormatAnalyzer<DelphiObject>
    {
        public override string Analyze(ILinkedNode node, DelphiObject obj, ILinkedNodeFactory nodeFactory)
        {
            foreach(var (key, value) in FindBlobs(null, obj))
            {
                var infoNode = nodeFactory.Create<IFileInfo>(node, new BlobInfo(key, value));
                if(infoNode != null)
                {
                    infoNode.SetClass(Classes.EmbeddedFileDataObject);
                    node.Set(Properties.HasMediaStream, infoNode);
                }
            }
            return obj.Name;
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
                    Data = new ArraySegment<byte>(data, offset, data.Length - offset);
                    return;
                }
                offset = 0;
                len = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                if(len == data.Length - offset)
                {
                    Data = new ArraySegment<byte>(data, offset, data.Length - offset);
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
                return new MemoryStream(Data.Array, Data.Offset, Data.Count, false);
            }

            public bool IsEncrypted => false;

            public string SubName => null;

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public override string ToString()
            {
                return $"/{Name}";
            }
        }
    }
}
