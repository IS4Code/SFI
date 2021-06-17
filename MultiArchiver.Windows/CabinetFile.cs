using IS4.MultiArchiver.Tools.IO;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.Cabinet;

namespace IS4.MultiArchiver.Windows
{
    public sealed class CabinetFile : IDisposable
    {
        readonly Task<ERF> fileTask;

        readonly TaskCompletionSource<bool> readyToOpen = new TaskCompletionSource<bool>();
        BlockingCollection<FileInfo> fileInfoChannel;
        BlockingCollection<bool> fileControlChannel;

        Context context;

        public CabinetFile(Stream stream)
        {
            fileTask = Task.Run(() => {
                return Copy(stream);
            });
            try{
                if(!readyToOpen.Task.Result)
                {
                    throw new ArgumentException(null, nameof(stream));
                }
            }catch(AggregateException e) when(e.InnerExceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }

        ERF Copy(Stream stream)
        {
            using(context = new Context(stream))
            {
                try{
                    var result = FDICopy(context.Handle, "", "", 0, Notify, null, IntPtr.Zero);
                    if(context.Exceptions.Count > 0)
                    {
                        readyToOpen.TrySetException(context.Exceptions);
                    }else{
                        readyToOpen.TrySetResult(result);
                    }
                }catch(Exception e)
                {
                    readyToOpen.TrySetException(e);
                }
                Dispose();
                return context.Error;
            }
        }

        public FileInfo GetNextFile()
        {
            try{
                fileControlChannel.Add(true);
                return fileInfoChannel.Take();
            }catch(OperationCanceledException)
            {
                return null;
            }catch(InvalidOperationException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            fileControlChannel.CompleteAdding();
            fileInfoChannel.CompleteAdding();
        }

        bool NextFileAllowed()
        {
            try{
                return fileControlChannel.Take();
            }catch(OperationCanceledException)
            {
                return false;
            }catch(InvalidOperationException)
            {
                return false;
            }
        }

        IntPtr Notify(FDINOTIFICATIONTYPE fdint, ref FDINOTIFICATION pfdin)
        {
            try{
                switch(fdint)
                {
                    case FDINOTIFICATIONTYPE.fdintCABINET_INFO:
                    {
                        fileInfoChannel = new BlockingCollection<FileInfo>();
                        fileControlChannel = new BlockingCollection<bool>();
                        readyToOpen.TrySetResult(true);
                        return (IntPtr)0;
                    }
                    case FDINOTIFICATIONTYPE.fdintPARTIAL_FILE:
                    {
                        return (IntPtr)0;
                    }
                    case FDINOTIFICATIONTYPE.fdintCOPY_FILE:
                    {
                        if(!NextFileAllowed()) return (IntPtr)(-1);
                        var info = new FileInfo(ref pfdin, out var writer);
                        fileInfoChannel.Add(info);
                        return context.GetNewPointer(writer);
                    }
                    case FDINOTIFICATIONTYPE.fdintCLOSE_FILE_INFO:
                    {
                        var writer = context.GetWriter(pfdin.hf);
                        writer.Complete();
                        return (IntPtr)1;
                    }
                    case FDINOTIFICATIONTYPE.fdintNEXT_CABINET:
                    {
                        return (IntPtr)(-1);
                    }
                    case FDINOTIFICATIONTYPE.fdintENUMERATE:
                    {
                        return (IntPtr)0;
                    }
                    default:
                    {
                        return (IntPtr)(-1);
                    }
                }
            }catch(Exception e)
            {
                context.Exceptions.Add(e);
                return (IntPtr)(-1);
            }
        }

        public class FileInfo
        {
            public string Name { get; }
            public uint Size { get; }
            public DateTime Date { get; }
            public FileAttributes Attributes { get; }
            public Stream Stream { get; }

            internal FileInfo(ref FDINOTIFICATION pfdin, out ChannelWriter<UnmanagedMemoryRange> writer)
            {
                bool utf8 = (pfdin.attribs & 0x80) != 0;
                Name = utf8 ? PtrToStringUtf8((IntPtr)pfdin.psz1) : pfdin.psz1;
                Size = unchecked((uint)pfdin.cb);
                Attributes = (FileAttributes)pfdin.attribs & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive);

                if(Kernel32.DosDateTimeToFileTime(pfdin.date, pfdin.time, out var fileTime))
                {
                    Date = DateTime.FromFileTimeUtc(unchecked((long)(((ulong)unchecked((uint)fileTime.dwHighDateTime) << 32) | unchecked((uint)fileTime.dwLowDateTime))));
                }
                Stream = ChannelMemoryStream.Create(out writer, 1);
            }

            unsafe string PtrToStringUtf8(IntPtr addr)
            {
                byte* b = (byte*)addr;
                while(*b != 0) b++;
                return Encoding.UTF8.GetString((byte*)addr, (int)(b - (byte*)addr));
            }
        }

        class Context : IDisposable
        {
            public ERF Error => (ERF)error;

            public List<Exception> Exceptions { get; } = new List<Exception>();

            public SafeHFDI Handle { get; }

            readonly ObjectMap objectMap = new ObjectMap();

            readonly object error;
            GCHandle errorHandle;

            readonly PFNALLOC alloc;
            readonly PFNFREE free;
            readonly PFNOPEN open;
            readonly PFNREAD read;
            readonly PFNWRITE write;
            readonly PFNCLOSE close;
            readonly PFNSEEK seek;

            public Context(Stream stream)
            {
                alloc = cb => {
                    try{
                        return Marshal.AllocHGlobal(unchecked((int)cb));
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                free = memory => {
                    try{
                        Marshal.FreeHGlobal(memory);
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                    }
                };

                open = (pszFile, oflag, pmode) => {
                    try{
                        if(pszFile != "") return IntPtr.Zero;
                        var subStream = new StreamPosition(stream);
                        return GetNewPointer(subStream);
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                read = (hf, memory, cb) => {
                    try{
                        var buffer = ArrayPool<byte>.Shared.Rent(unchecked((int)cb));
                        try{
                            var subStream = GetStream(hf);
                            var read = subStream.Read(buffer, 0, unchecked((int)cb));
                            Marshal.Copy(buffer, 0, memory, read);
                            return unchecked((uint)read);
                        }finally{
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                write = (hf, memory, cb) => {
                    try{
                        var writer = GetWriter(hf);
                        async Task Inner()
                        {
                            await writer.WriteAsync(new UnmanagedMemoryRange(memory, unchecked((int)cb)));
                            await writer.WriteAsync(default);
                            await writer.WaitToWriteAsync();
                        }
                        Inner().Wait();
                        return cb;
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                close = hf => {
                    try{
                        if(GetTarget(hf, true) is ChannelWriter<UnmanagedMemoryRange> writer)
                        {
                            writer.TryComplete();
                        }
                        return 0;
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                seek = (hf, dist, seektype) => {
                    try{
                        var subStream = GetStream(hf);
                        return (int)subStream.Seek(dist, seektype);
                    }catch(Exception e)
                    {
                        Exceptions.Add(e);
                        return default;
                    }
                };

                error = default(ERF);
                errorHandle = GCHandle.Alloc(error, GCHandleType.Pinned);

                unsafe{
                    Handle = FDICreate(alloc, free, open, read, write, close, seek, FDICPU.cpuUNKNOWN, ref *(ERF*)errorHandle.AddrOfPinnedObject());
                }
            }

            protected void Dispose(bool disposing)
            {
                if(disposing)
                {
                    Handle.Dispose();
                }

                if(errorHandle.IsAllocated)
                {
                    errorHandle.Free();
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~Context()
            {
                Dispose(false);
            }

            public class StreamPosition
            {
                readonly Stream stream;
            
                public long Position { get; private set; }

                public StreamPosition(Stream stream)
                {
                    this.stream = stream;
                }

                public int Read(byte[] buffer, int offset, int count)
                {
                    stream.Position = Position;
                    count = stream.Read(buffer, offset, count);
                    Position += count;
                    return count;
                }

                public long Seek(long offset, SeekOrigin origin)
                {
                    switch(origin)
                    {
                        case SeekOrigin.Begin:
                            return Position = offset;
                        case SeekOrigin.Current:
                            return Position += offset;
                        case SeekOrigin.End:
                            return Position = stream.Length + offset;
                        default:
                            return Position = stream.Seek(offset, origin);
                    }
                }
            }

            public object GetTarget(IntPtr hf, bool remove = false)
            {
                var result = objectMap[hf];
                if(remove) objectMap[hf] = null;
                return result;
            }

            public StreamPosition GetStream(IntPtr hf, bool remove = false)
            {
                return GetTarget(hf, remove) as StreamPosition;
            }

            public ChannelWriter<UnmanagedMemoryRange> GetWriter(IntPtr hf, bool remove = false)
            {
                return GetTarget(hf, remove) as ChannelWriter<UnmanagedMemoryRange>;
            }

            public IntPtr GetNewPointer(object target)
            {
                return objectMap[target];
            }

            class ObjectMap
            {
                readonly Dictionary<object, int> positions = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Default);
                readonly List<object> storage = new List<object>();

                public IntPtr this[object obj] {
                    get {
                        if(!positions.TryGetValue(obj, out var pos))
                        {
                            pos = storage.Count;
                            positions[obj] = pos;
                            storage.Add(obj);
                        }
                        return (IntPtr)pos;
                    }
                }

                public object this[IntPtr id] {
                    get {
                        return storage[(int)id];
                    }
                    set {
                        storage[(int)id] = value;
                    }
                }
            }
        }
    }
}
