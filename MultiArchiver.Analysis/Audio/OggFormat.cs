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

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = new NAudio.Vorbis.VorbisWaveReader(stream, false))
            {
                return resultFactory(reader, args);
            }
        }
    }
}
