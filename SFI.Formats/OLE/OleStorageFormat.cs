using IS4.SFI.Services;
using IS4.SFI.Tools;
using OpenMcdf;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the OLE storage format, producing instances of <see cref="CompoundFile"/>.
    /// </summary>
    [Description("Represents the OLE storage format.")]
    public class OleStorageFormat : BinaryFileFormat<CompoundFile>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public OleStorageFormat() : base(8, "application/x-ole-storage", "ole")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(header.Length < HeaderLength || header.MemoryCast<ulong>()[0] != 0xE11AB1A1E011CFD0)
            {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CompoundFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var file = new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.NoValidationException);
            return await resultFactory(file, args);
        }
    }
}
