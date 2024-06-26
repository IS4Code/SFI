﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Tools.IO
{
    /// <summary>
    /// Provides a read-only virtual data source that views a sequence of rows
    /// as a contiguous storage.
    /// </summary>
    public abstract class BitmapRowDataStream : Stream
    {
        readonly int rowBytes;
        readonly int length;
        readonly int height;

        int row;
        int columnOffset;

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="height">The number of rows in the image.</param>
        /// <param name="rowBytes">The number of bytes in a single row.</param>
        public BitmapRowDataStream(int height, int rowBytes)
        {
            this.height = height;
            this.rowBytes = rowBytes;
            length = rowBytes * height;
        }

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="height">The number of rows in the image.</param>
        /// <param name="width">The number of pixels on a row.</param>
        /// <param name="bpp">The number of bits representing a single pixel.</param>
        public BitmapRowDataStream(int height, int width, int bpp) : this(height, (width * bpp + 7) / 8)
        {

        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => length;

        /// <inheritdoc/>
        public override long Position {
            get {
                return row * rowBytes + columnOffset;
            }
            set {
                if(value < 0 || value > Int32.MaxValue) throw new ArgumentOutOfRangeException(nameof(value));
                row = Math.DivRem((int)value, rowBytes, out columnOffset);
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if(count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            int read = 0;
            while(count > 0)
            {
                if(row >= height)
                {
                    break;
                }
                var rest = Math.Min(count, rowBytes - columnOffset);
                CopyData(row, columnOffset, new(buffer, offset, rest));
                read += rest;
                offset += rest;
                columnOffset += rest;
                count -= rest;
                if(columnOffset == rowBytes)
                {
                    columnOffset = 0;
                    row++;
                }
            }
            return read;
        }

        /// <summary>
        /// Fills <paramref name="target"/> using the pixel data from a single row.
        /// </summary>
        /// <param name="row">The index of the row.</param>
        /// <param name="offset">The offset within the row.</param>
        /// <param name="target">The target segment to copy the data to.</param>
        /// <exception cref="ArgumentException"><paramref name="target"/> is too big for the row data.</exception>
        protected abstract void CopyData(int row, int offset, ArraySegment<byte> target);

        /// <inheritdoc/>
        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;
                case SeekOrigin.Current:
                    return Position += offset;
                case SeekOrigin.End:
                    return Position = Length + offset;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Provides a read-only stream capable of reading from a memory-backed bitmap data
    /// (such as provided by System.Drawing.Imaging.BitmapData). The bytes are
    /// read starting from the first row of the image in sequence.
    /// </summary>
    public class BitmapDataStream : BitmapRowDataStream
    {
        readonly IntPtr scan0;
        readonly int stride;

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="scan0">The pointer to the beginning of the first row of the image in memory.</param>
        /// <param name="stride">
        /// The number of bytes between the first pixels on two consecutive rows;
        /// could be negative to indicate that the rows go in reverse order in memory.
        /// </param>
        /// <param name="height">The number of rows in the image.</param>
        /// <param name="rowBytes">The number of bytes in a single row.</param>
        public BitmapDataStream(IntPtr scan0, int stride, int height, int rowBytes) : base(height, rowBytes)
        {
            this.scan0 = scan0;
            this.stride = stride;
        }

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="scan0">The pointer to the beginning of the first row of the image in memory.</param>
        /// <param name="stride">
        /// The number of bytes between the first pixels on two consecutive rows;
        /// could be negative to indicate that the rows go in reverse order in memory.
        /// </param>
        /// <param name="height">The number of rows in the image.</param>
        /// <param name="width">The number of pixels on a row.</param>
        /// <param name="bpp">The number of bits representing a single pixel.</param>
        public BitmapDataStream(IntPtr scan0, int stride, int height, int width, int bpp) : base(height, width, bpp)
        {
            this.scan0 = scan0;
            this.stride = stride;
        }

        /// <inheritdoc/>
        protected override void CopyData(int row, int offset, ArraySegment<byte> target)
        {
            Marshal.Copy(scan0 + stride * row + offset, target.Array, target.Offset, target.Count);
        }
    }
}
