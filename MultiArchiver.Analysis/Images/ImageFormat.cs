using IS4.MultiArchiver.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// An image format, producing instances of <see cref="Image"/>.
    /// </summary>
    public class ImageFormat : BinaryFileFormat<Image>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
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

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Image, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var image = Image.FromStream(stream))
            {
                return await resultFactory(image, args);
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
