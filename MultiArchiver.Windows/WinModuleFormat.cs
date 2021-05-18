using System;
using System.IO;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.DbgHelp;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Formats
{
    public class WinModuleFormat : ModuleFormat<SafeHINSTANCE>
    {
        public WinModuleFormat() : base("PE\0\0", "application/vnd.microsoft.portable-executable", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<SafeHINSTANCE, TResult> resultFactory)
        {
            if(stream is FileStream fileStream)
            {
                using(var inst = LoadLibraryEx(fileStream.Name, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | LoadLibraryExFlags.DONT_RESOLVE_DLL_REFERENCES))
                {
                    if(inst.IsNull) return null;
                    return resultFactory(inst);
                }
            }
            string tmpPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            try{
                using(var inst = LoadLibraryEx(tmpPath, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | LoadLibraryExFlags.DONT_RESOLVE_DLL_REFERENCES))
                {
                    if(inst.IsNull) return null;
                    return resultFactory(inst);
                }
            }finally{
                File.Delete(tmpPath);
            }
        }

        public override unsafe string GetExtension(SafeHINSTANCE value)
        {
            var dosHeader = (IntPtr)((long)value.DangerousGetHandle() & ~3);
            var ntHeader = (IMAGE_NT_HEADERS*)((byte*)dosHeader + Marshal.ReadInt32(dosHeader, 60));
            if(ntHeader->Signature == 0x4550)
            {
                if(ntHeader->OptionalHeader.Subsystem == IMAGE_SUBSYSTEM.IMAGE_SUBSYSTEM_NATIVE)
                {
                    return "sys";
                }
                if(ntHeader->OptionalHeader.AddressOfEntryPoint != 0)
                {
                    return "exe";
                }
                /*exe dll sys drv ocx cpl scr mui*/
                return "dll";
            }
            return null;
        }

        [DllImport(Lib.Kernel32, SetLastError = true, ExactSpelling = true, EntryPoint = "K32GetModuleInformation")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetModuleInformation(HPROCESS hProcess, HINSTANCE hModule, out MODULEINFO lpmodinfo, uint cb);
    }
}
