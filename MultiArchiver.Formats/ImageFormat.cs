using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class ImageFormat : IFileLoader
    {
        public int HeaderLength => 0;

        public string MediaType => "image";

        public string Extension => null;

        public string GetExtension(IDisposable value)
        {
            if(!(value is Image image)) throw new ArgumentException(null, nameof(value));
            return GetCodec(image.RawFormat)?.FilenameExtension;
        }

        public string GetMediaType(IDisposable value)
        {
            if(!(value is Image image)) throw new ArgumentException(null, nameof(value));
            return GetCodec(image.RawFormat)?.MimeType;
        }

        private ImageCodecInfo GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public IDisposable Match(Stream stream)
        {
            stream = new ReadSeekableStream(stream, 1024);
            var metadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(stream);
            return Image.FromStream(stream);
        }

        public bool Match(ArraySegment<byte> header)
        {
            return true;
        }

        public bool Match(Span<byte> header)
        {
            return true;
        }
    }
}
