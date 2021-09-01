using System;

namespace IS4.MultiArchiver.Services
{
    public interface IEncodingDetector
    {
        string Charset { get; }
        float Confidence { get; }
        void Write(ArraySegment<byte> data);
        void Write(Span<byte> data);
        void End();
    }
}
