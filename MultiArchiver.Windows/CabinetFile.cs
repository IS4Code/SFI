using IS4.MultiArchiver.Tools;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        public CabinetFile(Stream stream)
        {
            fileTask = Task.Run(() => {
                cabinetStream = stream;
                cabinetError = default;
                try
                {
                    var result = FDICopy(threadContext.Value, "", "", 0, Notify, IntPtr.Zero, IntPtr.Zero);
                    readyToOpen.TrySetResult(result);
                    Dispose();
                    cabinetStream = null;
                    return cabinetError;
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                    return default;
                }
            });
            if(!readyToOpen.Task.Result)
            {
                throw new ArgumentException(null, nameof(stream));
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

        IntPtr Notify(int fdint, ref FDINOTIFICATION pfdin)
        {
            switch(fdint)
            {
                case 0:
                {
                    fileInfoChannel = new BlockingCollection<FileInfo>();
                    fileControlChannel = new BlockingCollection<bool>();
                    readyToOpen.TrySetResult(true);
                    return (IntPtr)0;
                }
                case 1:
                {
                    return (IntPtr)0;
                }
                case 2:
                {
                    if(!NextFileAllowed()) return (IntPtr)(-1);
                    var info = new FileInfo(ref pfdin, out var writer);
                    fileInfoChannel.Add(info);
                    return GetNewPointer(writer, out _);
                }
                case 3:
                {
                    var writer = GetWriter(pfdin.hf, out var handle);
                    handle.Free();
                    writer.Complete();
                    return (IntPtr)1;
                }
                case 4:
                {
                    var writer = GetWriter(pfdin.psz2, out var handle);
                    handle.Free();
                    writer.Complete();
                    return (IntPtr)(-1);
                }
                case 5:
                {
                    return (IntPtr)0;
                }
                default:
                {
                    return (IntPtr)(-1);
                }
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
                Name = utf8 ? PtrToStringUtf8(pfdin.psz1) : Marshal.PtrToStringAnsi(pfdin.psz1);
                Size = pfdin.cb;
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
        internal struct FDINOTIFICATION
        {
            public uint cb;
            public IntPtr psz1;
            public IntPtr psz2;
            public IntPtr psz3;
            public IntPtr pv;
            public HFILE hf;
            public ushort date;
            public ushort time;
            public ushort attribs;
            public ushort setID;
            public ushort iCabinet;
            public ushort iFolder;
            public FDIERROR fdie;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr PFNFDINOTIFY(int fdint, ref FDINOTIFICATION pfdin);

        [DllImport("cabinet.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ExactSpelling = true, CharSet = CharSet.Ansi)]
        static extern bool FDICopy(HFDI hfdi, string pszCabinet, string pszCabPath, int flags, PFNFDINOTIFY pfnfdin, IntPtr pfnfdid, IntPtr pvUser);

        [DllImport("cabinet.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ExactSpelling = true, CharSet = CharSet.Ansi)]
        unsafe static extern SafeHFDI FDICreate(PFNALLOC pfnalloc, PFNFREE pfnfree, PFNOPEN pfnopen, PFNREAD pfnread, PFNWRITE pfnwrite, PFNCLOSE pfnclose, PFNSEEK pfnseek, int cpuType, out ERF perf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate IntPtr PFNALLOC(uint cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate void PFNFREE(IntPtr memory);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate HFILE PFNOPEN(string pszFile, int oflag, int pmode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate uint PFNREAD(HFILE hf, IntPtr memory, uint cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate int PFNSEEK(HFILE hf, int dist, int seektype);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        delegate uint PFNWRITE(HFILE hf, IntPtr memory, uint cb);

        [StructLayout(LayoutKind.Sequential)]
        struct ERF
        {
            public int erfOper;
            public int erfType;
            public int fError;
        }

        [ThreadStatic]
        static ERF cabinetError;

        [ThreadStatic]
        static Stream cabinetStream;

        static readonly ThreadLocal<SafeHFDI> threadContext = new ThreadLocal<SafeHFDI>(
            () => FDICreate(
                cb => Marshal.AllocHGlobal(unchecked((int)cb)),
                memory => Marshal.FreeHGlobal(memory),
                (pszFile, oflag, pmode) => {
                    if(pszFile != "") return new HFILE(IntPtr.Zero);
                    var stream = new StreamPosition(cabinetStream);
                    return new HFILE(GetNewPointer(stream, out _));
                },
                (hf, memory, cb) => {
                    var buffer = ArrayPool<byte>.Shared.Rent(unchecked((int)cb));
                    try{
                        var stream = GetStream(hf, out _);
                        var read = stream.Read(buffer, 0, unchecked((int)cb));
                        Marshal.Copy(buffer, 0, memory, read);
                        return unchecked((uint)read);
                    }finally{
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                },
                (hf, memory, cb) => {
                    var writer = GetWriter(hf, out _);
                    async Task Inner()
                    {
                        await writer.WriteAsync(new UnmanagedMemoryRange(memory, unchecked((int)cb)));
                        await writer.WriteAsync(default);
                        await writer.WaitToWriteAsync();
                    }
                    Inner().Wait();
                    return cb;
                },
                hf => {
                    GetTarget(hf, out var handle);
                    handle.Free();
                    return 0;
                },
                (hf, dist, seektype) => {
                    var stream = GetStream(hf, out _);
                    return (int)stream.Seek(dist, (SeekOrigin)seektype);
                },
                -1,
                out cabinetError
            )
        );

        class StreamPosition
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

        static object GetTarget(HFILE hf, out GCHandle handle)
        {
            handle = (GCHandle)hf.DangerousGetHandle();
            return handle.Target;
        }

        static StreamPosition GetStream(HFILE hf, out GCHandle handle)
        {
            return GetTarget(hf, out handle) as StreamPosition;
        }

        static ChannelWriter<UnmanagedMemoryRange> GetWriter(HFILE hf, out GCHandle handle)
        {
            return GetTarget(hf, out handle) as ChannelWriter<UnmanagedMemoryRange>;
        }

        static IntPtr GetNewPointer(object target, out GCHandle handle)
        {
            handle = GCHandle.Alloc(target);
            return (IntPtr)handle;
        }
    }
}
