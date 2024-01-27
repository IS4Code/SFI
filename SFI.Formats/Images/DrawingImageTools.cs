using IS4.SFI.Formats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace IS4.SFI.Tools.Images
{
    /// <summary>
    /// Stores utility methods for manipulating images from <see cref="System.Drawing"/>.
    /// </summary>
    public static class DrawingImageTools
    {
        static readonly ImageAttributes drawAttributes = new();

        static DrawingImageTools()
        {
            drawAttributes.SetWrapMode(WrapMode.TileFlipXY);
        }

        /// <summary>
        /// Resizes an image to particular dimensions, while preserving
        /// the original dimensions in metadata.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="pixelFormat">The pixel format of the new image.</param>
        /// <param name="backgroundColor">The back</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap Resize(this Image image, int width, int height, PixelFormat pixelFormat, Color backgroundColor, bool preserveResolution)
        {
            var resized = new Bitmap(width, height, pixelFormat);

            using(var gr = Graphics.FromImage(resized))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if(backgroundColor.A == 0)
                {
                    gr.CompositingMode = CompositingMode.SourceCopy;
                }
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.Clear(backgroundColor);
                gr.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, drawAttributes);
            }

            if(preserveResolution)
            {
                float xCoef = (float)resized.Width / image.Width;
                float yCoef = (float)resized.Height / image.Height;
                resized.SetResolution(image.HorizontalResolution * xCoef, image.VerticalResolution * yCoef);
            }

            return resized;
        }

        /// <summary>
        /// Retrieves the image metadata as a list of implementation-defined key-value pairs.
        /// </summary>
        /// <param name="image">The image to retrieve the metadata from.</param>
        /// <returns>The list of metadata items.</returns>
        public static IReadOnlyList<KeyValuePair<object, object>> GetMetadata(this Image image)
        {
            return new TypedMetadataView(image.PropertyItems);
        }

        /// <summary>
        /// Retrieves the image metadata as a list of raw-valued key-value pairs.
        /// </summary>
        /// <param name="image">The image to retrieve the metadata from.</param>
        /// <returns>The list of metadata items.</returns>
        public static IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> GetRawMetadata(this Image image)
        {
            return new RawMetadataView(image.PropertyItems);
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

                static object GetMemory<TTo>(Memory<byte> raw) where TTo : unmanaged
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
}
