using IS4.SFI.Services;
using System;
using System.Buffers;
using Ude;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// An implementation of <see cref="IEncodingDetector"/>
    /// using <see cref="CharsetDetector"/>.
    /// </summary>
    public class UdeEncodingDetector : IEncodingDetector
    {
        readonly CharsetDetector detector;

        /// <summary>
        /// Creates a new instance of the detector.
        /// </summary>
        public UdeEncodingDetector() : this(new CharsetDetector())
        {

        }

        /// <summary>
        /// Creates a new instance of the detector.
        /// </summary>
        /// <param name="detector">The inner detector to use.</param>
        public UdeEncodingDetector(CharsetDetector detector)
        {
            this.detector = detector;
        }

        /// <inheritdoc/>
        public string Charset => detector.Charset;

        /// <inheritdoc/>
        public float Confidence => detector.Confidence;

        /// <inheritdoc/>
        public void Write(ArraySegment<byte> data)
        {
            detector.Feed(data.Array, data.Offset, data.Count);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> data)
        {
            using var arrayLease = ArrayPool<byte>.Shared.Rent(data.Length, out var array);
            data.CopyTo(array);
            Write(array.Slice(0, data.Length));
        }

        /// <inheritdoc/>
        public void End()
        {
            detector.DataEnd();
        }
    }
}
