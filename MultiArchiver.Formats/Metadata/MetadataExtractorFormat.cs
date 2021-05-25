using IS4.MultiArchiver.Services;
using MetadataExtractor.Util;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
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

        public override TResult Match<TResult>(Stream stream, ResultFactory<FileTypeWrapper, TResult> resultFactory)
        {
            return resultFactory(new FileTypeWrapper(FileTypeDetector.DetectFileType(stream)));
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public class FileTypeWrapper
        {
            public FileType Value { get; }

            public FileTypeWrapper(FileType value)
            {
                Value = value;
            }
        }
    }
}
