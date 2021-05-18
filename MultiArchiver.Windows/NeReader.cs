using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IS4.MultiArchiver.Windows
{
    public class NeReader
    {
        readonly Stream stream;
        readonly BinaryReader reader;
        readonly uint headerOffset;

        public ushort Flags {
            get {
                stream.Position = headerOffset + 0x0C;
                return reader.ReadUInt16();
            }
        }

        public NeReader(Stream stream)
        {
            this.stream = stream;
            reader = new BinaryReader(stream, Encoding.ASCII, true);

            stream.Position = 0x3C;
            stream.Position = headerOffset = reader.ReadUInt32();
        }

        public IEnumerable<Resource> ReadResources()
        {
            stream.Position = headerOffset + 0x24;
            var resourcesOffset = headerOffset + reader.ReadUInt16();
            stream.Position = resourcesOffset;
            var shift = reader.ReadUInt16();
            var strnames = new SortedSet<short>();
            var resources = new List<(long, int, short, short)>();
            while(true)
            {
                short type = reader.ReadInt16();
                if(type == 0)
                {
                    break;
                }
                if(type > 0)
                {
                    strnames.Add(type);
                }
                ushort num = reader.ReadUInt16();
                reader.ReadInt32();
                for(int j = 0; j < num; j++)
                {
                    long pos = reader.ReadUInt16() << shift;
                    int len = reader.ReadUInt16() << shift;
                    reader.ReadInt16();
                    short name = reader.ReadInt16();
                    if(name >= 0)
                    {
                        strnames.Add(name);
                    }
                    reader.ReadInt32();

                    resources.Add((pos, len, type, name));
                }
            }
            var names = new Dictionary<short, string>();
            foreach(var name in strnames)
            {
                stream.Position = resourcesOffset + name;
                byte len = reader.ReadByte();
                names[name] = new string(reader.ReadChars(len));
            }

            object GetId(short id)
            {
                if(id < 0) return id & 0x7FFF;
                return names[id];
            }

            int maxAlignment = (1 << shift) - 1;

            foreach(var res in resources.OrderBy(t => t.Item1))
            {
                yield return new Resource(stream, GetId(res.Item3), GetId(res.Item4), res.Item1, res.Item2, maxAlignment);
            }
        }

        public class Resource : IFileInfo
        {
            public object Type { get; }
            public object Name { get; }
            public ArraySegment<byte> Data { get; }

            public bool IsEncrypted => false;

            string IFileNodeInfo.Name => Name?.ToString();

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public long Length { get; }

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey { get; }

            public object DataKey => (Type, Name);

            public Resource(Stream stream, object type, object name, long offset, int length, int maxAlignment)
            {
                ReferenceKey = stream;
                Type = type;
                Name = name;
                Length = length;
                var data = new byte[length];
                stream.Position = offset;
                int read = stream.Read(data, 0, length);
                int minLength = length - maxAlignment;
                for(int i = read - 1; i >= minLength; i--)
                {
                    if(data[i] != 0)
                    {
                        minLength = i + 1;
                        break;
                    }
                }
                Data = new ArraySegment<byte>(data, 0, minLength);
            }

            public Stream Open()
            {
                return new MemoryStream(Data.Array, Data.Offset, Data.Count, false);
            }
        }
    }
}
