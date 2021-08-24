using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using OpenMcdf;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class OleStorageFormat : BinaryFileFormat<CompoundFile>
    {
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

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CompoundFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var file = new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.NoValidationException))
            {
                return resultFactory(file, args);
            }
        }
    }
}
