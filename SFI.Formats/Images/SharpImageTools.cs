using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace IS4.SFI.Tools.Images
{
    /// <summary>
    /// Stores utility methods for manipulating images from <see cref="SixLabors.ImageSharp"/>.
    /// </summary>
    public static class SharpImageTools
    {
        static readonly Configuration createConfiguration = new()
        {
            PreferContiguousImageBuffers = true
        };

        /// <summary>
        /// Resizes an image to particular dimensions, while preserving
        /// the original dimensions in metadata.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format, or the original one.</param>
        /// <param name="backgroundColor">The back</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <returns>The resized image.</returns>
        public static Image Resize(this Image image, int width, int height, bool use32bppArgb, System.Drawing.Color backgroundColor, bool preserveResolution)
        {
            var operation = ResizeOperation(width, height, backgroundColor);

            Image resized;
            if(use32bppArgb)
            {
                resized = image.CloneAs<Bgra32>(createConfiguration);
                resized.Mutate(operation);
            }else{
                resized = image.Clone(createConfiguration, operation);
            }

            if(preserveResolution)
            {
                double xCoef = (double)resized.Width / image.Width;
                double yCoef = (double)resized.Height / image.Height;
                resized.Metadata.HorizontalResolution = image.Metadata.HorizontalResolution * xCoef;
                resized.Metadata.VerticalResolution = image.Metadata.VerticalResolution * yCoef;
            }

            return resized;
        }

        /// <summary>
        /// Resizes an image to particular dimensions, while preserving
        /// the original dimensions in metadata.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format, or the original one.</param>
        /// <param name="backgroundColor">The back</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <returns>The resized image.</returns>
        public static void ResizeInPlace(this Image image, int width, int height, System.Drawing.Color backgroundColor, bool preserveResolution)
        {
            var operation = ResizeOperation(width, height, backgroundColor);
            if(preserveResolution)
            {
                double xRes = image.Metadata.HorizontalResolution * width / image.Width;
                double yRes = image.Metadata.VerticalResolution * height / image.Height;
                image.Mutate(operation);
                image.Metadata.HorizontalResolution = xRes;
                image.Metadata.VerticalResolution = yRes;
            }else{
                image.Mutate(operation);
            }
        }

        static Action<IImageProcessingContext> ResizeOperation(int width, int height, System.Drawing.Color backgroundColor)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new(width, height),
                Mode = ResizeMode.Stretch
            };

            return context => {
                if(backgroundColor.A != 0)
                {
                    context = context.BackgroundColor(new(new Bgra32(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A)));
                }
                context = context.Resize(resizeOptions);
            };
        }

        /// <summary>
        /// Creates a new image that copies all data from this instance.
        /// </summary>
        /// <param name="image">The image to clone.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>, or the original one.</param>
        /// <returns>The cloned image.</returns>
        public static Image Clone(this Image image, bool use32bppArgb)
        {
            if(use32bppArgb)
            {
                return image.CloneAs<Bgra32>(createConfiguration);
            }else{
                return image.Clone(_ => { });
            }
        }

        /// <summary>
        /// Rotates or flips an image. The image is first rotated, then flipped.
        /// </summary>
        /// <param name="image">The image to rotate or flip.</param>
        /// <param name="clockwise90DegreeTurns">The multiple of 90 ° resulting in a clockwise rotation.</param>
        /// <param name="flipHorizontal">Whether to flip the image horizontally.</param>
        /// <param name="flipVertical">Whether to flip the image vertically.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format, or the original one.</param>
        /// <returns>The rotated or flipped image.</returns>
        public static Image RotateFlip(this Image image, int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical, bool use32bppArgb)
        {
            var operation = RotateFlipOperation(clockwise90DegreeTurns, flipHorizontal, flipVertical);

            if(operation == null)
            {
                return image.Clone(use32bppArgb);
            }

            Image rotated;
            if(use32bppArgb)
            {
                rotated = image.CloneAs<Bgra32>(createConfiguration);
                rotated.Mutate(operation);
            }else{
                rotated = image.Clone(createConfiguration, operation);
            }

            return rotated;
        }

        /// <summary>
        /// Rotates or flips an image. The image is first rotated, then flipped.
        /// </summary>
        /// <param name="image">The image to rotate or flip.</param>
        /// <param name="clockwise90DegreeTurns">The multiple of 90 ° resulting in a clockwise rotation.</param>
        /// <param name="flipHorizontal">Whether to flip the image horizontally.</param>
        /// <param name="flipVertical">Whether to flip the image vertically.</param>
        public static void RotateFlipInPlace(this Image image, int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical)
        {
            var operation = RotateFlipOperation(clockwise90DegreeTurns, flipHorizontal, flipVertical);
            if(operation == null)
            {
                return;
            }
            image.Mutate(operation);
        }
        
        static Action<IImageProcessingContext>? RotateFlipOperation(int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical)
        {
            if(flipHorizontal && flipVertical)
            {
                clockwise90DegreeTurns += 2;
            }
            clockwise90DegreeTurns = (clockwise90DegreeTurns % 4 + 4) % 4;
            if(clockwise90DegreeTurns == 0 && !flipHorizontal && !flipVertical)
            {
                return null;
            }

#pragma warning disable CS8509
            var rotateMode = clockwise90DegreeTurns switch
            {
                0 => RotateMode.None,
                1 => RotateMode.Rotate90,
                2 => RotateMode.Rotate180,
                3 => RotateMode.Rotate270
            };
#pragma warning restore CS8509

            var flipMode = (flipHorizontal, flipVertical) switch
            {
                (false, false) or (true, true) => FlipMode.None,
                (false, true) => FlipMode.Vertical,
                (true, false) => FlipMode.Horizontal
            };

            return context => {
                context = context.RotateFlip(rotateMode, flipMode);
            };
        }

        /// <summary>
        /// Retrieves the image metadata as a list of implementation-defined key-value pairs.
        /// </summary>
        /// <param name="image">The image to retrieve the metadata from.</param>
        /// <returns>The list of metadata items.</returns>
        public static IReadOnlyList<KeyValuePair<object, object>> GetMetadata(this IImage image)
        {
            return new TypedMetadataView(image.Metadata);
        }

        /// <summary>
        /// Retrieves the image metadata as a list of raw-valued key-value pairs.
        /// </summary>
        /// <param name="image">The image to retrieve the metadata from.</param>
        /// <returns>The list of metadata items.</returns>
        public static IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> GetRawMetadata(this IImage image)
        {
            return new RawMetadataView(image.Metadata);
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
}
