using IS4.SFI.Services;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// The MP3 audio format.
    /// </summary>
    [Browsable(false)]
    [Description("The MP3 audio format.")]
    public class Mp3Format : BinaryFileFormat<WaveStream>
    {
        readonly Mp3FileReaderBase.FrameDecompressorBuilder frameDecompressorBuilder;

        /// <param name="frameDecompressorBuilder">
        /// The instance of <see cref="Mp3FileReaderBase.FrameDecompressorBuilder"/> to
        /// use during decompression by the internal
        /// <see cref="Mp3FileReaderBase.Mp3FileReaderBase(Stream, Mp3FileReaderBase.FrameDecompressorBuilder)"/>.
        /// </param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Mp3Format(Mp3FileReaderBase.FrameDecompressorBuilder frameDecompressorBuilder) : base(0, "audio/mpeg", "mp3")
        {
            this.frameDecompressorBuilder = frameDecompressorBuilder;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = new Mp3FileReaderBase(stream, frameDecompressorBuilder);
            return await resultFactory(reader, args);
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return true;
        }
    }
}
