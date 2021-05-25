using IS4.MultiArchiver.Services;
using NAudio.Wave;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class OggFormat : SignatureFormat<WaveStream>
    {
        public OggFormat() : base("OggS", "application/ogg", "ogg")
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<WaveStream, TResult> resultFactory)
        {
            using(var reader = new NAudio.Vorbis.VorbisWaveReader(stream, false))
            {
                return resultFactory(reader);
            }
        }
    }
}
