using IS4.SFI.Services;
using MetadataExtractor.Util;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents a format using <see cref="FileTypeDetector.DetectFileType(Stream)"/>
    /// to detect the format, producing instances of <see cref="FileTypeWrapper"/>.
    /// </summary>
    [Browsable(false)]
    public class MetadataExtractorFormat : BinaryFileFormat<MetadataExtractorFormat.FileTypeWrapper>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string?, string?)"/>
        public MetadataExtractorFormat() : base(0, null, null)
        {

        }

        /// <inheritdoc/>
        public override string? GetExtension(FileTypeWrapper value)
        {
            return value.Value.GetCommonExtension();
        }

        /// <inheritdoc/>
        public override string? GetMediaType(FileTypeWrapper value)
        {
            return value.Value.GetMimeType();
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<FileTypeWrapper, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(new FileTypeWrapper(FileTypeDetector.DetectFileType(stream)), args);
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return true;
        }

        /// <summary>
        /// Wraps a <see cref="FileType"/>
        /// </summary>
        public class FileTypeWrapper
        {
            /// <summary>
            /// The recognized type of the file.
            /// </summary>
            public FileType Value { get; }

            /// <summary>
            /// Creates a new instance of the wrapper.
            /// </summary>
            /// <param name="value">The value of <see cref="Value"/>.</param>
            public FileTypeWrapper(FileType value)
            {
                Value = value;
            }
        }
    }
}
