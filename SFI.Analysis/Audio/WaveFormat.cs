using IS4.SFI.Services;
using IS4.SFI.Tools;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the WAV format.
    /// </summary>
    public class WaveFormat : SignatureFormat<WaveStream>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WaveFormat() : base(17, "RIFF", "audio/vnd.wave", "wav")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(header.Length < HeaderLength || !base.CheckHeader(header, isBinary, encodingDetector))
            {
                return false;
            }
            return header.MemoryCast<uint>()[2] == 0x45564157;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using(var reader = new WaveFileReader(stream))
            {
                return await resultFactory(reader, args);
            }
        }
    }
}
