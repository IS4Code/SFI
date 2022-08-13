using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools.IO
{
    /// <summary>
    /// Provides implementation for a stream reading data from an instance of
    /// <see cref="IEnumerator{T}"/> of <see cref="ArraySegment{T}"/> of
    /// <see cref="Byte"/>.
    /// </summary>
    /// <typeparam name="TException">
    /// The exception type to catch from 
    /// <see cref="IEnumerator.MoveNext"/> during reading; use
    /// <see cref="NoException"/> if nothing should be caught.
    /// </typeparam>
    public class EnumeratorStream<TException> : Stream where TException : Exception
    {
        readonly IEnumerator<ArraySegment<byte>> enumerator;

        ArraySegment<byte> remaining;

        /// <summary>
        /// Creates a new instance of the stream.
        /// </summary>
        /// <param name="enumerator">The enumerator to take the data from.</param>
        /// <param name="length">The expected length of the stream.</param>
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            int total = 0;
            while(count > 0)
            {
                while(remaining.Count == 0)
                {
                    // Try get another sequence if this one is depleted
                    if(!MoveNext()) return total;
                }
                int read = Math.Min(remaining.Count, count);
                remaining.Slice(0, read).CopyTo(buffer, offset);
                remaining = remaining.Slice(read);
                total += read;
                offset += read;
                count -= read;
            }
            return total;
        }

#pragma warning disable 1998
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Read(buffer, offset, count);
        }
#pragma warning restore 1998

        public override int ReadByte()
        {
            while(remaining.Count == 0)
            {
                // Try get another sequence if this one is depleted
                if(!MoveNext()) return -1;
            }
            var result = remaining.At(0);
            remaining = remaining.Slice(1);
            return result;
        }

        /// <summary>
        /// Retrieves the next sequence from the enumerator and stores it in
        /// <see cref="remaining"/>.
        /// </summary>
        /// <returns>True if a sequence was retrieved.</returns>
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

        /// <summary>
        /// This exception should never be thrown; instead its use indicates that no
        /// exceptions should be caught in <see cref="MoveNext"/>.
        /// </summary>
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
