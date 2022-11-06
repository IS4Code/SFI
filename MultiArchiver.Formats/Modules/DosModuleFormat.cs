using IS4.MultiArchiver.Formats.Modules;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the MS-DOS MZ module format, producing instances of
    /// <see cref="DosModule"/>.
    /// </summary>
    public class DosModuleFormat : ModuleFormat<DosModule>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DosModuleFormat() : base(null, "application/x-dosexec", "exe")
        {

        }
        
        protected override bool CheckSignature(Span<byte> header)
        {
            var fields = header.MemoryCast<ushort>();
            return (fields.Length > 0 && fields[0] == 0x4D5A) || base.CheckSignature(header);
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DosModule, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new DosModule(stream), args);
        }
    }
}
