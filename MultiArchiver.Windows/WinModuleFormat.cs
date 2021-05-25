using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.DbgHelp;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Formats
{
    public class WinModuleFormat : ModuleFormat
    {
        public WinModuleFormat() : base("PE\0\0", "application/vnd.microsoft.portable-executable", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<IModule, TResult> resultFactory)
        {
            if(stream is FileStream fileStream)
            {
                using(var inst = LoadLibraryEx(fileStream.Name, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | LoadLibraryExFlags.DONT_RESOLVE_DLL_REFERENCES))
                {
                    if(inst.IsNull) return null;
                    return resultFactory(new Module(inst));
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
                    return resultFactory(new Module(inst));
                }
            }finally{
                File.Delete(tmpPath);
            }
        }

        class Module : IModule
        {
            readonly SafeHINSTANCE module;

            public Module(SafeHINSTANCE value)
            {
                this.module = value;
            }

            public unsafe ModuleType Type {
                get {
                    var dosHeader = (IntPtr)((long)module.DangerousGetHandle() & ~3);
                    var ntHeader = (IMAGE_NT_HEADERS*)((byte*)dosHeader + Marshal.ReadInt32(dosHeader, 60));
                    if(ntHeader->Signature == 0x4550)
                    {
                        if(ntHeader->OptionalHeader.Subsystem == IMAGE_SUBSYSTEM.IMAGE_SUBSYSTEM_NATIVE)
                        {
                            return ModuleType.System;
                        }
                        if(ntHeader->OptionalHeader.AddressOfEntryPoint != 0)
                        {
                            return ModuleType.Executable;
                        }
                        /*exe dll sys drv ocx cpl scr mui*/
                        return ModuleType.Library;
                    }
                    return ModuleType.Unknown;
                }
            }

            [DllImport(Lib.Kernel32, SetLastError = true, ExactSpelling = true, EntryPoint = "K32GetModuleInformation")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetModuleInformation(HPROCESS hProcess, HINSTANCE hModule, out MODULEINFO lpmodinfo, uint cb);

            public IEnumerable<IModuleResource> ReadResources()
            {
                var resources = new List<ResourceInfo>();
                EnumResourceTypesEx(module, (mod, type, par) => {
                    var safeType = new SafeResourceId(type.DangerousGetHandle());
                    EnumResourceNamesEx(mod, safeType, (mod2, type2, name, par2) => {
                        var safeName = new SafeResourceId(name.DangerousGetHandle());
                        var res = FindResource(mod2, safeName, safeType);
                        if(!res.IsNull)
                        {
                            resources.Add(new ResourceInfo(module, safeType, safeName, res));
                        }
                        return true;
                    }, IntPtr.Zero, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN, 0);
                    return true;
                }, IntPtr.Zero, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN, 0);
                return resources;
            }

            class ResourceInfo : IModuleResource
            {
                readonly SafeHINSTANCE module;
                readonly SafeResourceId type;
                readonly SafeResourceId name;
                readonly HRSRC res;

                public ResourceInfo(SafeHINSTANCE module, SafeResourceId type, SafeResourceId name, HRSRC res)
                {
                    this.module = module;
                    this.type = type;
                    this.name = name;
                    this.res = res;
                }

                public int Length => unchecked((int)SizeofResource(module, res));

                public object Type {
                    get {
                        return type.IsIntResource ? (Win32ResourceType)type.id : (object)type.ToString();
                    }
                }

                public object Name {
                    get {
                        return name.IsIntResource ? name.id : (object)name.ToString();
                    }
                }

                public int Read(byte[] buffer, int offset, int length)
                {
                    length = Math.Min(length, Length);
                    var hResData = LoadResource(module, res);
                    var pResData = LockResource(hResData);
                    Marshal.Copy(pResData, buffer, offset, length);
                    return length;
                }
            }
        }
    }
}
