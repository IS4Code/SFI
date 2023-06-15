using IS4.SFI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a general format for extended MZ modules.
    /// </summary>
    public class GenericModuleFormat : ModuleFormat<GenericModuleFormat.Module>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public GenericModuleFormat() : base("", "application/x-msdownload", "exe")
        {

        }
        
        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Module, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(new Module(stream), args);
        }

        /// <inheritdoc/>
        public override string? GetMediaType(Module value)
        {
            return $"application/x-msdownload;format={value.Signature.ToLowerInvariant()}";
        }

        /// <summary>
        /// An implementation of <see cref="IModule"/> storing the signature
        /// of the extended MZ module.
        /// </summary>
        public class Module : IModule
        {
            /// <summary>
            /// The signature of the module.
            /// </summary>
            public string Signature { get; }

            IModuleSignature? IModule.Signature => null;

            /// <summary>
            /// Loads the module from a stream.
            /// </summary>
            /// <param name="stream">The input stream in the MZ format.</param>
            public Module(Stream stream)
            {
                var reader = new BinaryReader(stream, Encoding.ASCII, true);
                stream.Position = 0x3C;
                var headerPosition = reader.ReadUInt32();
                if(headerPosition <= 1 || headerPosition >= stream.Length - 1)
                {
                    throw new ArgumentException("Not an extended module file!");
                }
                stream.Position = headerPosition;
                var sigBuffer = new byte[8];
                var sigBytes = sigBuffer.Slice(0, stream.Read(sigBuffer, 0, sigBuffer.Length));
                var sig = DataTools.ExtractSignature(sigBytes);
                if(sig == null)
                {
                    throw new ArgumentException("Not an extended module file!");
                }
                if(sig == "LE" || sig == "LX" || sig == "NE" || sig == "PE")
                {
                    throw new ArgumentException("This format is recognized by other instances.");
                }
                Signature = sig;
            }

            /// <inheritdoc/>
            public ModuleType Type => ModuleType.Unknown;

            /// <inheritdoc/>
            public IEnumerable<IModuleResource> ReadResources()
            {
                return Array.Empty<IModuleResource>();
            }
        }
    }
}
