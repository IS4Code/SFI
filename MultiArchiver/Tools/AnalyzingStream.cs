using System;
using System.IO;

namespace IS4.MultiArchiver.Tools
{
    internal class AnalyzingStream : Stream
    {
        readonly Stream baseStream;
        readonly Action<ArraySegment<byte>> action;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => throw new NotSupportedException(); }

        public AnalyzingStream(Stream baseStream, Action<ArraySegment<byte>> action)
        {
            this.baseStream = baseStream;
            this.action = action;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = baseStream.Read(buffer, offset, count);
            action(new ArraySegment<byte>(buffer, offset, read));
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
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
