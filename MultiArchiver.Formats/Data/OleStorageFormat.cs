using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using OpenMcdf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the OLE storage format, producing instances of <see cref="CompoundFile"/>.
    /// </summary>
    public class OleStorageFormat : BinaryFileFormat<CompoundFile>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public OleStorageFormat() : base(8, "application/x-ole-storage", "ole")
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(header.Length < HeaderLength || header.MemoryCast<ulong>()[0] != 0xE11AB1A1E011CFD0)
            {
                return false;
            }
            return true;
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CompoundFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var file = new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.NoValidationException))
            {
                return await resultFactory(file, args);
            }
        }
    }
}
