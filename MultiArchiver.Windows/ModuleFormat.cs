using IS4.MultiArchiver.Services;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.MultiArchiver.Formats
{
    public abstract class ModuleFormat<T> : SignatureFormat<T> where T : class
    {
        const int CommonMaxStubSize = 256;

        readonly byte[] signature;

        readonly bool isPlain;

        public ModuleFormat(string signature, string mediaType, string extension) : base(CommonMaxStubSize + 1, "MZ", mediaType, extension)
        {
            isPlain = signature == null;
            this.signature = isPlain ? null : Encoding.ASCII.GetBytes(signature);
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(!base.CheckHeader(header, isBinary, encodingDetector))
            {
                return false;
            }
            var fields = MemoryMarshal.Cast<byte, ushort>(header);
            if(fields.Length <= 30)
            {
                // There is no e_lfanew field
                return isPlain;
            }
            var e_lfanew = fields[30];
            if(e_lfanew <= 1)
            {
                // MZ is there
                return isPlain;
            }
            if(header.Length < HeaderLength && e_lfanew >= header.Length - this.signature.Length)
            {
                // Points past the end of file
                return isPlain;
            }
            if(e_lfanew >= header.Length || isPlain)
            {
                // We didn't read this far
                return true;
            }
            var signature = header.Slice(e_lfanew);
            var testSignature = new Span<byte>(this.signature);
            int len = Math.Min(signature.Length, testSignature.Length);
            return signature.Slice(0, len).SequenceEqual(testSignature.Slice(0, len));
        }
    }
}
