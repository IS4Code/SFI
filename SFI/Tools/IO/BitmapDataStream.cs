using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Tools.IO
{
    /// <summary>
    /// Provides a read-only stream capable of reading from a memory-backed bitmap data
    /// (such as provided by System.Drawing.Imaging.BitmapData). The bytes are
    /// read starting from the first row of the image in sequence.
    /// </summary>
    public class BitmapDataStream : Stream
    {
        readonly IntPtr scan0;
        readonly int stride;
        readonly int rowBytes;
        readonly int length;
        readonly int height;

        int row;
        int columnOffset;

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
        public BitmapDataStream(IntPtr scan0, int stride, int height, int rowBytes)
        {
            this.scan0 = scan0;
            this.stride = stride;
            this.height = height;
            this.rowBytes = rowBytes;
            length = rowBytes * height;
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
        public BitmapDataStream(IntPtr scan0, int stride, int height, int width, int bpp) : this(scan0, stride, height, (width * bpp + 7) / 8)
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
                row = Math.DivRem((int)value, rowBytes, out var column);
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
                Marshal.Copy(scan0 + stride * row + columnOffset, buffer, offset, rest);
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
}
