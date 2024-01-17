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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Color = System.Drawing.Color;
using IImage = SixLabors.ImageSharp.IImage;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Provides an <see cref="IImage{TUnderlying}"/> implementation
    /// backed by an instance of <see cref="IImage"/>.
    /// </summary>
    public class SharpImage : ImageBase<IImage>
    {
        Image Image => UnderlyingImage as Image ?? throw new NotSupportedException();

        ImageMetadata Metadata => UnderlyingImage.Metadata;

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
        public override double HorizontalResolution => ResolutionUnit * Metadata.HorizontalResolution;

        /// <inheritdoc/>
        public override double VerticalResolution => ResolutionUnit * Metadata.VerticalResolution;

        /// <summary>
        /// The coefficient to use when converting resolution to pixel per inch.
        /// </summary>
        private double ResolutionUnit{
            get{
                switch(Metadata.ResolutionUnits)
                {
                    case PixelResolutionUnit.AspectRatio:
                        return 0;
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
        public override bool HasAlpha => PixelType.AlphaRepresentation is not (0 or null);

        /// <inheritdoc/>
        public override int BitDepth => PixelType.BitsPerPixel;

        /// <inheritdoc/>
        public override IReadOnlyList<Color> Palette => Array.Empty<Color>();

        /// <inheritdoc/>
        public override int? PaletteSize{
            get{
                if(Metadata.GetPngMetadata() is { } png)
                {
                    return png.ColorType == PngColorType.Palette ? null : 0;
                }
                if(Metadata.GetGifMetadata() is { } gif)
                {
                    return gif.GlobalColorTableLength;
                }
                if(Metadata.GetJpegMetadata() != null)
                {
                    return 0;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        [DynamicDependency(nameof(GetImagePixel), typeof(SharpImage))]
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
        [DynamicDependency(nameof(SetImagePixel), typeof(SharpImage))]
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
            pixel.FromArgb32(new(color.R, color.G, color.B, color.A));
            image[x, y] = pixel;
        }

        /// <inheritdoc/>
        [DynamicDependency(nameof(GetImageData), typeof(SharpImage))]
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
        public override void Save(Stream output, string mediaType)
        {
            var format = Configuration.Default.ImageFormatsManager.FindFormatByMimeType(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            Image.Save(output, format);
        }

        static readonly Configuration createConfiguration = new()
        {
            PreferContiguousImageBuffers = true
        };

        /// <inheritdoc/>
        public override Services.IImage Resize(int newWith, int newHeight, bool use32bppArgb, Color backgroundColor)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new(newWith, newHeight),
                Mode = ResizeMode.Stretch
            };
            Action<IImageProcessingContext> operation = context => {
                if(backgroundColor.A != 0)
                {
                    context = context.BackgroundColor(new(new Argb32(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A)));
                }
                context = context.Resize(resizeOptions);
            };
            Image clone;
            if(use32bppArgb)
            {
                clone = Image.CloneAs<Argb32>(createConfiguration);
                clone.Mutate(operation);
            }else{
                clone = Image.Clone(createConfiguration, operation);
            }
            return new SharpImage(clone, UnderlyingFormat);
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
        public override Memory<byte> GetPixel(int x, int y)
        {
            if(IsContiguous)
            {
                return base.GetPixel(x, y);
            }
            var memory = data.DangerousGetPixelRowMemory(y).Slice(x, 1);
            return Utils.Cast<TPixel, byte>(memory);
        }

        /// <inheritdoc/>
        public override Memory<byte> GetRow(int y)
        {
            if(IsContiguous)
            {
                return base.GetRow(y);
            }
            var memory = data.DangerousGetPixelRowMemory(y);
            return Utils.Cast<TPixel, byte>(memory);
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
                rowData.CopyTo(target.AsSpan());
            }
        }
    }

    static class Utils
    {
        public static Memory<TTo> Cast<TFrom, TTo>(Memory<TFrom> from) where TFrom : unmanaged where TTo : unmanaged
        {
            return new CastMemoryManager<TFrom, TTo>(from).Memory;
        }

        sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged
        {
            readonly Memory<TFrom> memory;

            public CastMemoryManager(Memory<TFrom> memory)
            {
                this.memory = memory;
            }

            public override Span<TTo> GetSpan()
            {
                return MemoryMarshal.Cast<TFrom, TTo>(memory.Span);
            }

            protected override void Dispose(bool disposing)
            {

            }

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                throw new NotImplementedException();
            }

            public override void Unpin()
            {

            }
        }
    }
}
