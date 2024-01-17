using IS4.SFI.Services;
using IS4.SFI.Tools.IO;
using Microsoft.CSharp.RuntimeBinder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
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
    public class SharpImage : ImageBase<IImage>
    {
        Image Image => UnderlyingImage as Image ?? throw new NotSupportedException();

        ImageMetadata Metadata => UnderlyingImage.Metadata;

        PixelTypeInfo PixelType => UnderlyingImage.PixelType;

        public SharpImage(IImage underlyingImage, IFileFormat<IImage> format) : base(underlyingImage, format)
        {

        }

        public override int Width => UnderlyingImage.Width;

        public override int Height => UnderlyingImage.Height;

        public override double HorizontalResolution => ResolutionUnit * Metadata.HorizontalResolution;

        public override double VerticalResolution => ResolutionUnit * Metadata.VerticalResolution;

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

        public override bool HasAlpha => PixelType.AlphaRepresentation is not (0 or null);

        public override int BitDepth => PixelType.BitsPerPixel;

        public override IReadOnlyList<Color> Palette => Array.Empty<Color>(); // TODO

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

        private static Color GetImagePixel<TPixel>(Image<TPixel> image, int x, int y) where TPixel : unmanaged, IPixel<TPixel>
        {
            Rgba32 pixel = default;
            image[x, y].ToRgba32(ref pixel);
            return Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
        }

        private static void SetImagePixel<TPixel>(Image<TPixel> image, int x, int y, Color color) where TPixel : unmanaged, IPixel<TPixel>
        {
            TPixel pixel = default;
            pixel.FromArgb32(new(color.R, color.G, color.B, color.A));
            image[x, y] = pixel;
        }

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

        private IImageData GetImageData<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new SharpImageData<TPixel>(this, image);
        }

        public override void Save(Stream output, string mediaType)
        {
            var format = Configuration.Default.ImageFormatsManager.FindFormatByMimeType(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            Image.Save(output, format);
        }

        static readonly Configuration createConfiguration = new()
        {
            PreferContiguousImageBuffers = true
        };

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

    public class SharpImageData<TPixel> : MemoryImageData<IImage> where TPixel : unmanaged, IPixel<TPixel>
    {
        readonly Image<TPixel> data;

        public override bool IsContiguous { get; }

        public SharpImageData(SharpImage image, Image<TPixel> data) : base(image, data.Width * Unsafe.SizeOf<TPixel>(), data.PixelType.BitsPerPixel)
        {
            this.data = data;
            // On contiguous buffers, the base implementation of GetPixel and GetRow is better since there is no MemoryManager allocation
            IsContiguous = data.DangerousTryGetSinglePixelMemory(out _);
        }

        public override Memory<byte> GetPixel(int x, int y)
        {
            if(IsContiguous)
            {
                return base.GetPixel(x, y);
            }
            var memory = data.DangerousGetPixelRowMemory(y).Slice(x, 1);
            return Utils.Cast<TPixel, byte>(memory);
        }

        public override Memory<byte> GetRow(int y)
        {
            if(IsContiguous)
            {
                return base.GetRow(y);
            }
            var memory = data.DangerousGetPixelRowMemory(y);
            return Utils.Cast<TPixel, byte>(memory);
        }

        public override Span<byte> GetSpan()
        {
            if(!data.DangerousTryGetSinglePixelMemory(out var memory))
            {
                throw new NotSupportedException("The bitmap's memory is not contiguous.");
            }
            return MemoryMarshal.Cast<TPixel, byte>(memory.Span);
        }

        protected override void Dispose(bool disposing)
        {

        }

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
