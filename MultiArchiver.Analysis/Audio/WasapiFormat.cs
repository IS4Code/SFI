using IS4.MultiArchiver.Services;
using NAudio.Wave;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class WasapiFormat : BinaryFileFormat<WaveStream>
    {
        public WasapiFormat() : base(0, null, null)
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return isBinary;
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return isBinary;
        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<WaveStream, TResult> resultFactory)
        {
            using(var reader = new StreamMediaFoundationReader(stream))
            {
                return resultFactory(reader);
            }
        }
    }
}
