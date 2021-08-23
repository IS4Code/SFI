using IS4.MultiArchiver.Services;
using NAudio.Wave;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IS4.MultiArchiver.Formats
{
    public class WaveFormat : SignatureFormat<WaveStream>
    {
        public WaveFormat() : base(17, "RIFF", "audio/vnd.wave", "wav")
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(header.Length < HeaderLength || !base.CheckHeader(header, isBinary, encodingDetector))
            {
                return false;
            }
            return MemoryMarshal.Cast<byte, uint>(header)[2] == 0x45564157;
        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<WaveStream, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = new WaveFileReader(stream))
            {
                return resultFactory(reader, args);
            }
        }
    }
}
