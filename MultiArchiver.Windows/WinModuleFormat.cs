using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Formats
{
    public class WinModuleFormat : SignatureFormat<SafeHINSTANCE>
    {
        public WinModuleFormat() : base(3, "MZ", "application/vnd.microsoft.portable-executable", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<SafeHINSTANCE, TResult> resultFactory)
        {
            string tmpPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            try{
                using(var inst = LoadLibraryEx(tmpPath, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE))
                {
                    if(inst.IsNull) return null;
                    return resultFactory(inst);
                }
            }finally{
                File.Delete(tmpPath);
            }
        }

        public override string GetExtension(SafeHINSTANCE value)
        {
            if(!GetModuleInformation(GetCurrentProcess(), value, out var info, unchecked((uint)Marshal.SizeOf<MODULEINFO>())))
            {
                return null;
            }
            return info.EntryPoint == IntPtr.Zero ? "dll" : "exe";
        }

        [DllImport(Lib.Kernel32, SetLastError = true, ExactSpelling = true, EntryPoint = "K32GetModuleInformation")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetModuleInformation(HPROCESS hProcess, HINSTANCE hModule, out MODULEINFO lpmodinfo, uint cb);
    }
}
