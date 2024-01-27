using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace IS4.SFI.Tools.Images
{
    /// <summary>
    /// Provides an <see cref="IImage{TUnderlying}"/> implementation
    /// backed by an instance of <see cref="Image"/>.
    /// </summary>
    public class DrawingImage : ImageBase<Image>
    {
        Bitmap Bitmap => UnderlyingImage as Bitmap ?? throw new NotSupportedException();

        /// <inheritdoc/>
        public DrawingImage(Image underlyingImage, IFileFormat<Image> format) : base(underlyingImage, format)
        {

        }

        /// <inheritdoc/>
        public override int Width => UnderlyingImage.Width;

        /// <inheritdoc/>
        public override int Height => UnderlyingImage.Height;

        /// <inheritdoc/>
        public override double HorizontalResolution => UnderlyingImage.HorizontalResolution;

        /// <inheritdoc/>
        public override double VerticalResolution => UnderlyingImage.VerticalResolution;

        /// <inheritdoc/>
        public override bool HasAlpha => Image.IsAlphaPixelFormat(UnderlyingImage.PixelFormat);

        /// <inheritdoc/>
        public override int BitDepth => Image.GetPixelFormatSize(UnderlyingImage.PixelFormat);

        /// <inheritdoc/>
        public override IReadOnlyList<Color> Palette {
            get {
                try
                {
                    return UnderlyingImage.Palette?.Entries ?? Array.Empty<Color>();
                } catch(ExternalException)
                {
                    return Array.Empty<Color>();
                }
            }
        }

        /// <inheritdoc/>
        public override IImageTag? Tag {
            get => (UnderlyingImage.Tag as IImageTag) ?? base.Tag;
            set => UnderlyingImage.Tag = base.Tag = value;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<object, object>> Metadata => UnderlyingImage.GetMetadata();

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata => UnderlyingImage.GetRawMetadata();

        /// <inheritdoc/>
        public override Color GetPixel(int x, int y)
        {
            return Bitmap.GetPixel(x, y);
        }

        /// <inheritdoc/>
        public override void SetPixel(int x, int y, Color color)
        {
            Bitmap.SetPixel(x, y, color);
        }

        /// <inheritdoc/>
        public override IImageData GetData()
        {
            var bmp = Bitmap;
            var bits = BitDepth;
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bits == 0 ? PixelFormat.Format32bppArgb : bmp.PixelFormat);
            return new DrawingImageData(this, data);
        }

        static readonly Image.GetThumbnailImageAbort abortUnused = delegate { return false; };

        /// <inheritdoc/>
        public override IImage? GetThumbnail()
        {
            var thumbnail = UnderlyingImage.GetThumbnailImage(0, 0, abortUnused, IntPtr.Zero);
            if(thumbnail == null)
            {
                return null;
            }
            return new DrawingImage(thumbnail, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override void Save(Stream output, string mediaType)
        {
            var encoder = Formats.ImageFormat.GetOutputEncoder(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            UnderlyingImage.Save(output, encoder, null);
        }

        /// <inheritdoc/>
        public override IImage Resize(int newWidth, int newHeight, bool preserveResolution, bool use32bppArgb, Color backgroundColor)
        {
            var resized = UnderlyingImage.Resize(newWidth, newHeight, use32bppArgb ? PixelFormat.Format32bppArgb : UnderlyingImage.PixelFormat, backgroundColor, preserveResolution);
            return new DrawingImage(resized, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override ImageBase<Image> Clone()
        {
            return new DrawingImage(new Bitmap(UnderlyingImage), UnderlyingFormat);
        }
    }

    /// <summary>
    /// Provides an <see cref="IImageData"/> implementation
    /// backed by an instance of <see cref="BitmapData"/>.
    /// </summary>
    public class DrawingImageData : MemoryImageData<Image>
    {
        BitmapData? _data;

        BitmapData data => _data ?? throw new ObjectDisposedException(nameof(DrawingImageData));

        /// <summary>
        /// Creates a new instance of the data.
        /// </summary>
        /// <param name="image">The value of <see cref="ImageData{TUnderlying}.Image"/>.</param>
        /// <param name="data">The instance of <see cref="BitmapData"/> to access the data of.</param>
        public DrawingImageData(DrawingImage image, BitmapData data) : base(image, data.Stride, System.Drawing.Image.GetPixelFormatSize(data.PixelFormat))
        {
            _data = data;
        }

        /// <inheritdoc/>
        public unsafe override Span<byte> GetSpan()
        {
            return new Span<byte>((data.Scan0 - Scan0).ToPointer(), Size);
        }

        /// <inheritdoc/>
        protected override Stream Open()
        {
            return new BitmapDataStream(data.Scan0, Stride, data.Height, Width, BitDepth);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(Interlocked.Exchange(ref _data, null) is { } bmpData)
            {
                ((Bitmap)Image.UnderlyingImage).UnlockBits(bmpData);
            }
        }
    }
}
