using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Text;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a format of DOS/Windows MZ modules as instances of <typeparamref name="T"/>.
    /// The implementation of <see cref="CheckHeader(ReadOnlySpan{byte}, bool, IEncodingDetector)"/>
    /// checks the signature in the extended MZ header.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="IFileFormat{T}" path="/typeparam[@name='T']"/></typeparam>
    public abstract class ModuleFormat<T> : SignatureFormat<T> where T : class
    {
        const int CommonMaxStubSize = 256;

        readonly byte[]? signature;

        readonly bool isPlain;

        /// <inheritdoc/>
        public ModuleFormat(string? signature, string? mediaType, string? extension) : base(CommonMaxStubSize + 1, "MZ", mediaType, extension)
        {
            isPlain = signature == null;
            this.signature = isPlain ? null : Encoding.ASCII.GetBytes(signature);
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(!base.CheckHeader(header, isBinary, encodingDetector))
            {
                return false;
            }
            var fields = header.MemoryCast<ushort>();
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
            if(header.Length < HeaderLength && e_lfanew >= header.Length - (this.signature?.Length ?? 0))
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
            var testSignature = this.signature.AsSpan();
            int len = Math.Min(signature.Length, testSignature.Length);
            return signature.Slice(0, len).SequenceEqual(testSignature.Slice(0, len));
        }
    }
}
