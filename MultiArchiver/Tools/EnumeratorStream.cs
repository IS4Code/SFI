using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.Tools
{
    public class EnumeratorStream<TException> : Stream where TException : Exception
    {
        readonly IEnumerator<ArraySegment<byte>> enumerator;

        public EnumeratorStream(IEnumerator<ArraySegment<byte>> enumerator, long length)
        {
            this.enumerator = enumerator;
            Length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
                
        }

        ArraySegment<byte> remaining;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int total = 0;
            while(count > 0)
            {
                while(remaining.Count == 0)
                {
                    if(!MoveNext()) return total;
                }
                int read = Math.Min(remaining.Count, count);
                Array.Copy(remaining.Array, remaining.Offset, buffer, offset, read);
                remaining = new ArraySegment<byte>(remaining.Array, remaining.Offset + read, remaining.Count - read);
                total += read;
                offset += read;
                count -= read;
            }
            return total;
        }

        public override int ReadByte()
        {
            while(remaining.Count == 0)
            {
                if(!MoveNext()) return -1;
            }
            var result = remaining.Array[remaining.Offset];
            remaining = new ArraySegment<byte>(remaining.Array, remaining.Offset + 1, remaining.Count - 1);
            return result;
        }

        bool MoveNext()
        {
            try{
                if(enumerator.MoveNext())
                {
                    remaining = enumerator.Current;
                    return true;
                }
                return false;
            }catch(TException)
            {
                return false;
            }
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

        public abstract class NoException : Exception
        {
            private NoException()
            {

            }

            public sealed override IDictionary Data => throw this;

            public sealed override string HelpLink { get => throw this; set => throw this; }

            public sealed override string Message => throw this;

            public sealed override string Source { get => throw this; set => throw this; }

            public sealed override string StackTrace => throw this;

            public sealed override Exception GetBaseException()
            {
                throw this;
            }
        }
    }
}
