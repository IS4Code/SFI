using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : BinaryFormatAnalyzer<SafeHINSTANCE>
    {
        public override string Analyze(ILinkedNode node, SafeHINSTANCE module, ILinkedNodeFactory nodeFactory)
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

            foreach(var info in resources)
            {
                var infoNode = nodeFactory.Create<IFileInfo>(node[info.Type], info);
                if(infoNode != null)
                {
                    infoNode.SetClass(Classes.EmbeddedFileDataObject);
                    node.Set(Properties.HasMediaStream, infoNode);
                }
            }

            return null;
        }

        class ResourceInfo : IFileInfo
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

            public long Length => SizeofResource(module, res);

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => module;

            public object DataKey => res.DangerousGetHandle();

            public unsafe Stream Open()
            {
                var len = Length;
                var hResData = LoadResource(module, res);
                var pResData = LockResource(hResData);
                return new UnmanagedMemoryStream((byte*)pResData, len, len, System.IO.FileAccess.Read);
            }

            public string Type {
                get {
                    return type.IsIntResource ? ((Win32ResourceType)type.id).ToString() : type.ToString();
                }
            }

            public bool IsEncrypted => false;

            public string Name => name.IsIntResource ? name.id.ToString() : name.ToString();

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public override string ToString()
            {
                return Type + "/" + Name;
            }

            enum Win32ResourceType : ushort
            {
                Cursor = 1,
                Bitmap = 2,
                Icon = 3,
                Menu = 4,
                Dialog = 5,
                String = 6,
                FontDirectory = 7,
                Font = 8,
                Accelerator = 9,
                UserData = 10,
                MessageTable = 11,
                GroupCursor = 12,
                MenuEx = 13,
                GroupIcon = 14,
                NameTable = 15,
                Version = 16,
                DialogInclude = 17,
                DialogEx = 18,
                PlugAndPlay = 19,
                VXD = 20,
                AnimatedCursor = 21,
                AnimatedIcon = 22,
                Html = 23,
                Manifest = 24,
            }
        }
    }
}
