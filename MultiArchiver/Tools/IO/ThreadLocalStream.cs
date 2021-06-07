using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools.IO
{
    public sealed class ThreadLocalStream : Stream
    {
        readonly ThreadLocal<Stream> local;
        Stream stream => local.Value;

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override bool CanTimeout => stream.CanTimeout;

        public override long Length => stream.Length;

        public override long Position { get => stream.Position; set => stream.Position = value; }

        public override int ReadTimeout { get => stream.ReadTimeout; set => stream.ReadTimeout = value; }

        public override int WriteTimeout { get => stream.WriteTimeout; set => stream.WriteTimeout = value; }

        public ThreadLocalStream(Func<Stream> streamFactory)
        {
            local = new ThreadLocal<Stream>(streamFactory);
        }

        public override void Close()
        {
            stream.Close();
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return stream.ReadByte();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var task = ReadAsync(buffer, offset, count);
            var completion = new TaskCompletionSource<int>(state);
            task.ContinueWith(t => {
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

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var task = WriteAsync(buffer, offset, count);
            var completion = new TaskCompletionSource<bool>(state);
            task.ContinueWith(t => {
                if(t.IsFaulted) completion.TrySetException(t.Exception.InnerExceptions);
                else if(t.IsCanceled) completion.TrySetCanceled();
                else completion.TrySetResult(true);
                callback?.Invoke(completion.Task);
            }, TaskScheduler.Default);
            return completion.Task;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _ = ((Task<bool>)asyncResult).Result;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                stream.Dispose();
            }
        }
    }
}
