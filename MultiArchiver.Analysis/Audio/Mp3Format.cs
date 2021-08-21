using IS4.MultiArchiver.Services;
using NAudio.Wave;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class Mp3Format : BinaryFileFormat<WaveStream>
    {
        readonly Mp3FileReaderBase.FrameDecompressorBuilder frameDecompressorBuilder;

        public Mp3Format(Mp3FileReaderBase.FrameDecompressorBuilder frameDecompressorBuilder) : base(0, "audio/mpeg", "mp3")
        {
            this.frameDecompressorBuilder = frameDecompressorBuilder;
        }

        public override TResult Match<TResult, TArgs>(Stream stream, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = new Mp3FileReaderBase(stream, frameDecompressorBuilder))
            {
                return resultFactory(reader, args);
            }
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }
    }
}
