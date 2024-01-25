using IS4.SFI.MediaAnalysis.Images;
using IS4.SFI.Services;
using IS4.SFI.Tags;
using IS4.SFI.Tools.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace IS4.SFI.Formats
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
        public override IReadOnlyList<KeyValuePair<object, object>> Metadata => new TypedMetadataView(UnderlyingImage.PropertyItems);

        /// <inheritdoc/>
        public override IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata => new RawMetadataView(UnderlyingImage.PropertyItems);

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
            var encoder = ImageFormat.GetOutputEncoder(mediaType) ?? throw new ArgumentException("The specified format cannot be found.", nameof(mediaType));
            UnderlyingImage.Save(output, encoder, null);
        }

        /// <inheritdoc/>
        public override IImage Resize(int newWith, int newHeight, bool use32bppArgb, Color backgroundColor)
        {
            return new DrawingImage(ImageTools.ResizeImage(UnderlyingImage, newWith, newHeight, use32bppArgb ? PixelFormat.Format32bppArgb : UnderlyingImage.PixelFormat, backgroundColor), UnderlyingFormat);
        }

        abstract class MetadataView<TKey, TValue> : IReadOnlyList<KeyValuePair<TKey, TValue>>
        {
            readonly PropertyItem[] items;

            public MetadataView(PropertyItem[] items)
            {
                this.items = items;
            }

            protected abstract KeyValuePair<TKey, TValue> Transform(PropertyItem item);

            public KeyValuePair<TKey, TValue> this[int index] => Transform(items[index]);

            public int Count => items.Length;

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                foreach(var item in items)
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
            public RawMetadataView(PropertyItem[] items) : base(items)
            {

            }

            protected override KeyValuePair<long, ReadOnlyMemory<byte>> Transform(PropertyItem item)
            {
                return new(item.Id, item.Value.AsMemory());
            }
        }

        sealed class TypedMetadataView : MetadataView<object, object>
        {
            public TypedMetadataView(PropertyItem[] items) : base(items)
            {

            }

            protected override KeyValuePair<object, object> Transform(PropertyItem item)
            {
                var valueRaw = item.Value.AsMemory().Slice(0, item.Len);
                object value = (DrawingPropertyType)item.Type switch
                {
                    DrawingPropertyType.ASCII => Encoding.UTF8.GetString(item.Value, 0, item.Len - 1),
                    DrawingPropertyType.UInt16 => GetMemory<ushort>(valueRaw),
                    DrawingPropertyType.UInt32 => GetMemory<uint>(valueRaw),
                    DrawingPropertyType.RationalUInt32 => GetMemory<SixLabors.ImageSharp.Rational>(valueRaw),
                    DrawingPropertyType.SByte => GetMemory<sbyte>(valueRaw),
                    DrawingPropertyType.Int16 => GetMemory<short>(valueRaw),
                    DrawingPropertyType.Int32 => GetMemory<int>(valueRaw),
                    DrawingPropertyType.RationalInt32 => GetMemory<SixLabors.ImageSharp.SignedRational>(valueRaw),
                    DrawingPropertyType.Float => GetMemory<float>(valueRaw),
                    DrawingPropertyType.Double => GetMemory<double>(valueRaw),
                    _ => valueRaw,
                };
                return new((DrawingPropertyId)item.Id, value);

                object GetMemory<TTo>(Memory<byte> raw) where TTo : unmanaged
                {
                    ReadOnlyMemory<TTo> cast = MemoryUtils.Cast<byte, TTo>(raw);
                    if(cast.Length == 1)
                    {
                        return cast.Span[0];
                    }
                    return cast;
                }
            }

            /// <summary>
            /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.propertyitem.type"/>
            /// </summary>
            enum DrawingPropertyType : short
            {
                Byte = 1,
                ASCII = 2,
                UInt16 = 3,
                UInt32 = 4,
                RationalUInt32 = 5,
                SByte = 6,
                Undefined = 7,
                Int16 = 8,
                Int32 = 9,
                RationalInt32 = 10,
                Float = 11,
                Double = 12
            }
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
