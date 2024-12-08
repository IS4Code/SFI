using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools.IO;
using Microsoft.CSharp.RuntimeBinder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Color = System.Drawing.Color;
using IImage = SixLabors.ImageSharp.IImage;

namespace IS4.SFI.Tools.Images
{
    /// <summary>
    /// Provides an <see cref="IImage{TUnderlying}"/> implementation
    /// backed by an instance of <see cref="IImage"/>.
    /// </summary>
    public class SharpImage : ImageBase<IImage>
    {
        Image Image => UnderlyingImage as Image ?? throw new NotSupportedException();

        ImageMetadata ImageMetadata => UnderlyingImage.Metadata;

        PixelTypeInfo PixelType => UnderlyingImage.PixelType;

        /// <inheritdoc/>
        public SharpImage(IImage underlyingImage, IFileFormat<IImage> format) : base(underlyingImage, format)
        {

        }

        /// <inheritdoc/>
        public override int Width => UnderlyingImage.Width;

        /// <inheritdoc/>
        public override int Height => UnderlyingImage.Height;

        /// <inheritdoc/>
        public override double HorizontalResolution => ResolutionUnit * ImageMetadata.HorizontalResolution;

        /// <inheritdoc/>
        public override double VerticalResolution => ResolutionUnit * ImageMetadata.VerticalResolution;

        /// <summary>
        /// The coefficient to use when converting resolution to pixel per inch.
        /// </summary>
        private double ResolutionUnit{
            get{
                switch(ImageMetadata.ResolutionUnits)
                {
                    case PixelResolutionUnit.AspectRatio:
                        return 96;
                    case PixelResolutionUnit.PixelsPerInch:
                        return 1;
                    case PixelResolutionUnit.PixelsPerCentimeter:
                        return 2.54d;
                    case PixelResolutionUnit.PixelsPerMeter:
                        return 0.0254d;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <inheritdoc/>
        public override bool HasAlpha => PixelType.AlphaRepresentation is not (null or 0);

        /// <inheritdoc/>
        public override int BitDepth{
            get{
                if(ImageMetadata.GetPngMetadata() is { BitDepth: { } pngBits and not 0 } png)
                {
                    if(png.ColorType is PngColorType.Rgb or PngColorType.RgbWithAlpha)
                    {
                        return PixelType.BitsPerPixel;
                    }
                    return (int)pngBits;
                }
                if(ImageMetadata.GetGifMetadata() is { GlobalColorTableLength: var palSize and > 0 })
                {
                    return (int)Math.Ceiling(Math.Log(palSize / 3, 2));
                }
                if(ImageMetadata.GetBmpMetadata() is { InfoHeaderType: not 0, BitsPerPixel: var bmpBits and > 0 })
                {
                    return (int)bmpBits;
                }
                return PixelType.BitsPerPixel;
            }
        }

        /// <inheritdoc/>
        public override bool? IsLossless{
            get{
                if(ImageMetadata.GetWebpMetadata() is { FileFormat: { } webpFormat })
                {
                    return webpFormat switch
                    {
                        SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless => true,
                        SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossy => false,
                        _ => null
                    };
                }
                return null;
            }
        }

        static readonly Type pixelType32bppArgb = typeof(Bgra32);

        /// <inheritdoc/>
        public override bool Is32bppArgb {
            [DynamicDependency(nameof(GetImagePixelType) + "``1", typeof(SharpImage))]
            get{
                try{
                    return pixelType32bppArgb.Equals(GetImagePixelType((dynamic)UnderlyingImage));
                }catch(RuntimeBinderException)
                {
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Color> Palette => Array.Empty<Color>();

        /// <inheritdoc/>
        public override int? PaletteSize{
            get{
                if(ImageMetadata.GetPngMetadata() is { BitDepth: { } pngBits and not 0 } png)
                {
                    switch(png.ColorType)
                    {
                        case PngColorType.Palette:
                        case PngColorType.Grayscale:
                        case PngColorType.GrayscaleWithAlpha:
                            return (int)pngBits <= 8 ? (1 << (int)pngBits) : 0;
                        case PngColorType.Rgb:
                        case PngColorType.RgbWithAlpha:
                            return 0;
                        default:
                            return null;
                    }
                }
                if(ImageMetadata.GetGifMetadata() is { GlobalColorTableLength: var fromGif and > 0 })
                {
                    return fromGif / 3;
                }
                if(ImageMetadata.GetBmpMetadata() is { BitsPerPixel: var bmpBits and > 0 })
                {
                    return (int)bmpBits <= 8 ? (1 << (int)bmpBits) : 0;
                }
                if(ImageMetadata.GetJpegMetadata() != null)
                {
                    return 0;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<object, object>> Metadata => UnderlyingImage.GetMetadata();

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata => UnderlyingImage.GetRawMetadata();

        /// <summary>
        /// Retrieves the pixel type of an image, as a type implementing <see cref="IPixel{TSelf}"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
        /// <returns>The pixel type of an image.</returns>
        protected static Type GetImagePixelType<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            return typeof(TPixel);
        }

        /// <inheritdoc/>
        [DynamicDependency(nameof(GetImagePixel) + "``1", typeof(SharpImage))]
        public override Color GetPixel(int x, int y)
        {
            try{
                return GetImagePixel((dynamic)UnderlyingImage, x, y);
            }catch(RuntimeBinderException)
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        [DynamicDependency(nameof(SetImagePixel) + "``1", typeof(SharpImage))]
        public override void SetPixel(int x, int y, Color color)
        {
            try{
                SetImagePixel((dynamic)UnderlyingImage, x, y, color);
            }catch(RuntimeBinderException)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Retrieves the color value of a particular pixel in an image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
        /// <param name="image">The image to access.</param>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at the specified coordinates.</returns>
        protected static Color GetImagePixel<TPixel>(Image<TPixel> image, int x, int y) where TPixel : unmanaged, IPixel<TPixel>
        {
            Rgba32 pixel = default;
            image[x, y].ToRgba32(ref pixel);
            return Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
        }

        /// <summary>
        /// Assigns the color value of a particular pixel in an image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
        /// <param name="image">The image to access.</param>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <param name="color">The color to assign at the specified coordinates.</param>
        protected static void SetImagePixel<TPixel>(Image<TPixel> image, int x, int y, Color color) where TPixel : unmanaged, IPixel<TPixel>
        {
            TPixel pixel = default;
            pixel.FromRgba32(new(color.R, color.G, color.B, color.A));
            image[x, y] = pixel;
        }

        /// <inheritdoc/>
        [DynamicDependency(nameof(GetImageData) + "``1", typeof(SharpImage))]
        public override IImageData GetData()
        {
            try{
                return GetImageData((dynamic)UnderlyingImage);
            }catch(RuntimeBinderException)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Obtains the raw pixel data of the image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
        /// <param name="image">The image to access.</param>
        /// <returns>An instance of <see cref="IImageData"/> for the image pixels.</returns>
        protected IImageData GetImageData<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new SharpImageData<TPixel>(this, image);
        }

        /// <inheritdoc/>
        public override Services.IImage? GetThumbnail()
        {
            var thumbnail = ImageMetadata.ExifProfile?.CreateThumbnail();
            if(thumbnail == null)
            {
                return null;
            }
            return new SharpImage(thumbnail, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override void Save(Stream output, string mediaType)
        {
            var manager = Configuration.Default.ImageFormatsManager;
            var format = manager.FindFormatByMimeType(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            var encoder = manager.FindEncoder(format) ?? throw new ArgumentException("The specified format's encoder cannot be found.", nameof(mediaType));
            Image.Save(output, encoder);
        }

        /// <inheritdoc/>
        public override Services.IImage Resize(int newWidth, int newHeight, bool preserveResolution, bool use32bppArgb, Color backgroundColor)
        {
            var resized = Image.Resize(newWidth, newHeight, use32bppArgb, backgroundColor, preserveResolution);
            return new SharpImage(resized, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override void ResizeInPlace(int newWidth, int newHeight, bool preserveResolution, Color backgroundColor)
        {
            Image.ResizeInPlace(newWidth, newHeight, backgroundColor, preserveResolution);
        }

        /// <inheritdoc/>
        public override Services.IImage RotateFlip(int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical, bool use32bppArgb)
        {
            var rotated = Image.RotateFlip(clockwise90DegreeTurns, flipHorizontal, flipVertical, use32bppArgb);
            return new SharpImage(rotated, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override void RotateFlipInPlace(int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical)
        {
            Image.RotateFlipInPlace(clockwise90DegreeTurns, flipHorizontal, flipVertical);
        }

        /// <inheritdoc/>
        public override Services.IImage Crop(System.Drawing.Rectangle cropRectangle, bool use32bppArgb)
        {
            var croppped = Image.Crop(cropRectangle, use32bppArgb);
            return new SharpImage(croppped, UnderlyingFormat);
        }

        /// <inheritdoc/>
        public override void CropInPlace(System.Drawing.Rectangle cropRectangle)
        {
            Image.CropInPlace(cropRectangle);
        }

        /// <inheritdoc/>
        public override ImageBase<IImage> Clone(bool use32bppArgb)
        {
            return new SharpImage(Image.Clone(use32bppArgb), UnderlyingFormat);
        }
    }

    /// <summary>
    /// Provides an <see cref="IImage{TUnderlying}"/> implementation
    /// backed by an instance of <see cref="Image{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
    public class SharpImage<TPixel> : SharpImage where TPixel : unmanaged, IPixel<TPixel>
    {
        Image<TPixel> Image => UnderlyingImage as Image<TPixel> ?? throw new InvalidOperationException();

        /// <inheritdoc/>
        public SharpImage(Image<TPixel> underlyingImage, IFileFormat<IImage> format) : base(underlyingImage, format)
        {

        }

        /// <inheritdoc/>
        public override Color GetPixel(int x, int y)
        {
            return GetImagePixel(Image, x, y);
        }

        /// <inheritdoc/>
        public override void SetPixel(int x, int y, Color color)
        {
            SetImagePixel(Image, x, y, color);
        }

        /// <inheritdoc/>
        public override IImageData GetData()
        {
            return GetImageData(Image);
        }
    }

    /// <summary>
    /// Provides an <see cref="IImageData"/> implementation
    /// backed by an instance of <see cref="Image{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type of the image.</typeparam>
    public class SharpImageData<TPixel> : MemoryImageData<IImage> where TPixel : unmanaged, IPixel<TPixel>
    {
        readonly Image<TPixel> data;

        /// <inheritdoc/>
        public override bool IsContiguous { get; }

        /// <summary>
        /// Creates a new instance of the data.
        /// </summary>
        /// <param name="image">The value of <see cref="ImageData{TUnderlying}.Image"/>.</param>
        /// <param name="data">The instance of <see cref="Image{TPixel}"/> to access the data of.</param>
        public SharpImageData(SharpImage image, Image<TPixel> data) : base(image, data.Width * Unsafe.SizeOf<TPixel>(), data.PixelType.BitsPerPixel)
        {
            this.data = data;
            // On contiguous buffers, the base implementation of GetPixel and GetRow is better since there is no MemoryManager allocation
            IsContiguous = data.DangerousTryGetSinglePixelMemory(out _);
        }
        
        /// <inheritdoc/>
        public override Span<byte> this[int x, int y] {
            get {
                if(IsContiguous)
                {
                    return base[x, y];
                }
                var memory = data.DangerousGetPixelRowMemory(y).Slice(x, 1);
                return MemoryMarshal.Cast<TPixel, byte>(memory.Span);
            }
        }

        /// <inheritdoc/>
        public override Span<byte> this[int y] {
            get {
                if(IsContiguous)
                {
                    return base[y];
                }
                var memory = data.DangerousGetPixelRowMemory(y);
                return MemoryMarshal.Cast<TPixel, byte>(memory.Span);
            }
        }

        /// <inheritdoc/>
        public override Memory<byte> GetPixel(int x, int y)
        {
            if(IsContiguous)
            {
                return base.GetPixel(x, y);
            }
            var memory = data.DangerousGetPixelRowMemory(y).Slice(x, 1);
            return MemoryUtils.Cast<TPixel, byte>(memory);
        }

        /// <inheritdoc/>
        public override Memory<byte> GetRow(int y)
        {
            if(IsContiguous)
            {
                return base.GetRow(y);
            }
            var memory = data.DangerousGetPixelRowMemory(y);
            return MemoryUtils.Cast<TPixel, byte>(memory);
        }

        /// <inheritdoc/>
        public override Span<byte> GetSpan()
        {
            if(!data.DangerousTryGetSinglePixelMemory(out var memory))
            {
                throw new NotSupportedException("The bitmap's memory is not contiguous.");
            }
            return MemoryMarshal.Cast<TPixel, byte>(memory.Span);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {

        }

        /// <inheritdoc/>
        protected override Stream Open()
        {
            return new DataStream(data);
        }

        class DataStream : BitmapRowDataStream
        {
            readonly Image<TPixel> data;

            public DataStream(Image<TPixel> data) : base(data.Height, data.Width * Unsafe.SizeOf<TPixel>())
            {
                this.data = data;
            }

            protected override void CopyData(int row, int offset, ArraySegment<byte> target)
            {
                var rowData = MemoryMarshal.Cast<TPixel, byte>(data.DangerousGetPixelRowMemory(row).Span).Slice(offset);
                var targetSpan = target.AsSpan();
                if(targetSpan.Length > rowData.Length)
                {
                    throw new ArgumentException("The buffer is too big for the row data.", nameof(target));
                }else{
                    rowData = rowData.Slice(0, targetSpan.Length);
                }
                rowData.CopyTo(targetSpan);
            }
        }
    }
}
