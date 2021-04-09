using IS4.MultiArchiver.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class ImageFormat : FileLoader<Image>
    {
        public ImageFormat() : base(0, "image", null)
        {

        }

        public override string GetExtension(Image image)
        {
            return GetCodec(image.RawFormat)?.FilenameExtension;
        }

        public override string GetMediaType(Image image)
        {
            return GetCodec(image.RawFormat)?.MimeType;
        }

        private ImageCodecInfo GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public override Image Match(Stream stream)
        {
            return Image.FromStream(stream);
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
