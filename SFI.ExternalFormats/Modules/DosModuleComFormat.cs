using IS4.SFI.Formats.Modules;
using IS4.SFI.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the MS-DOS COM executable format, producing instances of
    /// <see cref="DosModule"/>.
    /// </summary>
    public class DosModuleComFormat : BinaryFileFormat<DosModule>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DosModuleComFormat() : base(1, "application/x-dosexec", "com")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return header.Length > 0 && isBinary;
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return header.Count > 0 && isBinary;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<DosModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var file = context.GetService<IFileNodeInfo>();
            if(file != null && Path.GetExtension(file.Name).Equals(".com", StringComparison.OrdinalIgnoreCase))
            {
                return await resultFactory(new DosModule(stream), args);
            }
            return default;
        }
    }
}
