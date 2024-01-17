using IS4.SFI.MediaAnalysis.Images;
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

namespace IS4.SFI.Formats
{
    public class DrawingImage : ImageBase<Image>
    {
        Bitmap Bitmap => UnderlyingImage as Bitmap ?? throw new NotSupportedException();

        public DrawingImage(Image underlyingImage, IFileFormat<Image> format) : base(underlyingImage, format)
        {

        }

        public override int Width => UnderlyingImage.Width;

        public override int Height => UnderlyingImage.Height;

        public override double HorizontalResolution => UnderlyingImage.HorizontalResolution;

        public override double VerticalResolution => UnderlyingImage.VerticalResolution;

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

        public override void Save(Stream output, string mediaType)
        {
            var encoder = ImageFormat.GetOutputEncoder(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            UnderlyingImage.Save(output, encoder, new EncoderParameters());
        }

        public override IImage Resize(int newWith, int newHeight, bool use32bppArgb, Color backgroundColor)
        {
            return new DrawingImage(ImageTools.ResizeImage(UnderlyingImage, newWith, newHeight, use32bppArgb ? PixelFormat.Format32bppArgb : UnderlyingImage.PixelFormat, backgroundColor), UnderlyingFormat);
        }
    }

    public class DrawingImageData : MemoryImageData<Image>
    {
        BitmapData? _data;

        BitmapData data => _data ?? throw new ObjectDisposedException(nameof(DrawingImageData));

        public DrawingImageData(DrawingImage image, BitmapData data) : base(image, data.Stride, System.Drawing.Image.GetPixelFormatSize(data.PixelFormat))
        {
            _data = data;
        }

        public unsafe override Span<byte> GetSpan()
        {
            return new Span<byte>((data.Scan0 - Scan0).ToPointer(), Size);
        }

        protected override Stream Open()
        {
            return new BitmapDataStream(data.Scan0, Stride, data.Height, Width, BitDepth);
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
