using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

            object GetId(short id, bool type)
            {
                if(id < 0)
                {
                    id &= 0x7FFF;
                    return type ? (object)(Win32ResourceType)id : id;
                }
                return names[id];
            }

            int maxAlignment = (1 << shift) - 1;

            foreach(var res in resources.OrderBy(t => t.Item1))
            {
                yield return new Resource(stream, GetId(res.Item3, true), GetId(res.Item4, false), res.Item1, res.Item2, maxAlignment);
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
                var typeCode = type as Win32ResourceType? ?? 0;

                ReferenceKey = stream;
                Type = type;
                Name = name;
                Length = length;
                int start = 0;
                if(typeCode == Win32ResourceType.Bitmap)
                {
                    start = 14;
                }
                var data = new byte[start + length];
                stream.Position = offset;
                int read = stream.Read(data, start, length);
                int minLength = start + length - maxAlignment;
                for(int i = start + read - 1; i >= minLength; i--)
                {
                    if(data[i] != 0)
                    {
                        minLength = i + 1;
                        break;
                    }
                }
                Data = new ArraySegment<byte>(data, 0, minLength);
                if(typeCode == Win32ResourceType.Bitmap)
                {
                    var header = new Span<byte>(data, 0, start);
                    MemoryMarshal.Cast<byte, short>(header)[0] = 0x4D42;
                    var fields = MemoryMarshal.Cast<byte, int>(header.Slice(2));
                    fields[0] = Data.Count;

                    int dataStart = start + BitConverter.ToInt32(data, start);

                    int numColors = BitConverter.ToInt32(data, start + 32);
                    if(numColors == 0)
                    {
                        var bits = BitConverter.ToInt16(data, start + 14);
                        if(bits < 16)
                        {
                            numColors = 1 << bits;
                        }
                    }

                    dataStart += numColors * sizeof(int);

                    fields[2] = dataStart;
                }
            }

            public Stream Open()
            {
                return new MemoryStream(Data.Array, Data.Offset, Data.Count, false);
            }

            public override string ToString()
            {
                return Type + "/" + Name;
            }
        }
    }
}
