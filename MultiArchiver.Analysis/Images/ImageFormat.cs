using IS4.MultiArchiver.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class ImageFormat : BinaryFileFormat<Image>
    {
        public ImageFormat() : base(0, null, null)
        {

        }

        public override string GetExtension(Image image)
        {
            var extension = GetCodec(image.RawFormat)?.FilenameExtension;
            if(extension == null) return null;
            int next = extension.IndexOf(';');
            if(next != -1) extension = extension.Substring(2, next - 2);
            else extension = extension.Substring(2);
            return extension.ToLowerInvariant();
        }

        public override string GetMediaType(Image image)
        {
            return GetCodec(image.RawFormat)?.MimeType;
        }

        private ImageCodecInfo GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<Image, TResult> resultFactory)
        {
            using(var image = Image.FromStream(stream))
            {
                return resultFactory(image);
            }
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
