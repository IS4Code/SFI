using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace IS4.SFI.Formats
{
    public class DrawingImage : ImageBase<Image>
    {
        Bitmap Bitmap => UnderlyingImage as Bitmap ?? throw new NotSupportedException();

        public DrawingImage(Image underlyingImage, ImageFormat format) : base(underlyingImage, format)
        {

        }

        public override int Width => UnderlyingImage.Width;

        public override int Height => UnderlyingImage.Height;

        public override float HorizontalResolution => UnderlyingImage.HorizontalResolution;

        public override float VerticalResolution => UnderlyingImage.VerticalResolution;

        public override bool HasAlpha => Image.IsAlphaPixelFormat(UnderlyingImage.PixelFormat);

        public override int BitDepth => Image.GetPixelFormatSize(UnderlyingImage.PixelFormat);

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

        public override IImageTag? Tag {
            get => (UnderlyingImage.Tag as IImageTag) ?? base.Tag;
            set => UnderlyingImage.Tag = base.Tag = value;
        }

        public override Color GetPixel(int x, int y)
        {
            return Bitmap.GetPixel(x, y);
        }

        public override void SetPixel(int x, int y, Color color)
        {
            Bitmap.SetPixel(x, y, color);
        }

        public override IImageData GetData()
        {
            var bmp = Bitmap;
            var bits = BitDepth;
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bits == 0 ? PixelFormat.Format32bppArgb : bmp.PixelFormat);
            return new DrawingImageData(this, data);
        }
    }

    public class DrawingImageData : ImageData<Image>
    {
        BitmapData? _data;

        BitmapData data => _data ?? throw new ObjectDisposedException(nameof(DrawingImageData));

        public override int Scan0 { get; }
        public override int Stride { get; }
        public override int BitDepth { get; }
        public int Width { get; }
        public int Size { get; }

        public override long Length => Size;

        public DrawingImageData(DrawingImage image, BitmapData data) : base(image)
        {
            _data = data;

            Scan0 = -Math.Min(0, data.Stride * data.Height);
            Stride = data.Stride;
            Width = data.Width;
            BitDepth = System.Drawing.Image.GetPixelFormatSize(data.PixelFormat);
            Size = Math.Abs(data.Height * data.Stride);
        }

        public override Memory<byte> GetPixel(int x, int y)
        {
            var pixelSize = Math.DivRem(BitDepth, 8, out var rem);
            if(rem == 0)
            {
                throw new NotSupportedException("This operation is not supported when the bit depth is not divisible by 8.");
            }
            return GetRow(y).Slice(x * pixelSize, pixelSize);
        }

        public override Memory<byte> GetRow(int y)
        {
            return Memory.Slice(Scan0 + Stride * y, (Width * BitDepth + 7) / 8);
        }

        public unsafe override Span<byte> GetSpan()
        {
            return new Span<byte>((data.Scan0 - Scan0).ToPointer(), Size);
        }

        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            return new MemoryHandle((data.Scan0 - Scan0).ToPointer(), pinnable: this);
        }

        protected override Stream Open()
        {
            return new BitmapDataStream(data.Scan0, Stride, data.Height, Width, BitDepth);
        }

        public override void Unpin()
        {

        }

        protected override void Dispose(bool disposing)
        {
            if(Interlocked.Exchange(ref _data, null) is { } bmpData)
            {
                ((Bitmap)Image.UnderlyingImage).UnlockBits(bmpData);
            }
        }
    }
}
