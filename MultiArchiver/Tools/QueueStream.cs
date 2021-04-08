using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Tools
{
    internal sealed class QueueStream : Stream
    {
        readonly SemaphoreSlim syncSemaphore = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim readSemaphore = new SemaphoreSlim(0, 1);

        ArraySegment<byte> currentData = new ArraySegment<byte>(Array.Empty<byte>(), 0, 0);

        public bool AutoFlush { get; set; } = true;

        public bool ForceClose { get; set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override bool CanTimeout => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private bool FlushInner()
        {
            if(currentData.Array == null)
            {
                throw new IOException();
            }
            if(currentData.Count == 0)
            {
                return true;
            }
            return false;
        }

        public override void Flush()
        {
            while(true)
            {
                syncSemaphore.Wait();
                try{
                    if(FlushInner())
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                await syncSemaphore.WaitAsync(cancellationToken);
                try{
                    if(FlushInner())
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }
        }

        private bool CloseInner()
        {
            if(currentData.Array == null)
            {
                throw new IOException();
            }
            int remaining = currentData.Count;
            if(ForceClose || remaining == 0)
            {
                currentData = default;
                if(remaining == 0) readSemaphore.Release();
                return true;
            }
            return false;
        }

        public override void Close()
        {
            while(true)
            {
                syncSemaphore.Wait();
                try{
                    if(CloseInner())
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }

            base.Close();
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                await syncSemaphore.WaitAsync(cancellationToken);
                try{
                    if(CloseInner())
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }
        }

        private int ReadInner(byte[] buffer, int offset, int count, out int remaining)
        {
            if(currentData.Array == null)
            {
                remaining = 0;
                return 0;
            }
            int read = Math.Min(count, currentData.Count);
            remaining = currentData.Count - read;
            if(read > 0)
            {
                Array.Copy(currentData.Array, currentData.Offset, buffer, offset, read);
                currentData = new ArraySegment<byte>(currentData.Array, currentData.Offset + read, remaining);
                return read;
            }
            return -1;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            while(true)
            {
                int remaining = 0;
                readSemaphore.Wait();
                try{
                    syncSemaphore.Wait();
                    try{
                        int read = ReadInner(buffer, offset, count, out remaining);
                        if(read >= 0)
                        {
                            return read;
                        }
                    }finally{
                        syncSemaphore.Release();
                    }
                }finally{
                    if(remaining != 0) readSemaphore.Release();
                }
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if(count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            while(true)
            {
                int remaining = 0;
                await readSemaphore.WaitAsync(cancellationToken);
                try{
                    await syncSemaphore.WaitAsync(cancellationToken);
                    try{
                        int read = ReadInner(buffer, offset, count, out remaining);
                        if(read >= 0)
                        {
                            return read;
                        }
                    }finally{
                        syncSemaphore.Release();
                    }
                }finally{
                    if(remaining != 0) readSemaphore.Release();
                }
            }
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

        private int ReadByteInner(out int remaining)
        {
            if(currentData.Array == null)
            {
                remaining = 0;
                return -1;
            }
            if(currentData.Count > 0)
            {
                remaining = currentData.Count - 1;
                byte result = currentData.Array[currentData.Offset];
                currentData = new ArraySegment<byte>(currentData.Array, currentData.Offset + 1, remaining);
                return result;
            }
            remaining = 0;
            return -2;
        }

        public override int ReadByte()
        {
            while(true)
            {
                int remaining = 0;
                readSemaphore.Wait();
                try{
                    syncSemaphore.Wait();
                    try{
                        int result = ReadByteInner(out remaining);
                        if(result >= -1)
                        {
                            return result;
                        }
                    }finally{
                        syncSemaphore.Release();
                    }
                }finally{
                    if(remaining != 0) readSemaphore.Release();
                }
            }
        }

        public async Task<int> ReadByteAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                int remaining = 0;
                await readSemaphore.WaitAsync(cancellationToken);
                try{
                    await syncSemaphore.WaitAsync(cancellationToken);
                    try{
                        int result = ReadByteInner(out remaining);
                        if(result >= -1)
                        {
                            return result;
                        }
                    }finally{
                        syncSemaphore.Release();
                    }
                }finally{
                    if(remaining != 0) readSemaphore.Release();
                }
            }
        }

        private bool WriteInner(byte[] buffer, int offset, int count)
        {
            if(currentData.Array == null)
            {
                throw new IOException();
            }
            if(currentData.Count == 0)
            {
                currentData = new ArraySegment<byte>(buffer, offset, count);
                readSemaphore.Release();
                return true;
            }
            return false;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if(count == 0) return;
            while(true)
            {
                syncSemaphore.Wait();
                try{
                    if(WriteInner(buffer, offset, count))
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }
            if(AutoFlush) Flush();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if(count == 0) return;
            while(true)
            {
                await syncSemaphore.WaitAsync(cancellationToken);
                try{
                    if(WriteInner(buffer, offset, count))
                    {
                        break;
                    }
                }finally{
                    syncSemaphore.Release();
                }
            }
            if(AutoFlush) await FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var task = WriteAsync(buffer, offset, count);
            var completion = new TaskCompletionSource<bool>(state);
            task.ContinueWith(t =>
            {
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
