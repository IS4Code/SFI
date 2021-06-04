using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using IS4.MultiArchiver.Windows;
using static Vanara.PInvoke.VersionDll;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinModuleAnalyzer : BinaryFormatAnalyzer<IModule>
    {
        public WinModuleAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        public override string Analyze(ILinkedNode node, IModule module, ILinkedNodeFactory nodeFactory)
        {
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
                        case Win32ResourceType.Font:
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
                            label = ReadVersion(node, info);
                            continue;
                    }
                    if(info.Type.Equals("MUI"))
                    {
                        continue;
                    }
                    cache[(resource.Type, resource.Name)] = info;
                    var infoNode = nodeFactory.Create<IFileInfo>(node[info.Type], info);
                    if(infoNode != null)
                    {
                        infoNode.SetClass(Classes.EmbeddedFileDataObject);
                        node.Set(Properties.HasMediaStream, infoNode);
                    }
                    ordinal++;
                }
            }
            foreach(var info in groups)
            {
                info.MakeGroup(cache);
                var infoNode = nodeFactory.Create<IFileInfo>(node[info.Type], info);
                if(infoNode != null)
                {
                    infoNode.SetClass(Classes.EmbeddedFileDataObject);
                    node.Set(Properties.HasMediaStream, infoNode);
                }
            }
            return label;
        }

        static readonly byte[] ansiVersionString = Encoding.ASCII.GetBytes("VS_VERSION_INFO");
        static readonly byte[] unicodeVersionString = Encoding.Unicode.GetBytes("\0VS_VERSION_INFO");

        unsafe string ReadVersion(ILinkedNode node, ResourceInfo info)
        {
            string label = null;
            using(var stream = info.Open())
            {
                // Allocate space for ANSI/Unicode conversions
                var buffer = new byte[stream.Length * 3];
                int bufferLen = buffer.Length / 3;
                stream.Read(buffer, bufferLen, bufferLen);

                var bufferData = buffer.AsSpan().Slice(bufferLen, bufferLen);

                var signature = bufferData.Slice(4);

                bool useAnsi;
                if(signature.StartsWith(ansiVersionString))
                {
                    useAnsi = true;
                }else if(signature.StartsWith(unicodeVersionString))
                {
                    useAnsi = false;
                }else{
                    return null;
                }

                fixed(byte* data = bufferData)
                {
                    if(VerQueryValue(useAnsi, (IntPtr)data, bufferLen, @"\", out var rootVal, out var rootLen))
                    {
                        ref var version = ref *(VS_FIXEDFILEINFO*)rootVal;
                        if(version.dwSignature == 0xFEEF04BD)
                        {
                            var fileVersion = VerFromDWORDs(version.dwFileVersionMS, version.dwFileVersionLS);
                            var productVersion = VerFromDWORDs(version.dwProductVersionMS, version.dwProductVersionLS);

                            node.Set(Properties.Version, fileVersion);
                            node.Set(Properties.SoftwareVersion, productVersion);
                        }
                    }

                    if(VerQueryValue(useAnsi, (IntPtr)data, bufferLen, @"\VarFileInfo\Translation", out var transBlock, out var transLen))
                    {
                        var num = transLen / (uint)sizeof(LANGANDCODEPAGE);
                        var translations = new Span<LANGANDCODEPAGE>((void*)transBlock, unchecked((int)num));
                        foreach(var trans in translations)
                        {
                            var enc = useAnsi ? Encoding.GetEncoding(trans.wCodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback) : Encoding.Unicode;
                            string lang;
                            if(trans.wLanguage > 0)
                            {
                                lang = CultureInfo.GetCultureInfo(trans.wLanguage).IetfLanguageTag;
                            }else{
                                lang = null;
                            }
                            var dir = $@"\StringFileInfo\{trans.wLanguage:X4}{trans.wCodePage:X4}\";
                            foreach(var pair in predefinedProperties)
                            {
                                if(VerQueryValue(useAnsi, (IntPtr)data, bufferLen, dir + pair.Key, out var text, out var textLen))
                                {
                                    var len = enc.GetMaxByteCount(unchecked((int)textLen));
                                    var value = enc.GetString((byte*)text, len).Substring(0, unchecked((int)textLen));
                                    int end = value.IndexOf('\0');
                                    if(end != -1) value = value.Substring(0, end);
                                    if(lang != null && pair.Value.lang)
                                    {
                                        node.Set(pair.Value.prop, value, lang);
                                    }else{
                                        node.Set(pair.Value.prop, value);
                                    }
                                    if(pair.Value.prop == Properties.Name)
                                    {
                                        label = value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return label;
        }

        static unsafe bool VerQueryValue(bool ansi, IntPtr pBlock, int length, string lpSubBlock, out IntPtr lplpBuffer, out uint puLen)
        {
            bool result;
            if(ansi) result = VerQueryValueA(pBlock, lpSubBlock, out lplpBuffer, out puLen);
            else result = Vanara.PInvoke.VersionDll.VerQueryValue(pBlock, lpSubBlock, out lplpBuffer, out puLen);
            if(result && ((byte*)lplpBuffer < (byte*)pBlock || (byte*)lplpBuffer >= (byte*)pBlock + length))
            {
                throw new InvalidOperationException("Version block encoding was not correctly detected.");
            }
            return result;
        }

        [DllImport("Version.dll", SetLastError=false, CharSet=CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool VerQueryValueA(IntPtr pBlock, [MarshalAs(UnmanagedType.LPStr)] string lpSubBlock, out IntPtr lplpBuffer, out uint puLen);

        [StructLayout(LayoutKind.Sequential)]
        struct LANGANDCODEPAGE
        {
            public readonly ushort wLanguage;
            public readonly ushort wCodePage;
        }

        static string VerFromDWORDs(uint ms, uint ls)
        {
            return $"{(ms >> 16) & 0xffff}.{(ms >> 0) & 0xffff}.{(ls >> 16) & 0xffff}.{(ls >> 0) & 0xffff}";
        }

        static readonly Dictionary<string, (Properties prop, bool lang)> predefinedProperties = new Dictionary<string, (Properties, bool)>
        {
            //{ "Comments", Properties. },
            { "InternalName", (Properties.Name, false) },
            //{ "ProductName", Properties. },
            { "CompanyName", (Properties.Creator, true) },
            { "LegalCopyright", (Properties.CopyrightNotice, true) },
            { "ProductVersion", (Properties.SoftwareVersion, false) },
            { "FileDescription", (Properties.Description, true) },
            //{ "LegalTrademarks", Properties. },
            //{ "PrivateBuild", Properties. },
            { "FileVersion", (Properties.Version, false) },
            { "OriginalFilename", (Properties.OriginalName, false) },
            //{ "SpecialBuild", Properties. },
        };

        class ResourceInfo : IFileInfo
        {
            readonly IModuleResource resource;
            readonly int? ordinal;

            public ArraySegment<byte> Data { get; private set; }

            public Win32ResourceType TypeCode { get; }

            public int CursorHotspot { get; }

            public int OriginalHeight { get; }

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

                Data = new ArraySegment<byte>(buffer, 0, start + read);
                if(TypeCode == Win32ResourceType.Bitmap || TypeCode == Win32ResourceType.Icon || TypeCode == Win32ResourceType.Cursor)
                {
                    var header = new Span<byte>(buffer, 0, start);
                    MemoryMarshal.Cast<byte, short>(header)[0] = 0x4D42;
                    var fields = MemoryMarshal.Cast<byte, int>(header.Slice(2));
                    fields[0] = Data.Count;

                    int headerLength = BitConverter.ToInt32(buffer, start);

                    if(headerLength < 0)
                    {
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
                        return;
                    }

                    dataStart += numColors * sizeof(int);

                    if(dataStart >= resource.Length)
                    {
                        return;
                    }

                    if(TypeCode == Win32ResourceType.Icon || TypeCode == Win32ResourceType.Cursor)
                    {
                        var bmpHeader = MemoryMarshal.Cast<byte, int>(new Span<byte>(buffer, start, headerLength));
                        OriginalHeight = bmpHeader[2];
                        bmpHeader[2] /= 2;
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
                var data = new Span<byte>(Data.Array, Data.Offset, Data.Count);
                var header = MemoryMarshal.Cast<byte, short>(data);
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
                        var info = new ArraySegment<byte>(Data.Array, Data.Offset + 6 + i * 14, 14);
                        var span = MemoryMarshal.Cast<byte, short>(new Span<byte>(info.Array, info.Offset, info.Count));
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
                    if(!buffer.TryGetBuffer(out var seg))
                    {
                        seg = new ArraySegment<byte>(buffer.ToArray());
                    }
                    Data = seg;
                }
            }

            public long Length => resource.Length;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object ReferenceKey => resource;

            public object DataKey => null;

            public Stream Open()
            {
                return new MemoryStream(Data.Array, Data.Offset, Data.Count, false);
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

            public override string ToString()
            {
                return $"/{Type}/{Name}" + (ordinal != null ? $":{ordinal}" : "");
            }
        }
    }
}
