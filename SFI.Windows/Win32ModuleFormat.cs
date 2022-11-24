using IS4.SFI.Formats.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.DbgHelp;
using static Vanara.PInvoke.Kernel32;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Windows PE module format, loaded using P/Invoke.
    /// </summary>
    public class Win32ModuleFormat : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Win32ModuleFormat() : base("PE\0\0", "application/vnd.microsoft.portable-executable", null)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            if(stream is FileStream fileStream)
            {
                using var inst = LoadLibraryEx(fileStream.Name, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | LoadLibraryExFlags.DONT_RESOLVE_DLL_REFERENCES);
                if(inst.IsNull) return default;
                return await resultFactory(new Module(inst), args);
            }
            using var tmpPath = FileTools.GetTemporaryFile("mz");
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            using(var inst = LoadLibraryEx(tmpPath, LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE | LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | LoadLibraryExFlags.DONT_RESOLVE_DLL_REFERENCES))
            {
                if(inst.IsNull) return default;
                return await resultFactory(new Module(inst), args);
            }
        }

        class Module : IModule
        {
            readonly SafeHINSTANCE module;

            IModuleSignature? IModule.Signature => null;

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
