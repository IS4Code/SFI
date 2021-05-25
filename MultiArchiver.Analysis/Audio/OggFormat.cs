using NAudio.Wave;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class OggFormat : SignatureFormat<WaveStream>
    {
        public OggFormat() : base("OggS", "application/ogg", "ogg")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<WaveStream, TResult> resultFactory)
        {
            using(var reader = new NAudio.Vorbis.VorbisWaveReader(stream, false))
            {
                return resultFactory(reader);
            }
        }
    }
}
