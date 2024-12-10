using IS4.SFI.Services;
using IS4.SFI.Tools.Images;
using SixLabors.ImageSharp.Formats;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ISharpImage = SixLabors.ImageSharp.IImage;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// A general image format, producing instances of <see cref="IImage{TUnderlying}"/> of
    /// <see cref="Image"/> or <see cref="ISharpImage"/>, based on <see cref="UseImageSharp"/>.
    /// </summary>
    [Description("A general image format.")]
    public class ImageFormat : BinaryFileFormat<IImage>, IFileFormat<Image>, IFileFormat<ISharpImage>, ISupportInitialize
    {
        /// <summary>
        /// Whether to prefer the ImageSharp library to load images.
        /// </summary>
        [Description("Whether to prefer the ImageSharp library to load images.")]
        public bool PreferImageSharp { get; set; } = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Whether to allow the ImageSharp library to load images.
        /// </summary>
        [Description("Whether to allow the ImageSharp library to load images.")]
        public bool AllowImageSharp { get; set; } = true;

        /// <summary>
        /// Whether to allow the native System.Drawing library to load images.
        /// </summary>
        [Description("Whether to allow the native System.Drawing library to load images.")]
        public bool AllowNativeImage { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Whether to use the ImageSharp library to load images.
        /// </summary>
#pragma warning disable CS0618
        [Obsolete("The " + nameof(UseImageSharp) + " property was renamed to " + nameof(PreferImageSharp) + ".", false)]
#pragma warning restore CS0618
        [Description("Whether to use the ImageSharp library to load images.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool UseImageSharp {
            get => PreferImageSharp;
            set => PreferImageSharp = value;
        }

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ImageFormat() : base(0, null, null)
        {

        }

        /// <inheritdoc/>
        public override string? GetMediaType(IImage image)
        {
            switch(image)
            {
                case IImage<Image> { UnderlyingImage: var nativeImage }:
                    return GetMediaType(nativeImage);
                case IImage<ISharpImage> { UnderlyingImage: var sharpImage }:
                    return GetMediaType(sharpImage);
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public override string? GetExtension(IImage image)
        {
            switch(image)
            {
                case IImage<Image> { UnderlyingImage: var nativeImage }:
                    return GetExtension(nativeImage);
                case IImage<ISharpImage> { UnderlyingImage: var sharpImage }:
                    return GetExtension(sharpImage);
                default:
                    return null;
            }
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
        public string? GetExtension(ISharpImage image)
        {
            return storedFormats.TryGetValue(image, out var format)
                ? format.FileExtensions.FirstOrDefault(ext => ext.Length >= 3) ?? format.FileExtensions.FirstOrDefault()
                : null;
        }

        /// <inheritdoc/>
        public string? GetMediaType(ISharpImage image)
        {
            return storedFormats.TryGetValue(image, out var format) ? format.DefaultMimeType : null;
        }

        /// <summary>
        /// Stores the decoded format of an <see cref="ISharpImage"/> instance.
        /// </summary>
        static readonly ConditionalWeakTable<ISharpImage, IImageFormat> storedFormats = new();

        public static ImageCodecInfo? GetOutputEncoder(string mediaType)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.MimeType.Equals(mediaType, StringComparison.OrdinalIgnoreCase));
        }

        private ImageCodecInfo? GetCodec(System.Drawing.Imaging.ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        static readonly SixLabors.ImageSharp.Configuration loadConfiguration = CreateConfiguration();

        static SixLabors.ImageSharp.Configuration CreateConfiguration()
        {
            var clone = SixLabors.ImageSharp.Configuration.Default.Clone();
            clone.PreferContiguousImageBuffers = true;
            return clone;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IImage, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            bool allowNative = AllowNativeImage;
            bool allowImageSharp = AllowImageSharp;
            bool preferImageSharp = PreferImageSharp;
            try{
                if(allowNative && !preferImageSharp)
                {
                    using var drawingImage = Image.FromStream(stream);
                    return await resultFactory(new DrawingImage(drawingImage, this), args);
                }
            }catch(PlatformNotSupportedException) when(allowImageSharp)
            {
                AllowNativeImage = false;
            }catch(Exception e) when (allowImageSharp && e is ArgumentException or OutOfMemoryException)
            {

            }

            try{
                var result = await SixLabors.ImageSharp.Image.LoadWithFormatAsync(loadConfiguration, stream);
                using var image = result.Image;
                storedFormats.Add(image, result.Format);
                return await resultFactory(new SharpImage(image, this), args);
            }catch(Exception e) when (preferImageSharp && allowNative && e is NotSupportedException or SixLabors.ImageSharp.UnknownImageFormatException or SixLabors.ImageSharp.InvalidImageContentException)
            {
                using var drawingImage = Image.FromStream(stream);
                return await resultFactory(new DrawingImage(drawingImage, this), args);
            }
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

        void ISupportInitialize.BeginInit()
        {

        }

        void ISupportInitialize.EndInit()
        {
            if(!AllowImageSharp && !AllowNativeImage)
            {
                throw new ApplicationException($"Both {nameof(AllowImageSharp)} and {nameof(AllowNativeImage)} are disabled.");
            }
            if(PreferImageSharp && !AllowImageSharp)
            {
                throw new ApplicationException($"{nameof(PreferImageSharp)} is set but ImageSharp is disabled.");
            }
        }
    }
}
