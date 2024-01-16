using IS4.SFI.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// A general image format, producing instances of <see cref="IImage{TUnderlying}"/> of <see cref="Image"/>.
    /// </summary>
    [Description("A general image format.")]
    public class ImageFormat : BinaryFileFormat<IImage<Image>>, IFileFormat<Image>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ImageFormat() : base(0, null, null)
        {

        }

        /// <inheritdoc/>
        public string? GetExtension(Image image)
        {
            var extension = GetCodec(image.RawFormat)?.FilenameExtension;
            if(extension == null) return null;
            int next = extension.IndexOf(';');
            if(next != -1) extension = extension.Substring(2, next - 2);
            else extension = extension.Substring(2);
            return extension.ToLowerInvariant();
        }

        /// <inheritdoc/>
        public string? GetMediaType(Image image)
        {
            return GetCodec(image.RawFormat)?.MimeType;
        }

        /// <inheritdoc/>
        public override string? GetMediaType(IImage<Image> image)
        {
            return GetMediaType(image.UnderlyingImage);
        }

        /// <inheritdoc/>
        public override string? GetExtension(IImage<Image> image)
        {
            return GetExtension(image.UnderlyingImage);
        }

        public static ImageCodecInfo? GetOutputEncoder(string mediaType)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.MimeType.Equals(mediaType, StringComparison.OrdinalIgnoreCase));
        }

        private ImageCodecInfo? GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IImage<Image>, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var image = Image.FromStream(stream);
            return await resultFactory(new DrawingImage(image, this), args);
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
    }
}
