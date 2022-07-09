using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools.IO
{
    public class BitmapDataStream : Stream
    {
        readonly IntPtr scan0;
        readonly int stride;
        readonly int rowBytes;
        readonly int length;
        readonly int height;

        int row;
        int columnOffset;

        public BitmapDataStream(IntPtr scan0, int stride, int height, int rowBytes)
        {
            this.scan0 = scan0;
            this.stride = stride;
            this.height = height;
            this.rowBytes = rowBytes;
            length = rowBytes * height;
        }

        public BitmapDataStream(IntPtr scan0, int stride, int height, int width, int bpp) : this(scan0, stride, height, (width * bpp + 7) / 8)
        {

        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position {
            get {
                return row * rowBytes + columnOffset;
            }
            set {
                if(value < 0 || value > Int32.MaxValue) throw new ArgumentOutOfRangeException(nameof(value));
                row = Math.DivRem((int)value, rowBytes, out var column);
            }
        }

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

#pragma warning disable 1998
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Read(buffer, offset, count);
        }
#pragma warning restore 1998

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

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
