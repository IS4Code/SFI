using IS4.MultiArchiver.Services;
using System;
using System.Buffers;
using Ude;

namespace IS4.MultiArchiver.Tools
{
    public class UdeEncodingDetector : IEncodingDetector
    {
        readonly CharsetDetector detector;

        public UdeEncodingDetector() : this(new CharsetDetector())
        {

        }

        public UdeEncodingDetector(CharsetDetector detector)
        {
            this.detector = detector;
        }

        public string Charset => detector.Charset;

        public float Confidence => detector.Confidence;

        public void Write(ArraySegment<byte> data)
        {
            detector.Feed(data.Array, data.Offset, data.Count);
        }

        public void Write(Span<byte> data)
        {
            var array = ArrayPool<byte>.Shared.Rent(data.Length);
            try{
                data.CopyTo(array);
                Write(new ArraySegment<byte>(array, 0, data.Length));
            }finally{
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public void End()
        {
            detector.DataEnd();
        }
    }
}
