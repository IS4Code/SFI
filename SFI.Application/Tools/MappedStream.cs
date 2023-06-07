using IS4.SFI.Tools;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Provides a custom stream with a buffer mapped to external storage.
    /// </summary>
    public abstract class MappedStream : MemoryStream
    {
        bool loaded;

        /// <summary>
        /// Pre-loads the data into the stream.
        /// </summary>
        /// <returns>A task representing the operation.</returns>
        protected abstract ValueTask Load();

        /// <summary>
        /// Stores the data into the external storage.
        /// </summary>
        /// <param name="data">The data in the stream.</param>
        /// <returns>A task representing the operation.</returns>
        protected abstract ValueTask Store(ArraySegment<byte> data);

        /// <inheritdoc/>
        public override void Close()
        {
            if(!loaded)
            {
                var valueTask = Store(this.GetData());
                if(valueTask.IsCompletedSuccessfully)
                {
                    return;
                }else if(valueTask.IsFaulted)
                {
                    var task = valueTask.AsTask();
                    if(task.Exception.InnerExceptions.Count == 1)
                    {
                        ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                    }
                    throw task.Exception;
                }else try{
                    valueTask.AsTask().Wait();
                }catch(SynchronizationLockException)
                {
                    
                }catch(PlatformNotSupportedException)
                {
                    
                }
            }

            base.Close();
        }

        private async ValueTask Initialize()
        {
            if(!loaded)
            {
                loaded = true;
                await Load();
                Position = 0;
            }
        }

        private void InitializeSync()
        {
            var valueTask = Initialize();
            if(valueTask.IsCompletedSuccessfully)
            {
                return;
            }else if(valueTask.IsFaulted)
            {
                var task = valueTask.AsTask();
                if(task.Exception.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                }
                throw task.Exception;
            }else{
                valueTask.AsTask().Wait();
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            InitializeSync();
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            InitializeSync();
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Initialize();
            return await base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            InitializeSync();
            return base.ReadByte();
        }
    }
}
