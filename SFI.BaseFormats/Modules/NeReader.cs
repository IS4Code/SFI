using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IS4.SFI.Formats.Modules
{
    /// <summary>
    /// Reads Windows New Executable (NE) modules.
    /// </summary>
    public class NeReader : IModule
    {
        readonly Stream stream;
        readonly BinaryReader reader;
        readonly uint headerOffset;

        /// <summary>
        /// The flags in the module's header.
        /// </summary>
        public ushort Flags {
            get {
                stream.Position = headerOffset + 0x0C;
                return reader.ReadUInt16();
            }
        }

        /// <inheritdoc/>
        public ModuleType Type {
            get {
                return (Flags & 0x8000) != 0 ? ModuleType.Library : ModuleType.Executable;
            }
        }

        IModuleSignature? IModule.Signature => null;

        /// <summary>
        /// Creates a new instance of the reader.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public NeReader(Stream stream)
        {
            this.stream = stream;
            reader = new BinaryReader(stream, Encoding.ASCII, true);

            stream.Position = 0x3C;
            stream.Position = headerOffset = reader.ReadUInt32();
            short sig = reader.ReadInt16();
            if(sig != 0x454E)
            {
                throw new ArgumentException("Not a valid NE file!", nameof(stream));
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IModuleResource> ReadResources()
        {
            stream.Position = headerOffset + 0x24;
            var resourcesOffset = headerOffset + reader.ReadUInt16();
            var residentNameOffset = headerOffset + reader.ReadUInt16();
            if(resourcesOffset == residentNameOffset)
            {
                // replaced by the resident-name table
                yield break;
            }
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
                    return type ? (object)(Win32ResourceType)id : (int)id;
                }
                return names[id];
            }

            int maxAlignment = (1 << shift) - 1;

            foreach(var res in resources.OrderBy(t => t.Item1))
            {
                yield return new Resource(stream, GetId(res.Item3, true), GetId(res.Item4, false), res.Item1, res.Item2, maxAlignment);
            }
        }

        /// <summary>
        /// Represents a resource in an NE module.
        /// </summary>
        public class Resource : IModuleResource
        {
            /// <inheritdoc/>
            public object Type { get; }

            /// <inheritdoc/>
            public object Name { get; }

            readonly long offset;
            readonly int maxAlignment;

            /// <inheritdoc/>
            public int Length { get; }

            readonly Stream stream;

            /// <summary>
            /// Creates a new instance of the resource.
            /// </summary>
            /// <param name="stream">The input stream.</param>
            /// <param name="type">The value of <see cref="Type"/>.</param>
            /// <param name="name">The value of <see cref="Name"/>.</param>
            /// <param name="offset">The offset of the resource.</param>
            /// <param name="length">The stored length of the resource.</param>
            /// <param name="maxAlignment">
            /// The maximum possible difference between the stored and real length of the resource,
            /// as a result of the shift encoding of the length.
            /// </param>
            public Resource(Stream stream, object type, object name, long offset, int length, int maxAlignment)
            {
                Type = type;
                Name = name;
                Length = length;
                this.offset = offset;
                this.stream = stream;
                this.maxAlignment = maxAlignment;
            }

            /// <inheritdoc/>
            public int Read(byte[] buffer, int offset, int length)
            {
                length = Math.Min(length, Length);
                stream.Position = this.offset;
                int read = stream.Read(buffer, offset, length);
                int minLength = offset + Math.Max(0, Math.Min(Length - maxAlignment, length));
                for(int i = offset + read - 1; i >= minLength; i--)
                {
                    if(buffer[i] != 0)
                    {
                        minLength = i + 1;
                        break;
                    }
                }
                return minLength - offset;
            }
        }
    }
}
