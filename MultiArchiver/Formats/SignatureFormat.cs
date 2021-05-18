using IS4.MultiArchiver.Services;
using System;
using System.Text;

namespace IS4.MultiArchiver.Formats
{
    public abstract class SignatureFormat<T> : BinaryFileFormat<T> where T : class
    {
        readonly byte[] signature;

        public SignatureFormat(int headerLength, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            signature = Array.Empty<byte>();
        }

        public SignatureFormat(int headerLength, string signature, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            this.signature = Encoding.ASCII.GetBytes(signature);
        }

        public SignatureFormat(int headerLength, byte[] signature, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            this.signature = (byte[])signature.Clone();
        }

        public SignatureFormat(string signature, string mediaType, string extension) : base(signature.Length + 1, mediaType, extension)
        {
            this.signature = Encoding.ASCII.GetBytes(signature);
        }

        public SignatureFormat(byte[] signature, string mediaType, string extension) : base(signature.Length + 1, mediaType, extension)
        {
            this.signature = (byte[])signature.Clone();
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return header.Length > signature.Length && header.StartsWith(signature);
        }
    }
}
