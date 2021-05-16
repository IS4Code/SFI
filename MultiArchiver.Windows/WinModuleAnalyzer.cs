using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : BinaryFormatAnalyzer<SafeHINSTANCE>
    {
        public override string Analyze(ILinkedNode node, SafeHINSTANCE module, ILinkedNodeFactory nodeFactory)
        {
            EnumResourceTypesEx(module, (mod, type, par) => {
                using(var safeType = new SafeResourceId(type.DangerousGetHandle()))
                {
                    EnumResourceNamesEx(mod, safeType, (mod2, type2, name, par2) => {
                        using(var safeName = new SafeResourceId(name.DangerousGetHandle()))
                        {
                            var res = FindResource(module, safeName, safeType);
                            if(!res.IsNull)
                            {
                                var info = new ResourceInfo(module, safeType, safeName, res);
                                var infoNode = nodeFactory.Create<IFileInfo>(node[info.Type], info);
                                if(infoNode != null)
                                {
                                    infoNode.SetClass(Classes.EmbeddedFileDataObject);
                                    node.Set(Properties.HasMediaStream, infoNode);
                                }
                            }
                        }
                        return true;
                    }, IntPtr.Zero, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN, 0);
                }
                return true;
            }, IntPtr.Zero, RESOURCE_ENUM_FLAGS.RESOURCE_ENUM_LN, 0);
            return null;
        }

        /*ILinkedNode Analyze(ILinkedNode parent, ResourceInfo info, ILinkedNodeFactory nodeFactory)
        {
            var node = parent[info.Identifier];
            if(node != null)
            {
                var content = nodeFactory.Create<IStreamFactory>(node, info);
                if(content != null)
                {
                    content.Set(Properties.IsStoredAs, node);
                }
            }
            return node;
        }*/

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
                Accelerator = 9,
                AnimatedCursor = 21,
                AnimatedIcon = 22,
                Bitmap = 2,
                Cursor = 1,
                Dialog = 5,
                Font = 8,
                FontDir = 7,
                GroupCursor = 12,
                GroupIcon = 14,
                Icon = 3,
                Html = 23,
                Menu = 4,
                Manifest = 24,
                MessageTable = 11,
                UserData = 10,
                String = 6,
                Version = 16,
                PlugAndPlay = 19,
            }
        }
    }
}
