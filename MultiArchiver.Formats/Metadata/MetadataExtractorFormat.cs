using IS4.MultiArchiver.Services;
using MetadataExtractor.Util;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents a format using <see cref="FileTypeDetector.DetectFileType(Stream)"/>
    /// to detect the format, producing instances of <see cref="FileTypeWrapper"/>.
    /// </summary>
    public class MetadataExtractorFormat : BinaryFileFormat<MetadataExtractorFormat.FileTypeWrapper>
    {
        public MetadataExtractorFormat() : base(0, null, null)
        {

        }

        public override string GetExtension(FileTypeWrapper value)
        {
            return value.Value.GetCommonExtension();
        }

        public override string GetMediaType(FileTypeWrapper value)
        {
            return value.Value.GetMimeType();
        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<FileTypeWrapper, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new FileTypeWrapper(FileTypeDetector.DetectFileType(stream)), args);
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
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
