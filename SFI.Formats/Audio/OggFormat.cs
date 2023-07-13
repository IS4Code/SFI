using NAudio.Wave;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// The OGG audio format.
    /// </summary>
    [Description("The OGG audio format.")]
    public class OggFormat : SignatureFormat<WaveStream>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public OggFormat() : base("OggS", "application/ogg", "ogg")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = new NAudio.Vorbis.VorbisWaveReader(stream, false);
            return await resultFactory(reader, args);
        }
    }
}
