using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools
{
    public sealed class ChannelStream : Stream
    {
        readonly ChannelReader<ArraySegment<byte>> channelReader;
        ArraySegment<byte> current;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override bool CanTimeout => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public ChannelStream(ChannelReader<ArraySegment<byte>> channelReader)
        {
            this.channelReader = channelReader;
        }

        public static ChannelStream Create(out ChannelWriter<ArraySegment<byte>> writer, int? capacity = null)
        {
            var ch = capacity is int i ? Channel.CreateBounded<ArraySegment<byte>>(new BoundedChannelOptions(i)
            {
                FullMode = BoundedChannelFullMode.Wait,
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            }) : Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });
            writer = ch.Writer;
            return new ChannelStream(ch.Reader);
        }

        public override void Close()
        {
            throw new NotSupportedException();
        }

        private bool TryGetNext()
        {
            try{
                if(current.Array == null)
                {
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

        private int ReadInner(byte[] buffer, ref int offset, ref int count)
        {
            if(current.Count == 0)
            {
                current = default;
                return 0;
            }
            int len = Math.Min(count, current.Count);
            Array.Copy(current.Array, current.Offset, buffer, offset, len);
            current = new ArraySegment<byte>(current.Array, current.Offset + len, current.Count - len);
            if(current.Count == 0)
            {
                current = default;
            }
            offset += len;
            count -= len;
            return len;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            int read = 0;
            while(read < count)
            {
                if(!TryGetNext())
                {
                    break;
                }
                read += ReadInner(buffer, ref offset, ref count);
            }
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            int read = 0;
            try{
                while(read < count)
                {
                    if(current.Array == null)
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

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
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

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }

        private int ReadByteInner()
        {
            if(current.Count == 0)
            {
                current = default;
                return -1;
            }
            var result = current.Array[current.Offset];
            current = new ArraySegment<byte>(current.Array, current.Offset + 1, current.Count - 1);
            if(current.Count == 0)
            {
                current = default;
            }
            return result;
        }

        public override int ReadByte()
        {
            int result = -1;
            while(result == -1)
            {
                if(!TryGetNext())
                {
                    break;
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
                    if(current.Array == null)
                    {
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
