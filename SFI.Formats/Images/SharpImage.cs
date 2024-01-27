using IS4.SFI.Services;
using IS4.SFI.Tools.IO;
using Microsoft.CSharp.RuntimeBinder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
                if(ImageMetadata.GetBmpMetadata() is { BitsPerPixel: var bmpBits and > 0 })
                {
                    return (int)bmpBits;
                }
                return PixelType.BitsPerPixel;
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
        public override IReadOnlyList<KeyValuePair<object, object>> Metadata => new TypedMetadataView(ImageMetadata);

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata => new RawMetadataView(ImageMetadata);

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
            pixel.FromRgba32(new(color.R, color.G, color.B, color.A));
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

        static readonly Configuration createConfiguration = new()
        {
            PreferContiguousImageBuffers = true
        };

        /// <inheritdoc/>
        public override Services.IImage Resize(int newWidth, int newHeight, bool preserveResolution, bool use32bppArgb, Color backgroundColor)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new(newWidth, newHeight),
                Mode = ResizeMode.Stretch
            };
            Action<IImageProcessingContext> operation = context => {
                if(backgroundColor.A != 0)
                {
                    context = context.BackgroundColor(new(new Bgra32(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A)));
                }
                context = context.Resize(resizeOptions);
            };
            Image clone;
            if(use32bppArgb)
            {
                clone = Image.CloneAs<Bgra32>(createConfiguration);
                clone.Mutate(operation);
            }else{
                clone = Image.Clone(createConfiguration, operation);
            }
            if(preserveResolution)
            {
                float xCoef = (float)clone.Width / Image.Width;
                float yCoef = (float)clone.Height / Image.Height;
                clone.Metadata.HorizontalResolution = Image.Metadata.HorizontalResolution * xCoef;
                clone.Metadata.VerticalResolution = Image.Metadata.VerticalResolution * yCoef;
            }
            return new SharpImage(clone, UnderlyingFormat);
        }

        abstract class MetadataView<TKey, TValue> : IReadOnlyList<KeyValuePair<TKey, TValue>>
        {
            readonly IReadOnlyList<IExifValue> exif;
            readonly IReadOnlyList<IptcValue> iptc;

            public MetadataView(ImageMetadata metadata)
            {
                exif = metadata.ExifProfile?.Values ?? Array.Empty<IExifValue>();
                var iptcEnumerable = metadata.IptcProfile?.Values ?? Array.Empty<IptcValue>();
                iptc = (iptcEnumerable as IReadOnlyList<IptcValue>) ?? iptcEnumerable.ToList();
            }

            protected abstract KeyValuePair<TKey, TValue> Transform(IExifValue item);

            protected abstract KeyValuePair<TKey, TValue> Transform(IptcValue item);

            public KeyValuePair<TKey, TValue> this[int index] {
                get {
                    if(index < exif.Count)
                    {
                        return Transform(exif[index]);
                    }
                    index -= exif.Count;
                    return Transform(iptc[index]);
                }
            }

            public int Count => exif.Count + iptc.Count;

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                foreach(var item in exif)
                {
                    yield return Transform(item);
                }
                foreach(var item in iptc)
                {
                    yield return Transform(item);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
        sealed class RawMetadataView : MetadataView<long, ReadOnlyMemory<byte>>
        {
            public RawMetadataView(ImageMetadata metadata) : base(metadata)
            {

            }

            protected override KeyValuePair<long, ReadOnlyMemory<byte>> Transform(IExifValue item)
            {
                var val = item.GetValue();

                ReadOnlyMemory<byte> result;
                if(val is string str)
                {
                    result = MemoryUtils.Cast<char, byte>(MemoryMarshal.AsMemory(str.AsMemory()));
                }else if(item.IsArray && val is Array arr)
                {
                    result = (item.DataType, arr) switch
                    {
                        (ExifDataType.Byte, byte[] array) => array.AsMemory(),
                        (ExifDataType.Short, ushort[] array) => MemoryUtils.Cast<ushort, byte>(array.AsMemory()),
                        (ExifDataType.Long, uint[] array) => MemoryUtils.Cast<uint, byte>(array.AsMemory()),
                        (ExifDataType.Rational, Rational[] array) => MemoryUtils.Cast<Rational, byte>(array.AsMemory()),
                        (ExifDataType.SignedByte, sbyte[] array) => MemoryUtils.Cast<sbyte, byte>(array.AsMemory()),
                        (ExifDataType.SignedShort, short[] array) => MemoryUtils.Cast<short, byte>(array.AsMemory()),
                        (ExifDataType.SignedLong, int[] array) => MemoryUtils.Cast<int, byte>(array.AsMemory()),
                        (ExifDataType.SignedRational, SignedRational[] array) => MemoryUtils.Cast<SignedRational, byte>(array.AsMemory()),
                        (ExifDataType.SingleFloat, float[] array) => MemoryUtils.Cast<float, byte>(array.AsMemory()),
                        (ExifDataType.DoubleFloat, double[] array) => MemoryUtils.Cast<double, byte>(array.AsMemory()),
                        (ExifDataType.Long8, ulong[] array) => MemoryUtils.Cast<ulong, byte>(array.AsMemory()),
                        (ExifDataType.SignedLong8, long[] array) => MemoryUtils.Cast<long, byte>(array.AsMemory()),
                        _ => MemoryUtils.GetObjectMemory(arr)
                    };
                }else{
                    result = (item.DataType, val) switch
                    {
                        (_, EncodedString value) => MemoryUtils.Cast<char, byte>(MemoryMarshal.AsMemory(value.Text.AsMemory())),
                        (ExifDataType.Byte, Number value) => MemoryUtils.GetValueMemory((byte)(uint)value),
                        (ExifDataType.Byte, byte value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.Short, Number value) => MemoryUtils.GetValueMemory((ushort)(uint)value),
                        (ExifDataType.Short, ushort value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.Long, Number value) => MemoryUtils.GetValueMemory((uint)value),
                        (ExifDataType.Long, uint value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.Rational, Rational value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SignedByte, Number value) => MemoryUtils.GetValueMemory((sbyte)(int)value),
                        (ExifDataType.SignedByte, sbyte value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SignedShort, Number value) => MemoryUtils.GetValueMemory((short)(int)value),
                        (ExifDataType.SignedShort, short value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SignedLong, Number value) => MemoryUtils.GetValueMemory((int)value),
                        (ExifDataType.SignedLong, int value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SignedRational, SignedRational value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SingleFloat, float value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.DoubleFloat, double value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.Long8, ulong value) => MemoryUtils.GetValueMemory(value),
                        (ExifDataType.SignedLong8, long value) => MemoryUtils.GetValueMemory(value),
                        _ => MemoryUtils.GetObjectMemory(val)
                    };
                }

                return new((long)item.Tag, result);
            }

            protected override KeyValuePair<long, ReadOnlyMemory<byte>> Transform(IptcValue item)
            {
                return new((long)item.Tag, item.ToByteArray().AsMemory());
            }
        }

        sealed class TypedMetadataView : MetadataView<object, object>
        {
            public TypedMetadataView(ImageMetadata metadata) : base(metadata)
            {

            }

            protected override KeyValuePair<object, object> Transform(IExifValue item)
            {
                return new(item.Tag, item.GetValue());
            }

            protected override KeyValuePair<object, object> Transform(IptcValue item)
            {
                return new(item.Tag, item.Value);
            }
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
                rowData.CopyTo(target.AsSpan());
            }
        }
    }
}
