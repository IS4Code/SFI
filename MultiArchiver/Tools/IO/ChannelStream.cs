using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools.IO
{
    /// <summary>
    /// Provides a read-only stream using an instance of <see cref="ChannelReader{T}"/>
    /// using an arbitrary sequence of bytes as the source of data.
    /// </summary>
    /// <typeparam name="TSequence">The collection of bytes provided by the channel.</typeparam>
    public abstract class ChannelStream<TSequence> : Stream, IEnumerator<TSequence>, IAsyncEnumerator<TSequence> where TSequence : struct, IReadOnlyCollection<byte>
    {
        readonly ChannelReader<TSequence> channelReader;

        /// <summary>
        /// The current element in the sequence.
        /// It is sliced when it is not read as a whole.
        /// </summary>
        TSequence current;

        public sealed override bool CanRead => true;

        public sealed override bool CanSeek => false;

        public sealed override bool CanWrite => false;

        public sealed override bool CanTimeout => false;

        public sealed override long Length => throw new NotSupportedException();

        public sealed override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        TSequence IEnumerator<TSequence>.Current => current;

        TSequence IAsyncEnumerator<TSequence>.Current => current;

        object IEnumerator.Current => current;

        /// <summary>
        /// Creates a new stream instance from a channel reader.
        /// </summary>
        /// <param name="channelReader">
        /// The reader for the channel providing the byte sequences to read.
        /// </param>
        public ChannelStream(ChannelReader<TSequence> channelReader)
        {
            this.channelReader = channelReader;
        }

        /// <summary>
        /// Creates a new channel and retrieve its reader and writer.
        /// </summary>
        /// <param name="writer">The variable to receive the writer for the created channel.</param>
        /// <param name="capacity">The capacity of the channel, if it should be bounded.</param>
        /// <returns>The reader for the created channel.</returns>
        /// <remarks>
        /// The channel is created with the following settings:
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="ChannelOptions.AllowSynchronousContinuations"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="ChannelOptions.SingleReader"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="ChannelOptions.SingleWriter"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="BoundedChannelOptions.FullMode"/> (if <paramref name="capacity"/> is provided)</term>
        ///     <description><see cref="BoundedChannelFullMode.Wait"/></description>
        /// </item>
        /// </list>
        /// </remarks>
        protected static ChannelReader<TSequence> CreateReader(out ChannelWriter<TSequence> writer, int? capacity = null)
        {
            var ch = capacity is int i ? Channel.CreateBounded<TSequence>(new BoundedChannelOptions(i)
            {
                FullMode = BoundedChannelFullMode.Wait,
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            }) : Channel.CreateUnbounded<TSequence>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });
            writer = ch.Writer;
            return ch.Reader;
        }

        /// <summary>
        /// Synchronously retrieves the next byte sequence from
        /// the channel and stores it in <see cref="current"/>.
        /// </summary>
        /// <returns>True if a sequence was retrieved.</returns>
        private bool TryGetNext()
        {
            try{
                if(!channelReader.TryRead(out current))
                {
                    // An element is not readily available; get it from the task
                    var valueTask = channelReader.ReadAsync();
                    if(valueTask.IsCompletedSuccessfully)
                    {
                        current = valueTask.Result;
                    }else if(valueTask.IsFaulted)
                    {
                        var task = valueTask.AsTask();
                        if(task.Exception.InnerExceptions.Count == 1)
                        {
                            if(task.Exception.InnerException is ChannelClosedException)
                            {
                                // The channel was closed from the other side
                                return false;
                            }
                            ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                        }
                        throw task.Exception;
                    }else{
                        current = valueTask.AsTask().Result;
                    }
                }
                return true;
            }catch(ChannelClosedException)
            {
                return false;
            }catch(AggregateException agg) when(agg.InnerExceptions.Count == 1)
            {
                if(!(agg.InnerException is ChannelClosedException))
                {
                    ExceptionDispatchInfo.Capture(agg.InnerException).Throw();
                    throw;
                }
                return false;
            }
        }

        /// <summary>
        /// When overriden in a derived type, copies <paramref name="len"/> bytes from the sequence
        /// <paramref name="current"/> into <paramref name="buffer"/>, and adjusts it to
        /// start with the bytes after that.
        /// </summary>
        /// <param name="current">The variable storing the byte sequence to copy from.</param>
        /// <param name="buffer">The target array to receive the bytes.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> to start copying to.</param>
        /// <param name="len">The number of bytes to copy.</param>
        protected abstract void ReadFrom(ref TSequence current, byte[] buffer, int offset, int len);
        
        /// <summary>
        /// Reads from the current byte sequence stored in <see cref="current"/>
        /// into <paramref name="buffer"/>, calling <see cref="ReadFrom(ref TSequence, byte[], int, int)"/>.
        /// </summary>
        /// <param name="buffer">The array to receive the bytes.</param>
        /// <param name="offset">The offset to write the data to, increased by its length after the operation.</param>
        /// <param name="count">The number of bytes to store, decreased by its length after the operation.</param>
        /// <returns>The number of copied bytes, or 0 if there are no more bytes in the current sequence.</returns>
        private int ReadInner(byte[] buffer, ref int offset, ref int count)
        {
            if(current.Count == 0)
            {
                // No data left; invalidate it and return 0
                current = default;
                return 0;
            }
            int len = Math.Min(count, current.Count);
            ReadFrom(ref current, buffer, offset, len);
            if(current.Count == 0)
            {
                // No data remained, invalidate it
                current = default;
            }
            offset += len;
            count -= len;
            return len;
        }

        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            int read = 0;
            while(read < count)
            {
                if(current.Count == 0)
                {
                    // The current sequence was depleted, get the next one
                    if(!TryGetNext())
                    {
                        break;
                    }
                }
                read += ReadInner(buffer, ref offset, ref count);
            }
            return read;
        }

        public sealed async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            int read = 0;
            try{
                while(read < count)
                {
                    if(current.Count == 0)
                    {
                        // The current sequence was depleted, get the next one
                        if(!channelReader.TryRead(out current))
                        {
                            current = await channelReader.ReadAsync(cancellationToken);
                        }
                    }
                    read += ReadInner(buffer, ref offset, ref count);
                }
            }catch(ChannelClosedException)
            {

            }
            return read;
        }

        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var task = ReadAsync(buffer, offset, count);
            var completion = new TaskCompletionSource<int>(state);
            task.ContinueWith(t => {
                if(t.IsFaulted)
                {
                    completion.TrySetException(t.Exception.InnerExceptions);
                }else if(t.IsCanceled)
                {
                    completion.TrySetCanceled();
                }else{
                    completion.TrySetResult(t.Result);
                }
                callback?.Invoke(completion.Task);
            }, TaskScheduler.Default);
            return completion.Task;
        }

        public sealed override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }

        /// <summary>
        /// Reads a single byte from <paramref name="current"/> and modifies it
        /// to start on the next byte.
        /// </summary>
        /// <param name="current">The variable to provide the input sequence.</param>
        /// <returns>The first byte of the sequence.</returns>
        protected abstract byte ReadFrom(ref TSequence current);

        /// <summary>
        /// Reads a single byte from <see cref="current"/> and adjusts it.
        /// </summary>
        /// <returns>The next byte, or -1 if the sequence is empty.</returns>
        private int ReadByteInner()
        {
            if(current.Count == 0)
            {
                // No data left; invalidate it and return -1
                current = default;
                return -1;
            }
            var result = ReadFrom(ref current);
            if(current.Count == 0)
            {
                // No data remaining; invalidate it
                current = default;
            }
            return result;
        }

        public sealed override int ReadByte()
        {
            int result = -1;
            while(result == -1)
            {
                if(current.Count == 0)
                {
                    // The current sequence was depleted, get the next one
                    if(!TryGetNext())
                    {
                        break;
                    }
                }
                result = ReadByteInner();
            }
            return -1;
        }

        public async Task<int> ReadByteAsync(CancellationToken cancellationToken)
        {
            int result = -1;
            try{
                while(result != -1)
                {
                    if(current.Count == 0)
                    {
                        // The current sequence was depleted, get the next one
                        if(!channelReader.TryRead(out current))
                        {
                            current = await channelReader.ReadAsync(cancellationToken);
                        }
                    }
                    result = ReadByteInner();
                }
            }catch(ChannelClosedException)
            {

            }
            return result;
        }

        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public sealed override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public sealed override void Flush()
        {
            throw new NotSupportedException();
        }

        public sealed override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        bool IEnumerator.MoveNext()
        {
            return TryGetNext();
        }

        async ValueTask<bool> IAsyncEnumerator<TSequence>.MoveNextAsync()
        {
            try{
                if(!channelReader.TryRead(out current))
                {
                    current = await channelReader.ReadAsync();
                }
                return true;
            }catch(ChannelClosedException)
            {
                return false;
            }
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            Dispose();
            return default;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}
