using System;
using System.IO;
using System.Runtime.InteropServices;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using IS4.MultiArchiver.Windows;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : BinaryFormatAnalyzer<IModule>
    {
        public override string Analyze(ILinkedNode node, IModule module, ILinkedNodeFactory nodeFactory)
        {
            foreach(var resource in module.ReadResources())
            {
                var info = new ResourceInfo(resource);
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
            readonly IModuleResource resource;
            readonly Lazy<ArraySegment<byte>> data;

            public ResourceInfo(IModuleResource resource)
            {
                this.resource = resource;
                data = new Lazy<ArraySegment<byte>>(() => {
                    int start = 0;
                    var typeCode = resource.Type as Win32ResourceType? ?? 0;
                    if(typeCode == Win32ResourceType.Bitmap || typeCode == Win32ResourceType.Icon || typeCode == Win32ResourceType.Cursor)
                    {
                        start = 14;
                    }
                    var buffer = new byte[start + resource.Length];
                    int read = resource.Read(buffer, start, resource.Length);

                    var segment = new ArraySegment<byte>(buffer, 0, start + read);
                    if(typeCode == Win32ResourceType.Bitmap || typeCode == Win32ResourceType.Icon || typeCode == Win32ResourceType.Cursor)
                    {
                        var header = new Span<byte>(buffer, 0, start);
                        MemoryMarshal.Cast<byte, short>(header)[0] = 0x4D42;
                        var fields = MemoryMarshal.Cast<byte, int>(header.Slice(2));
                        fields[0] = segment.Count;

                        int headerLength = BitConverter.ToInt32(buffer, start);

                        int dataStart = start + headerLength;

                        int numColors = BitConverter.ToInt32(buffer, start + 32);
                        if(numColors == 0)
                        {
                            var bits = BitConverter.ToInt16(buffer, start + 14);
                            if(bits < 16)
                            {
                                numColors = 1 << bits;
                            }
                        }

                        dataStart += numColors * sizeof(int);

                        if(typeCode == Win32ResourceType.Icon || typeCode == Win32ResourceType.Cursor)
                        {
                            var bmpHeader = MemoryMarshal.Cast<byte, int>(new Span<byte>(buffer, start, headerLength));
                            bmpHeader[2] /= 2;
                        }

                        fields[2] = dataStart;
                    }
                    return segment;
                });
            }

            public long Length => resource.Length;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => resource;

            public object DataKey => null;

            public unsafe Stream Open()
            {
                var seg = data.Value;
                return new MemoryStream(seg.Array, seg.Offset, seg.Count, false);
            }

            public bool IsEncrypted => false;

            public string Name => resource.Name.ToString();

            public string Type => resource.Type.ToString();

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public override string ToString()
            {
                return Type + "/" + Name;
            }
        }
    }
}
