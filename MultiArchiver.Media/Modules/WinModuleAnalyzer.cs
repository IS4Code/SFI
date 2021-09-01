using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Media.Modules;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tags;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : MediaObjectAnalyzer<IModule>
    {
        public WinModuleAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        public override AnalysisResult Analyze(IModule module, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);
            var cache = new Dictionary<(object, object), ResourceInfo>();
            var groups = new List<ResourceInfo>();

            string label = null;

            foreach(var resourceGroup in module.ReadResources().GroupBy(r => $"{r.Type}/{r.Name}"))
            {
                int ordinal = 0;
                bool more = resourceGroup.Skip(1).Any();
                foreach(var resource in resourceGroup)
                {
                    var info = new ResourceInfo(resource, more ? ordinal : (int?)null);
                    switch(info.TypeCode)
                    {
                        case Win32ResourceType.GroupIcon:
                        case Win32ResourceType.GroupCursor:
                            groups.Add(info);
                            continue;
                        case Win32ResourceType.Accelerator:
                        case Win32ResourceType.Dialog:
                        case Win32ResourceType.DialogEx:
                        case Win32ResourceType.DialogInclude:
                        case Win32ResourceType.FontDirectory:
                        case Win32ResourceType.Menu:
                        case Win32ResourceType.MenuEx:
                        case Win32ResourceType.MessageTable:
                        case Win32ResourceType.NameTable:
                        case Win32ResourceType.PlugAndPlay:
                        case Win32ResourceType.String:
                        case Win32ResourceType.VXD:
                            continue;
                        case Win32ResourceType.Version:
                            var versionInfo = new WinVersionInfo(info.Data);
                            label = analyzers.Analyze(versionInfo, context.WithNode(node)).Label;
                            continue;
                    }
                    if(info.Type.Equals("MUI"))
                    {
                        continue;
                    }
                    cache[(resource.Type, resource.Name)] = info;
                    var infoNode = analyzers.Analyze<IFileInfo>(info, context.WithParent(node[info.Type])).Node;
                    if(infoNode != null)
                    {
                        node.Set(Properties.HasMediaStream, infoNode);
                    }
                    ordinal++;
                }
            }
            foreach(var info in groups)
            {
                info.MakeGroup(cache);
                var infoNode = analyzers.Analyze<IFileInfo>(info, context.WithParent(node[info.Type])).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.HasMediaStream, infoNode);
                }
            }
            return new AnalysisResult(node, label);
        }

        class ResourceInfo : IFileInfo, IImageResourceTag
        {
            readonly IModuleResource resource;
            readonly int? ordinal;

            public ArraySegment<byte> Data { get; private set; }

            public Win32ResourceType TypeCode { get; }

            public int CursorHotspot { get; }

            public int OriginalHeight { get; }

            public bool IsTransparent { get; }

            public ResourceInfo(IModuleResource resource, int? ordinal)
            {
                this.resource = resource;
                this.ordinal = ordinal;
                TypeCode = resource.Type as Win32ResourceType? ?? 0;

                int start = 0;
                if(TypeCode == Win32ResourceType.Bitmap || TypeCode == Win32ResourceType.Icon)
                {
                    start = 14;
                }
                if(TypeCode == Win32ResourceType.Cursor)
                {
                    start = 10;
                }
                var buffer = new byte[start + resource.Length];
                int read = resource.Read(buffer, start, resource.Length);

                if(TypeCode == Win32ResourceType.Cursor)
                {
                    start += 4;
                    read -= 4;
                }

                void InvalidBitmap()
                {
                    Data = buffer.Slice(start, read);
                }

                Data = buffer.Slice(0, start + read);
                if(TypeCode == Win32ResourceType.Bitmap || TypeCode == Win32ResourceType.Icon || TypeCode == Win32ResourceType.Cursor)
                {
                    var header = buffer.AsSpan(0, start);
                    header.MemoryCast<short>()[0] = 0x4D42;
                    var fields = header.Slice(2).MemoryCast<int>();
                    fields[0] = Data.Count;

                    int headerLength = BitConverter.ToInt32(buffer, start);

                    if(headerLength < 0)
                    {
                        InvalidBitmap();
                        return;
                    }

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

                    if(numColors < 0)
                    {
                        InvalidBitmap();
                        return;
                    }

                    dataStart += numColors * sizeof(int);

                    if(dataStart < start || dataStart >= resource.Length)
                    {
                        InvalidBitmap();
                        return;
                    }

                    if(TypeCode == Win32ResourceType.Icon || TypeCode == Win32ResourceType.Cursor)
                    {
                        var bmpHeader = buffer.AsSpan(start, headerLength).MemoryCast<int>();
                        if(bmpHeader.Length <= 2)
                        {
                            InvalidBitmap();
                            return;
                        }
                        OriginalHeight = bmpHeader[2];
                        bmpHeader[2] /= 2;

                        IsTransparent = true;
                    }

                    if(TypeCode == Win32ResourceType.Cursor)
                    {
                        CursorHotspot = fields[2];
                    }

                    fields[2] = dataStart;
                }
            }

            public void MakeGroup(IReadOnlyDictionary<(object, object), ResourceInfo> cache)
            {
                var data = Data.AsSpan();
                var header = data.MemoryCast<short>();
                var type = header[1];
                var count = header[2];
                object typeCode;
                switch(type)
                {
                    case 1:
                        typeCode = Win32ResourceType.Icon;
                        break;
                    case 2:
                        typeCode = Win32ResourceType.Cursor;
                        break;
                    default:
                        return;
                }
                using(var buffer = new MemoryStream())
                {
                    int dataOffset = 6 + count * 16;
                    var resources = new List<ResourceInfo>();
                    var writer = new BinaryWriter(buffer);
                    buffer.Write(Data.Array, Data.Offset, 6);
                    for(int i = 0; i < count; i++)
                    {
                        var info = Data.Slice(6 + i * 14, 14);
                        var span = info.AsSpan().MemoryCast<short>();
                        int id = span[6];
                        buffer.Write(info.Array, info.Offset, info.Count - 2);

                        if(!cache.TryGetValue((typeCode, id), out var res))
                        {
                            return;
                        }
                        resources.Add(res);

                        writer.Write(dataOffset);

                        dataOffset += res.Data.Count - 14;
                    }
                    foreach(var res in resources)
                    {
                        buffer.Write(res.Data.Array, res.Data.Offset + 14, 8);
                        writer.Write(res.OriginalHeight);
                        buffer.Write(res.Data.Array, res.Data.Offset + 26, res.Data.Count - 26);
                    }
                    Data = buffer.GetData();
                }
            }

            public long Length => resource.Length;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => resource;

            public object DataKey => null;

            public Stream Open()
            {
                return Data.AsStream(false);
            }

            public bool IsEncrypted => false;

            public string Name => resource.Name.ToString();

            public string SubName => ordinal?.ToString();

            public string Type => resource.Type.ToString();

            public string Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.Embedded;

            public override string ToString()
            {
                return $"/{Type}/{Name}" + (ordinal != null ? $":{ordinal}" : "");
            }
        }
    }
}
