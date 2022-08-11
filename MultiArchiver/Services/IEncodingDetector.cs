using System;

namespace IS4.MultiArchiver.Services
{
    /// <summary>
    /// The common interface used by a detector of an input file's charset.
    /// Once initialized, the instance shall receive consecutive chunks of the
    /// input data via <see cref="Write(ArraySegment{byte})"/> or
    /// <see cref="Write(Span{byte})"/> until its and, signalized
    /// by calling <see cref="End"/>.
    /// </summary>
    public interface IEncodingDetector
    {
        /// <summary>
        /// The recognized character set. May not be set before a call to <see cref="End"/>.
        /// </summary>
        string Charset { get; }

        /// <summary>
        /// The confidence of the resulting <see cref="Charset"/>.
        /// </summary>
        float Confidence { get; }

        /// <summary>
        /// Gives additional data to the detector.
        /// </summary>
        /// <param name="data">The next collection of bytes of the data.</param>
        void Write(ArraySegment<byte> data);

        /// <summary>
        /// Gives additional data to the detector.
        /// </summary>
        /// <param name="data">The next collection of bytes of the data.</param>
        void Write(Span<byte> data);

        /// <summary>
        /// Indicates that the data is at its end.
        /// </summary>
        void End();
    }
}
