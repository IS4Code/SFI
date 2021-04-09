using IS4.MultiArchiver.Services;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ImageMetadataFormat : FileLoader<IReadOnlyList<MetadataExtractor.Directory>>
    {
        public ImageMetadataFormat() : base(0, "image", null)
        {

        }

        /*public override string GetExtension(IReadOnlyList<MetadataExtractor.Directory> value)
        {
            return null;
        }

        public override string GetMediaType(IReadOnlyList<MetadataExtractor.Directory> value)
        {
            return "image";
        }*/

        public override IReadOnlyList<MetadataExtractor.Directory> Match(Stream stream)
        {
            return ImageMetadataReader.ReadMetadata(stream);
        }

        public override bool Match(ArraySegment<byte> header)
        {
            return true;
        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }
    }
}
