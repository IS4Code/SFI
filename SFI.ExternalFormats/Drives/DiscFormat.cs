using DiscUtils;
using IS4.SFI.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents an arbitrary disc image format, as an instance of <typeparamref name="TDisc"/>.
    /// </summary>
    public abstract class DiscFormat<TDisc> : BinaryFileFormat<TDisc> where TDisc : DiscFileSystem
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public DiscFormat(string? mediaType, string? extension) : base(0, mediaType, extension)
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return isBinary;
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return isBinary;
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="TDisc"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the instance from.</param>
        /// <returns>
        /// A <typeparamref name="TDisc"/> instance corresponding to the file system stored in the stream.
        /// </returns>
        protected abstract TDisc Create(Stream stream);

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<TDisc, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var disc = Create(stream);
            return await resultFactory(disc, args);
        }
    }
}
