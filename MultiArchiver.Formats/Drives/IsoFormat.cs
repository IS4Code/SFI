using DiscUtils.Iso9660;
using IS4.MultiArchiver.Services;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class IsoFormat : BinaryFileFormat<CDReader>
    {
        public IsoFormat() : base(0, "application/x-iso9660-image", "iso")
        {

        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return isBinary;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return isBinary;
        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<CDReader, TResult> resultFactory)
        {
            using(var reader = new CDReader(stream, true))
            {
                return resultFactory(reader);
            }
        }
    }
}
