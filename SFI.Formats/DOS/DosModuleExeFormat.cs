using IS4.SFI.Formats.Modules;
using IS4.SFI.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the MS-DOS MZ module format, producing instances of
    /// <see cref="DosModule"/>.
    /// </summary>
    public class DosModuleExeFormat : ModuleFormat<DosModule>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DosModuleExeFormat() : base(null, "application/x-msdos-program", "exe")
        {

        }

        /// <inheritdoc/>
        protected override bool CheckSignature(ReadOnlySpan<byte> header)
        {
            var fields = header.MemoryCast<ushort>();
            return (fields.Length > 0 && fields[0] == 0x4D5A) || base.CheckSignature(header);
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DosModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(new DosModule(stream), args);
        }
    }
}
