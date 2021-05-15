using IS4.MultiArchiver.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class ImageMetadataFormat : BinaryFileFormat<IReadOnlyList<MetadataExtractor.Directory>>
    {
        public ImageMetadataFormat() : base(0, null, null)
        {

        }

        public override string GetExtension(IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            return GetFileTag(metadata, FileTypeDirectory.TagExpectedFileNameExtension) ?? base.GetExtension(metadata);
        }

        public override string GetMediaType(IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            return GetFileTag(metadata, FileTypeDirectory.TagDetectedFileMimeType) ?? base.GetMediaType(metadata);
        }

        private string GetFileTag(IReadOnlyList<MetadataExtractor.Directory> metadata, int tag)
        {
            return metadata.OfType<FileTypeDirectory>().FirstOrDefault()?.GetString(tag);
        }

        public override TResult Match<TResult>(Stream stream, Func<IReadOnlyList<MetadataExtractor.Directory>, TResult> resultFactory)
        {
            return resultFactory(ImageMetadataReader.ReadMetadata(stream));
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }
    }
}
