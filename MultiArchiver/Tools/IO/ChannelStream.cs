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
    public abstract class ChannelStream<TSequence> : Stream, IEnumerator<TSequence> where TSequence : struct, IReadOnlyCollection<byte>
    {
        readonly ChannelReader<TSequence> channelReader;
        TSequence current;

        public sealed override bool CanRead => true;

        public sealed override bool CanSeek => false;

        public sealed override bool CanWrite => false;

        public sealed override bool CanTimeout => false;

        public sealed override long Length => throw new NotSupportedException();

        public sealed override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        TSequence IEnumerator<TSequence>.Current => current;

        object IEnumerator.Current => current;

        public ChannelStream(ChannelReader<TSequence> channelReader)
        {
            this.channelReader = channelReader;
        }

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

        private bool TryGetNext()
        {
            try{
                if(!channelReader.TryRead(out current))
                {
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

        protected abstract void ReadFrom(ref TSequence current, byte[] buffer, int offset, int len);
        
        private int ReadInner(byte[] buffer, ref int offset, ref int count)
        {
            if(current.Count == 0)
            {
                current = default;
                return 0;
            }
            int len = Math.Min(count, current.Count);
            ReadFrom(ref current, buffer, offset, len);
            if(current.Count == 0)
            {
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
                    if(!TryGetNext())
                    {
                        break;
                    }
                }
                read += ReadInner(buffer, ref offset, ref count);
            }
            return read;
        }

        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
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
            task.ContinueWith(t =>
            {
                if(t.IsFaulted) completion.TrySetException(t.Exception.InnerExceptions);
                else if(t.IsCanceled) completion.TrySetCanceled();
                else completion.TrySetResult(t.Result);
                callback?.Invoke(completion.Task);
            }, TaskScheduler.Default);
            return completion.Task;
        }

        public sealed override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }

        protected abstract byte ReadFrom(ref TSequence current);

        private int ReadByteInner(ref TSequence current)
        {
            if(current.Count == 0)
            {
                current = default;
                return -1;
            }
            var result = ReadFrom(ref current);
            if(current.Count == 0)
            {
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
                    if(!TryGetNext())
                    {
                        break;
                    }
                }
                result = ReadByteInner(ref current);
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
                        if(!channelReader.TryRead(out current))
                        {
                            current = await channelReader.ReadAsync(cancellationToken);
                        }
                    }
                    result = ReadByteInner(ref current);
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

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}
