using IS4.MultiArchiver.Services;
using MetadataExtractor.Util;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class MetadataExtractorFormat : FileFormat<FileType>
    {
        public MetadataExtractorFormat() : base(0, null, null)
        {

        }

        public override string GetExtension(FileType value)
        {
            return value.GetCommonExtension();
        }

        public override string GetMediaType(FileType value)
        {
            return value.GetMimeType();
        }

        public override TResult Match<TResult>(Stream stream, Func<FileType, TResult> resultFactory)
        {
            return resultFactory(FileTypeDetector.DetectFileType(stream));
        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }
    }
}
