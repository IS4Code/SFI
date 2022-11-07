using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IS4.SFI.Media.Modules
{
    /// <summary>
    /// Reads extended MZ modules in the LE (Linear Executable) sub-format.
    /// </summary>
    public class LeReader : IModule
    {
        readonly Stream stream;
        readonly BinaryReader reader;
        readonly uint headerOffset;

        /// <summary>
        /// The flags in the header of the module.
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
        /// Creates a new instance of the reader from an input stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public LeReader(Stream stream)
        {
            this.stream = stream;
            reader = new BinaryReader(stream, Encoding.ASCII, true);

            stream.Position = 0x3C;
            stream.Position = headerOffset = reader.ReadUInt32();
            short sig = reader.ReadInt16();
            if(sig != 0x454C && sig != 0x584C)
            {
                throw new ArgumentException("Not a valid LE file!", nameof(stream));
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IModuleResource> ReadResources()
        {
            return Array.Empty<IModuleResource>();
        }
    }
}
