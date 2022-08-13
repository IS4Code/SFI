using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// The OGG audio format.
    /// </summary>
    public class OggFormat : SignatureFormat<WaveStream>
    {
        /// <summary>
        /// Creates a new instance of the format.
        /// </summary>
        public OggFormat() : base("OggS", "application/ogg", "ogg")
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = new NAudio.Vorbis.VorbisWaveReader(stream, false))
            {
                return await resultFactory(reader, args);
            }
        }
    }
}
