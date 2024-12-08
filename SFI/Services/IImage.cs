using IS4.SFI.Formats;
using IS4.SFI.Tags;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace IS4.SFI.Services
{
    /// <summary>
    /// A basic image or bitmap interface, supporting pixel access.
    /// </summary>
    public interface IImage : IIdentityKey, IDisposable, ICloneable
    {
        /// <summary>
        /// The width of the image, in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the image, in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The horizontal resolution, in pixels per inch.
        /// </summary>
        double HorizontalResolution { get; }

        /// <summary>
        /// The vertical resolution, in pixels per inch.
        /// </summary>
        double VerticalResolution { get; }

        /// <summary>
        /// Whether the image is using the 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>.
        /// </summary>
        bool Is32bppArgb { get; }

        /// <summary>
        /// Whether image has the alpha channel.
        /// </summary>
        bool HasAlpha { get; }

        /// <summary>
        /// The color palette of the image.
        /// </summary>
        IReadOnlyList<Color> Palette { get; }

        /// <summary>
        /// The size of the color palette of the image.
        /// </summary>
        int? PaletteSize { get; }

        /// <summary>
        /// The format of the image.
        /// </summary>
        IFileFormat<IImage> Format { get; }

        /// <summary>
        /// The number of bits per pixel.
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// Whether the image uses lossless compression.
        /// Might be <see langword="null"/> when the information is not known,
        /// or when the format does not support different types of compression.
        /// </summary>
        bool? IsLossless { get; }

        /// <summary>
        /// Stores a collection of implementation-defined pairs of metadata entries attached to the image,
        /// using the most natural types of the keys and values available.
        /// </summary>
        /// <remarks>
        /// This collection should have the same size and index layout as <see cref="RawMetadata"/>.
        /// </remarks>
        IReadOnlyList<KeyValuePair<object, object>> Metadata { get; }

        /// <summary>
        /// Stores a collection of implementation-defined pairs of metadata entries attached to the image,
        /// using the raw types of keys and values.
        /// </summary>
        /// <remarks>
        /// This collection should have the same size and index layout as <see cref="Metadata"/>.
        /// </remarks>
        IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata { get; }

        /// <summary>
        /// An arbitrary tag assigned to the image.
        /// </summary>
        IImageTag? Tag { get; set; }

        /// <summary>
        /// Accesses the color of the pixel at the given point.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns>The color value of the pixel at the specified coordinates.</returns>
        Color this[int x, int y] { get; set; }

        /// <summary>
        /// Accesses the color of the pixel at the given point.
        /// </summary>
        /// <param name="point">The coordinates of the pixel.</param>
        /// <returns>The color value of the pixel at <paramref name="point"/>.</returns>
        Color this[Point point] { get; set; }

        /// <summary>
        /// Retrieves the color value of a particular pixel.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at the specified coordinates.</returns>
        Color GetPixel(int x, int y);

        /// <summary>
        /// Assigns the color value of a particular pixel.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <param name="color">The color to assign at the specified coordinates.</param>
        void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Obtains the raw pixel data of the image.
        /// </summary>
        /// <returns>An instance of <see cref="IImageData"/> for the image pixels.</returns>
        IImageData GetData();

        /// <summary>
        /// Retrieves or produces a thumbnail of the image, possibly using the embedded one.
        /// </summary>
        /// <returns>An instance of <see cref="IImage"/> for the thumbnail, or null.</returns>
        IImage? GetThumbnail();

        /// <summary>
        /// Saves the image in a particular format.
        /// </summary>
        /// <param name="output">The output stream to write the image to.</param>
        /// <param name="mediaType">The media type identifying the format.</param>
        void Save(Stream output, string mediaType);

        /// <summary>
        /// Creates a new image that copies all data from this instance.
        /// </summary>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>), or the original one.</param>
        /// <returns>The cloned image.</returns>
        IImage Clone(bool use32bppArgb);

        /// <summary>
        /// Resizes the image to new dimensions and returns it as a new image.
        /// </summary>
        /// <param name="newWidth">The new width of the image.</param>
        /// <param name="newHeight">The new height of the image.</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>), or the original one.</param>
        /// <param name="backgroundColor">The background of the resized image.</param>
        /// <returns>A new image with the specified dimensions.</returns>
        IImage Resize(int newWidth, int newHeight, bool preserveResolution = true, bool use32bppArgb = false, Color backgroundColor = default);
        
        /// <summary>
        /// Resizes the image to new dimensions by mutating the original one.
        /// </summary>
        /// <param name="newWidth">The new width of the image.</param>
        /// <param name="newHeight">The new height of the image.</param>
        /// <param name="preserveResolution">Whether to preserve the original resolution.</param>
        /// <param name="backgroundColor">The background of the resized image.</param>
        void ResizeInPlace(int newWidth, int newHeight, bool preserveResolution = true, Color backgroundColor = default);

        /// <summary>
        /// Rotates or flips the image and returns it as a new image. The image is first rotated, then flipped.
        /// </summary>
        /// <param name="clockwise90DegreeTurns">The multiple of 90 ° resulting in a clockwise rotation.</param>
        /// <param name="flipHorizontal">Whether to flip the image horizontally.</param>
        /// <param name="flipVertical">Whether to flip the image vertically.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>), or the original one.</param>
        /// <returns>A new image rotated or flipped in the specified way.</returns>
        IImage RotateFlip(int clockwise90DegreeTurns = 0, bool flipHorizontal = false, bool flipVertical = false, bool use32bppArgb = false);

        /// <summary>
        /// Rotates or flips the image by mutating the original one. The image is first rotated, then flipped.
        /// </summary>
        /// <param name="clockwise90DegreeTurns">The multiple of 90 ° resulting in a clockwise rotation.</param>
        /// <param name="flipHorizontal">Whether to flip the image horizontally.</param>
        /// <param name="flipVertical">Whether to flip the image vertically.</param>
        void RotateFlipInPlace(int clockwise90DegreeTurns = 0, bool flipHorizontal = false, bool flipVertical = false);

        /// <summary>
        /// Crops the image and returns it as a new image.
        /// </summary>
        /// <param name="cropRectangle">The rectangle within the image that should be cropped to.</param>
        /// <param name="use32bppArgb">Whether to create the image using 32-bit ARGB pixel format (compatible with <see cref="Color.FromArgb(int)"/>), or the original one.</param>
        /// <returns>A new image cropped to the area.</returns>
        IImage Crop(Rectangle cropRectangle, bool use32bppArgb = false);

        /// <summary>
        /// Crops the image by mutating the original one.
        /// </summary>
        /// <param name="cropRectangle">The rectangle within the image that should be cropped to.</param>
        void CropInPlace(Rectangle cropRectangle);
    }

    /// <summary>
    /// An image or bitmap interface with a particular underlying instance.
    /// </summary>
    /// <typeparam name="TUnderlying">
    /// The type of the underlying image instance.
    /// </typeparam>
    public interface IImage<TUnderlying> : IImage
    {
        /// <summary>
        /// The underlying image instance.
        /// </summary>
        TUnderlying UnderlyingImage { get; }

        /// <summary>
        /// The format of the image.
        /// </summary>
        new IFileFormat<IImage<TUnderlying>> Format { get; }
    }

    /// <summary>
    /// Provides access to raw image data as bytes.
    /// </summary>
    public interface IImageData : IMemoryOwner<byte>, IStreamFactory
    {
        /// <summary>
        /// Whether the image data is backed by a contiguous range of memory, in which
        /// case the <see cref="IMemoryOwner{T}.Memory"/> member is usable and describable
        /// by <see cref="Scan0"/> and <see cref="Stride"/>.
        /// </summary>
        bool IsContiguous { get; }

        /// <summary>
        /// The offset of the top-left pixel in the data.
        /// </summary>
        int Scan0 { get; }

        /// <summary>
        /// The number of rows in the data.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The offset in bytes of each row from the previous.
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// The size of each row in bytes.
        /// </summary>
        int RowSize { get; }

        /// <summary>
        /// The number of bits per pixel.
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// Stores the complete sequence of bytes comprising the data.
        /// This property is usable even if <see cref="IsContiguous"/>
        /// is <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// The data starts at the top-most row and follows each row,
        /// being exactly <see cref="RowSize"/> bytes long with no padding.
        /// There is no guarantee that each <see cref="Memory{T}"/> instance
        /// in the sequence must align with each row.
        /// </remarks>
        ReadOnlySequence<byte> Sequence { get; }

        /// <summary>
        /// Retrieves the memory range of the pixels at the given row.
        /// </summary>
        /// <param name="y">The coordinates of the pixel.</param>
        /// <returns>The range of bytes corresponding to the row's pixels.</returns>
        Span<byte> this[int y] { get; }

        /// <summary>
        /// Retrieves the memory range of the pixel at the given point.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns>The range of bytes corresponding to the pixel.</returns>
        Span<byte> this[int x, int y] { get; }

        /// <summary>
        /// Retrieves the memory range of the pixel at the given point.
        /// </summary>
        /// <param name="point">The coordinates of the pixel.</param>
        /// <returns>The range of bytes corresponding to the pixel.</returns>
        Span<byte> this[Point point] { get; }

        /// <summary>
        /// Retrieves the memory range of bytes corresponding to a row of pixels.
        /// </summary>
        /// <param name="y">The Y coordinate of the pixels.</param>
        /// <returns>The <see cref="Memory{T}"/> instance corresponding to the row.</returns>
        Memory<byte> GetRow(int y);

        /// <summary>
        /// Retrieves the memory range of bytes corresponding to a particular pixel.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <returns>The <see cref="Memory{T}"/> instance corresponding to the pixel.</returns>
        Memory<byte> GetPixel(int x, int y);
    }

    /// <summary>
    /// Provides an abstract implementation of <see cref="IImage{TUnderlying}"/>.
    /// </summary>
    /// <typeparam name="TUnderlying">
    /// The type of the underlying image instance.
    /// </typeparam>
    public abstract class ImageBase<TUnderlying> : IImage<TUnderlying>, IFileFormat<IImage> where TUnderlying : class
    {
        TUnderlying? underlyingImage;

        readonly IFileFormat<TUnderlying> format;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="underlyingImage">The value of <see cref="UnderlyingImage"/>.</param>
        /// <param name="format">The value of <see cref="UnderlyingFormat"/>.</param>
        protected ImageBase(TUnderlying underlyingImage, IFileFormat<TUnderlying> format)
        {
            this.underlyingImage = underlyingImage;
            this.format = format;
        }

        /// <inheritdoc/>
        public virtual IImageTag? Tag { get; set; }

        /// <inheritdoc/>
        public TUnderlying UnderlyingImage => underlyingImage ?? throw new ObjectDisposedException(ToString());

        /// <summary>
        /// The <see cref="IFileFormat{T}"/> instance describing the underlying image's format.
        /// </summary>
        public IFileFormat<TUnderlying> UnderlyingFormat => format;

        /// <inheritdoc/>
        public abstract int Width { get; }

        /// <inheritdoc/>
        public abstract int Height { get; }

        /// <inheritdoc/>
        public abstract double HorizontalResolution { get; }

        /// <inheritdoc/>
        public abstract double VerticalResolution { get; }

        /// <inheritdoc/>
        public abstract bool HasAlpha { get; }

        /// <inheritdoc/>
        public abstract bool Is32bppArgb { get; }

        /// <inheritdoc/>
        public abstract IReadOnlyList<Color> Palette { get; }

        /// <inheritdoc/>
        public virtual int? PaletteSize => Palette.Count;

        /// <inheritdoc/>
        public abstract int BitDepth { get; }

        /// <inheritdoc/>
        public abstract bool? IsLossless { get; }

        /// <inheritdoc/>
        public abstract IReadOnlyList<KeyValuePair<object, object>> Metadata { get; }

        /// <inheritdoc/>
        public abstract IReadOnlyList<KeyValuePair<long, ReadOnlyMemory<byte>>> RawMetadata { get; }

        /// <inheritdoc cref="IIdentityKey.DataKey"/>
        public IFileFormat<IImage<TUnderlying>> Format => this;

        IFileFormat<IImage> IImage.Format => this;

        string? FormatMediaType => format.GetMediaType(UnderlyingImage);

        string? FormatExtension => format.GetExtension(UnderlyingImage);

        /// <inheritdoc/>
        public abstract Color GetPixel(int x, int y);

        /// <inheritdoc/>
        public abstract void SetPixel(int x, int y, Color color);

        /// <inheritdoc/>
        public abstract IImageData GetData();

        /// <inheritdoc/>
        public abstract IImage? GetThumbnail();

        /// <inheritdoc/>
        public abstract void Save(Stream output, string mediaType);

        /// <inheritdoc/>
        public virtual IImage Resize(int newWidth, int newHeight, bool preserveResolution, bool use32bppArgb, Color backgroundColor)
        {
            var clone = Clone(use32bppArgb);
            clone.ResizeInPlace(newWidth, newHeight, preserveResolution, backgroundColor);
            return clone;
        }

        /// <inheritdoc/>
        public abstract void ResizeInPlace(int newWidth, int newHeight, bool preserveResolution, Color backgroundColor);

        /// <inheritdoc/>
        public virtual IImage RotateFlip(int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical, bool use32bppArgb)
        {
            var clone = Clone(use32bppArgb);
            clone.RotateFlipInPlace(clockwise90DegreeTurns, flipHorizontal, flipVertical);
            return clone;
        }

        /// <inheritdoc/>
        public abstract void RotateFlipInPlace(int clockwise90DegreeTurns, bool flipHorizontal, bool flipVertical);

        /// <inheritdoc/>
        public virtual IImage Crop(Rectangle cropRectangle, bool use32bppArgb = false)
        {
            var clone = Clone(use32bppArgb);
            clone.CropInPlace(cropRectangle);
            return clone;
        }

        /// <inheritdoc/>
        public abstract void CropInPlace(Rectangle cropRectangle);

        /// <inheritdoc cref="IImage.Clone(bool)"/>
        public abstract ImageBase<TUnderlying> Clone(bool use32bppArgb);

        /// <summary>
        /// Replaces the underlying image with a new instance. The original image is disposed.
        /// </summary>
        /// <param name="newUnderlyingImage">The new value of <see cref="UnderlyingImage"/>.</param>
        protected void ReplaceWith(TUnderlying newUnderlyingImage)
        {
            if(Interlocked.Exchange(ref underlyingImage, newUnderlyingImage) is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc cref="IIdentityKey.ReferenceKey"/>
        protected virtual object? ReferenceKey => underlyingImage;

        /// <inheritdoc cref="IIdentityKey.DataKey"/>
        protected virtual object? DataKey => null;

        object? IIdentityKey.ReferenceKey => ReferenceKey;

        object? IIdentityKey.DataKey => DataKey;

        IImage IImage.Clone(bool use32bppArgb) => Clone(use32bppArgb);

        object ICloneable.Clone() => Clone(false);

        /// <inheritdoc/>
        public Color this[int x, int y] {
            get => GetPixel(x, y);
            set => SetPixel(x, y, value);
        }

        /// <inheritdoc/>
        public Color this[Point point] {
            get => this[point.X, point.Y];
            set => this[point.X, point.Y] = value;
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if(Interlocked.Exchange(ref underlyingImage, null) is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~ImageBase()
        {
            Dispose(false);
        }

        ImageBase<TUnderlying> AsCompatible(object obj, string argName)
        {
            return
                (obj as ImageBase<TUnderlying>)
                ?? throw new ArgumentException($"The object must derive from {typeof(ImageBase<TUnderlying>)}.", argName);
        }

        string? IFileFormat<IImage>.GetMediaType(IImage value)
        {
            return AsCompatible(value, nameof(value)).FormatMediaType;
        }

        string? IFileFormat<IImage>.GetExtension(IImage value)
        {
            return AsCompatible(value, nameof(value)).FormatExtension;
        }

        string? IFileFormat.GetMediaType(object value)
        {
            return AsCompatible(value, nameof(value)).FormatMediaType;
        }

        string? IFileFormat.GetExtension(object value)
        {
            return AsCompatible(value, nameof(value)).FormatExtension;
        }
    }

    /// <summary>
    /// Provides an abstract implementation of <see cref="IImageData"/>.
    /// </summary>
    /// <typeparam name="TUnderlying">
    /// The type of the underlying image instance.
    /// </typeparam>
    public abstract class ImageData<TUnderlying> : MemoryManager<byte>, IImageData where TUnderlying : class
    {
        /// <summary>
        /// A reference to the image holding the data.
        /// </summary>
        protected ImageBase<TUnderlying> Image { get; }

        /// <summary>
        /// Creates a new instance of the data.
        /// </summary>
        /// <param name="image">The value of <see cref="Image"/>.</param>
        public ImageData(ImageBase<TUnderlying> image)
        {
            Image = image;
        }

        object? IIdentityKey.ReferenceKey => ((IIdentityKey)Image).ReferenceKey;

        object? IIdentityKey.DataKey => ((IIdentityKey)Image).DataKey;

        StreamFactoryAccess IStreamFactory.Access => StreamFactoryAccess.Parallel;

        /// <inheritdoc/>
        public abstract bool IsContiguous { get; }

        /// <inheritdoc/>
        public abstract int Scan0 { get; }

        /// <inheritdoc/>
        public abstract int Stride { get; }

        /// <inheritdoc/>
        public abstract int Height { get; }

        /// <inheritdoc/>
        public abstract int RowSize { get; }

        /// <inheritdoc/>
        public abstract int BitDepth { get; }

        /// <inheritdoc/>
        public abstract long Length { get; }

        FirstRow? sequenceFirstRow;

        /// <inheritdoc/>
        public virtual ReadOnlySequence<byte> Sequence {
            get {
                if(IsContiguous && Stride == RowSize)
                {
                    // No padding, natural order
                    return new(Memory);
                }
                switch(Height)
                {
                    case 0:
                        // No data to expose
                        return ReadOnlySequence<byte>.Empty;
                    case 1:
                        // Just the first row
                        return new(GetRow(0));
                }
                var firstRow = sequenceFirstRow ??= CreateSequence();
                return new(firstRow, 0, firstRow.LastRow, RowSize);
            }
        }

        FirstRow CreateSequence()
        {
            var index = Height - 1;
            var lastRow = new LastRow(this, index);
            Row previousRow = lastRow;
            while(--index >= 1)
            {
                previousRow = new Row(this, index, previousRow);
            }
            return new FirstRow(this, 0, previousRow, lastRow);
        }

        /// <inheritdoc/>
        public virtual Span<byte> this[int x, int y] {
            get => GetPixel(x, y).Span;
        }

        /// <inheritdoc/>
        public virtual Span<byte> this[Point point] {
            get => this[point.X, point.Y];
        }

        /// <inheritdoc/>
        public virtual Span<byte> this[int y] {
            get => GetRow(y).Span;
        }

        /// <inheritdoc/>
        public abstract Memory<byte> GetPixel(int x, int y);

        /// <inheritdoc/>
        public abstract Memory<byte> GetRow(int y);

        /// <inheritdoc cref="IStreamFactory.Open()"/>
        protected abstract Stream Open();

        Stream IStreamFactory.Open()
        {
            return Open();
        }

        class Row : ReadOnlySequenceSegment<byte>
        {
            public Row(ImageData<TUnderlying> data, int index, Row? next)
            {
                Memory = data.GetRow(index);
                RunningIndex = data.RowSize * index;
                Next = next;
            }
        }

        class FirstRow : Row
        {
            /// <summary>
            /// A reference to the last row of the image.
            /// </summary>
            public LastRow LastRow { get; }

            public FirstRow(ImageData<TUnderlying> data, int index, Row? next, LastRow lastRow) : base(data, index, next)
            {
                LastRow = lastRow;
            }
        }

        class LastRow : Row
        {
            /// <summary>
            /// A reference to the image data to keep it alive.
            /// </summary>
            public ImageData<TUnderlying> Data { get; }

            public LastRow(ImageData<TUnderlying> data, int index) : base(data, index, null)
            {
                Data = data;
            }
        }
    }

    /// <summary>
    /// Provides a more concrete implementation of <see cref="ImageData{TUnderlying}"/>
    /// where the underlying memory can be contiguous, calculating the row and pixel layout
    /// automatically using the <see cref="Stride"/> property.
    /// </summary>
    /// <typeparam name="TUnderlying">
    /// The type of the underlying image instance.
    /// </typeparam>
    public abstract class MemoryImageData<TUnderlying> : ImageData<TUnderlying> where TUnderlying : class
    {
        /// <inheritdoc/>
        public override bool IsContiguous => true;

        /// <inheritdoc/>
        public override int Scan0 { get; }

        /// <inheritdoc/>
        public override int Stride { get; }

        /// <inheritdoc/>
        public override int Height { get; }

        /// <inheritdoc/>
        public override int BitDepth { get; }

        /// <inheritdoc/>
        public override int RowSize { get; }

        /// <summary>
        /// The width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The size of the image data.
        /// </summary>
        public int Size { get; }

        /// <inheritdoc/>
        public override long Length => Size;

        /// <summary>
        /// Creates a new instance of the data.
        /// </summary>
        /// <param name="image">The underlying image.</param>
        /// <param name="stride">The value of <see cref="Stride"/>.</param>
        /// <param name="bitDepth">The value of <see cref="BitDepth"/>.</param>
        public MemoryImageData(ImageBase<TUnderlying> image, int stride, int bitDepth) : base(image)
        {
            Width = image.Width;
            BitDepth = bitDepth;
            RowSize = (Width * bitDepth + 7) / 8;
            Height = image.Height;
            if(Height == 0)
            {
                // No data to move in
                return;
            }
            Stride = stride;
            if(stride >= 0)
            {
                // Top-down: top row first
                Scan0 = 0;
            }else{
                stride = -stride;
                // Bottom-up: top row last
                // Scan0 is the offset of the top row relative to the bottom row
                // (which is at offset 0).
                Scan0 = (Height - 1) * stride;
            }
            // The allocated memory ends on the last byte of the last row;
            // there is no guarantee that the bytes remaining to height*stride
            // are allocated.
            Size = (Height - 1) * stride + RowSize;
        }

        private void CheckContiguous()
        {
            if(!IsContiguous)
            {
                throw new NotImplementedException("This operation is not implemented for non-contiguous memory.");
            }
        }

        /// <inheritdoc/>
        public override Memory<byte> GetPixel(int x, int y)
        {
            CheckContiguous();
            var pixelSize = Math.DivRem(BitDepth, 8, out var rem);
            if(rem == 0)
            {
                throw new NotSupportedException("This operation is not supported when the bit depth is not divisible by 8.");
            }
            return GetRow(y).Slice(x * pixelSize, pixelSize);
        }

        /// <inheritdoc/>
        public override Memory<byte> GetRow(int y)
        {
            CheckContiguous();
            return Memory.Slice(Scan0 + Stride * y, RowSize);
        }

        /// <inheritdoc/>
        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            if(elementIndex != 0)
            {
                throw new NotImplementedException();
            }
            return Memory.Pin();
        }

        /// <inheritdoc/>
        public override void Unpin()
        {
            
        }
    }
}
