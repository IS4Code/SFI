using System;
using System.IO;

namespace IS4.MultiArchiver.Tools
{
    public class ReadSeekableStream : Stream
    {
        private long underlyingPosition;
        private readonly byte[] seekBackBuffer;
        private int seekBackBufferCount;
        private int seekBackBufferIndex;
        private readonly Stream underlyingStream;

        public ReadSeekableStream(Stream underlyingStream, int seekBackBufferSize)
        {
            if(!underlyingStream.CanRead)
                throw new Exception("Provided stream " + underlyingStream + " is not readable");
            this.underlyingStream = underlyingStream;
            seekBackBuffer = new byte[seekBackBufferSize];
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int copiedFromBackBufferCount = 0;
            if(seekBackBufferIndex < seekBackBufferCount)
            {
                copiedFromBackBufferCount = Math.Min(count, seekBackBufferCount - seekBackBufferIndex);
                Buffer.BlockCopy(seekBackBuffer, seekBackBufferIndex, buffer, offset, copiedFromBackBufferCount);
                offset += copiedFromBackBufferCount;
                count -= copiedFromBackBufferCount;
                seekBackBufferIndex += copiedFromBackBufferCount;
            }
            int bytesReadFromUnderlying = 0;
            if(count > 0)
            {
                bytesReadFromUnderlying = underlyingStream.Read(buffer, offset, count);
                if(bytesReadFromUnderlying > 0)
                {
                    underlyingPosition += bytesReadFromUnderlying;

                    var copyToBufferCount = Math.Min(bytesReadFromUnderlying, seekBackBuffer.Length);
                    var copyToBufferOffset = Math.Min(seekBackBufferCount, seekBackBuffer.Length - copyToBufferCount);
                    var bufferBytesToMove = Math.Min(seekBackBufferCount - 1, copyToBufferOffset);

                    if(bufferBytesToMove > 0)
                        Buffer.BlockCopy(seekBackBuffer, seekBackBufferCount - bufferBytesToMove, seekBackBuffer, 0, bufferBytesToMove);
                    Buffer.BlockCopy(buffer, offset, seekBackBuffer, copyToBufferOffset, copyToBufferCount);
                    seekBackBufferCount = Math.Min(seekBackBuffer.Length, seekBackBufferCount + copyToBufferCount);
                    seekBackBufferIndex = seekBackBufferCount;
                }
            }
            return copiedFromBackBufferCount + bytesReadFromUnderlying;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(origin == SeekOrigin.End)
                return SeekFromEnd((int)Math.Max(0, -offset));

            var relativeOffset = origin == SeekOrigin.Current
                ? offset
                : offset - Position;

            if(relativeOffset == 0)
                return Position;
            else if(relativeOffset > 0)
                return SeekForward(relativeOffset);
            else
                return SeekBackwards(-relativeOffset);
        }

        private long SeekForward(long origOffset)
        {
            long offset = origOffset;
            var seekBackBufferLength = seekBackBuffer.Length;

            int backwardSoughtBytes = seekBackBufferCount - seekBackBufferIndex;
            int seekForwardInBackBuffer = (int)Math.Min(offset, backwardSoughtBytes);
            offset -= seekForwardInBackBuffer;
            seekBackBufferIndex += seekForwardInBackBuffer;

            if(offset > 0)
            {
                // first completely fill seekBackBuffer to remove special cases from while loop below
                if(seekBackBufferCount < seekBackBufferLength)
                {
                    var maxRead = seekBackBufferLength - seekBackBufferCount;
                    if(offset < maxRead)
                        maxRead = (int)offset;
                    var bytesRead = underlyingStream.Read(seekBackBuffer, seekBackBufferCount, maxRead);
                    underlyingPosition += bytesRead;
                    seekBackBufferCount += bytesRead;
                    seekBackBufferIndex = seekBackBufferCount;
                    if(bytesRead < maxRead)
                    {
                        if(seekBackBufferCount < offset)
                            throw new NotSupportedException("Reached end of stream seeking forward " + origOffset + " bytes");
                        return Position;
                    }
                    offset -= bytesRead;
                }

                // now alternate between filling tempBuffer and seekBackBuffer
                bool fillTempBuffer = true;
                var tempBuffer = new byte[seekBackBufferLength];
                while(offset > 0)
                {
                    var maxRead = offset < seekBackBufferLength ? (int)offset : seekBackBufferLength;
                    var bytesRead = underlyingStream.Read(fillTempBuffer ? tempBuffer : seekBackBuffer, 0, maxRead);
                    underlyingPosition += bytesRead;
                    var bytesReadDiff = maxRead - bytesRead;
                    offset -= bytesRead;
                    if(bytesReadDiff > 0 /* reached end-of-stream */ || offset == 0)
                    {
                        if(fillTempBuffer)
                        {
                            if(bytesRead > 0)
                            {
                                Buffer.BlockCopy(seekBackBuffer, bytesRead, seekBackBuffer, 0, bytesReadDiff);
                                Buffer.BlockCopy(tempBuffer, 0, seekBackBuffer, bytesReadDiff, bytesRead);
                            }
                        } else
                        {
                            if(bytesRead > 0)
                                Buffer.BlockCopy(seekBackBuffer, 0, seekBackBuffer, bytesReadDiff, bytesRead);
                            Buffer.BlockCopy(tempBuffer, bytesRead, seekBackBuffer, 0, bytesReadDiff);
                        }
                        if(offset > 0)
                            throw new NotSupportedException("Reached end of stream seeking forward " + origOffset + " bytes");
                    }
                    fillTempBuffer = !fillTempBuffer;
                }
            }
            return Position;
        }

        private long SeekBackwards(long offset)
        {
            var intOffset = (int)offset;
            if(offset > int.MaxValue || intOffset > seekBackBufferIndex)
                throw new NotSupportedException("Cannot currently seek backwards more than " + seekBackBufferIndex + " bytes");
            seekBackBufferIndex -= intOffset;
            return Position;
        }

        private long SeekFromEnd(long offset)
        {
            var intOffset = (int)offset;
            var seekBackBufferLength = seekBackBuffer.Length;
            if(offset > int.MaxValue || intOffset > seekBackBufferLength)
                throw new NotSupportedException("Cannot seek backwards from end more than " + seekBackBufferLength + " bytes");

            // first completely fill seekBackBuffer to remove special cases from while loop below
            if(seekBackBufferCount < seekBackBufferLength)
            {
                var maxRead = seekBackBufferLength - seekBackBufferCount;
                var bytesRead = underlyingStream.Read(seekBackBuffer, seekBackBufferCount, maxRead);
                underlyingPosition += bytesRead;
                seekBackBufferCount += bytesRead;
                seekBackBufferIndex = Math.Max(0, seekBackBufferCount - intOffset);
                if(bytesRead < maxRead)
                {
                    if(seekBackBufferCount < intOffset)
                        throw new NotSupportedException("Could not seek backwards from end " + intOffset + " bytes");
                    return Position;
                }
            } else
            {
                seekBackBufferIndex = seekBackBufferCount;
            }

            // now alternate between filling tempBuffer and seekBackBuffer
            bool fillTempBuffer = true;
            var tempBuffer = new byte[seekBackBufferLength];
            while(true)
            {
                var bytesRead = underlyingStream.Read(fillTempBuffer ? tempBuffer : seekBackBuffer, 0, seekBackBufferLength);
                underlyingPosition += bytesRead;
                var bytesReadDiff = seekBackBufferLength - bytesRead;
                if(bytesReadDiff > 0) // reached end-of-stream
                {
                    if(fillTempBuffer)
                    {
                        if(bytesRead > 0)
                        {
                            Buffer.BlockCopy(seekBackBuffer, bytesRead, seekBackBuffer, 0, bytesReadDiff);
                            Buffer.BlockCopy(tempBuffer, 0, seekBackBuffer, bytesReadDiff, bytesRead);
                        }
                    } else
                    {
                        if(bytesRead > 0)
                            Buffer.BlockCopy(seekBackBuffer, 0, seekBackBuffer, bytesReadDiff, bytesRead);
                        Buffer.BlockCopy(tempBuffer, bytesRead, seekBackBuffer, 0, bytesReadDiff);
                    }
                    seekBackBufferIndex -= intOffset;
                    return Position;
                }
                fillTempBuffer = !fillTempBuffer;
            }
        }

        public override long Position {
            get { return underlyingPosition - (seekBackBufferCount - seekBackBufferIndex); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                underlyingStream.Close();
            base.Dispose(disposing);
        }

        public override bool CanTimeout { get { return underlyingStream.CanTimeout; } }
        public override bool CanWrite { get { return underlyingStream.CanWrite; } }
        public override long Length { get { return underlyingStream.Length; } }
        public override void SetLength(long value) { underlyingStream.SetLength(value); }
        public override void Write(byte[] buffer, int offset, int count) { underlyingStream.Write(buffer, offset, count); }
        public override void Flush() { underlyingStream.Flush(); }
    }
}
