using IS4.MultiArchiver.Services;
using System;
using System.Text;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// An extension of <see cref="BinaryFileFormat{T}"/> for formats identified
    /// using sequence of bytes at the start of the data, i.e. a signature.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SignatureFormat<T> : BinaryFileFormat<T> where T : class
    {
        readonly byte[] signature;

        /// <summary>
        /// Creates a new instance of the format from an empty signature.
        /// </summary>
        /// <param name="headerLength">The minimum required length of the header.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SignatureFormat(int headerLength, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            signature = Array.Empty<byte>();
        }

        /// <summary>
        /// Creates a new instance of the format from a <see cref="String"/> signature.
        /// </summary>
        /// <param name="headerLength">The minimum required length of the header.</param>
        /// <param name="signature">A string representation of the signature, converted to ASCII.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SignatureFormat(int headerLength, string signature, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            this.signature = Encoding.ASCII.GetBytes(signature);
        }

        /// <summary>
        /// Creates a new instance of the format from a <see cref="Byte"/> array signature.
        /// </summary>
        /// <param name="headerLength">The minimum required length of the header.</param>
        /// <param name="signature">The raw bytes of the signature.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SignatureFormat(int headerLength, byte[] signature, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {
            this.signature = (byte[])signature.Clone();
        }

        /// <summary>
        /// Creates a new instance of the format from a <see cref="String"/> signature.
        /// </summary>
        /// <param name="signature">A string representation of the signature, converted to ASCII.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SignatureFormat(string signature, string mediaType, string extension) : base(signature.Length + 1, mediaType, extension)
        {
            this.signature = Encoding.ASCII.GetBytes(signature);
        }

        /// <summary>
        /// Creates a new instance of the format from a <see cref="Byte"/> array signature.
        /// </summary>
        /// <param name="signature">The raw bytes of the signature.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SignatureFormat(byte[] signature, string mediaType, string extension) : base(signature.Length + 1, mediaType, extension)
        {
            this.signature = (byte[])signature.Clone();
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return CheckSignature(header);
        }

        /// <summary>
        /// In the default implementation, checks that the sequence of bytes
        /// provided in <paramref name="header"/> is longer than the signature
        /// stored by the format and starts with it.
        /// </summary>
        /// <param name="header">A collection of bytes from the beginning of the file.</param>
        /// <returns>True if the signature in the header matches.</returns>
        protected virtual bool CheckSignature(Span<byte> header)
        {
            return header.Length > signature.Length && header.StartsWith(signature);
        }
    }
}
