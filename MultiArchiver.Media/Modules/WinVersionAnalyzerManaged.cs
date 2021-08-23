using IS4.MultiArchiver.Media.Modules;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinVersionAnalyzerManaged : EntityAnalyzer<WinVersionInfo>
    {
        public override AnalysisResult Analyze(WinVersionInfo entity, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);
            return new AnalysisResult(node, ReadVersion(node, entity.Data));
        }

        static readonly (string key, PropertyUri propUri, bool useLang)[] predefinedProperties = new[]
        {
            //( "Comments", Properties. ),
            ( "InternalName", Properties.Name, false),
            //( "ProductName", Properties. ),
            ( "CompanyName", Properties.Creator, true),
            ( "LegalCopyright", Properties.CopyrightNotice, true),
            ( "ProductVersion", Properties.SoftwareVersion, false),
            ( "FileDescription", Properties.Description, true),
            //( "LegalTrademarks", Properties. ),
            //( "PrivateBuild", Properties. ),
            ( "FileVersion", Properties.Version, false),
            ( "OriginalFilename", Properties.OriginalName, false),
            //( "SpecialBuild", Properties. ),
        };

        string ReadVersion(ILinkedNode node, ArraySegment<byte> versionData)
        {
            string label = null;
            var info = new EntryInfo(versionData);

            if(info.Name == "VS_VERSION_INFO")
            {
                ref var version = ref MemoryMarshal.Cast<byte, VS_FIXEDFILEINFO>(info.Value.AsSpan())[0];
                if(version.dwSignature == 0xFEEF04BD)
                {
                    var fileVersion = VerFromDWORDs(version.dwFileVersionMS, version.dwFileVersionLS);
                    var productVersion = VerFromDWORDs(version.dwProductVersionMS, version.dwProductVersionLS);

                    node.Set(Properties.Version, fileVersion);
                    node.Set(Properties.SoftwareVersion, productVersion);
                }
            }

            var children = info.Children;
            if( children.TryGetValue("StringFileInfo", out var stringInfo) &&
                children.TryGetValue("VarFileInfo", out var fileInfo) &&
                fileInfo.Children.TryGetValue("Translation", out var translation) )
            {
                var transSpan = MemoryMarshal.Cast<byte, LANGANDCODEPAGE>(translation.Value.AsSpan());
                foreach(var trans in transSpan)
                {
                    var locKey = $"{trans.wLanguage:X4}{trans.wCodePage:X4}";
                    if(stringInfo.Children.TryGetValue(locKey, out var locInfo))
                    {
                        var encoding = Encoding.GetEncoding(trans.wCodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
                        var culture = FindCulture(trans.wLanguage);
                        foreach(var (propKey, propUri, useLang) in predefinedProperties)
                        {
                            if(locInfo.Children.TryGetValue(propKey, out var propInfo))
                            {
                                var value = propInfo.GetValue(encoding);
                                if(culture != null && useLang)
                                {
                                    node.Set(propUri, value, new LanguageCode(culture));
                                }else{
                                    node.Set(propUri, value);
                                }
                            }
                        }
                        if(locInfo.Children.TryGetValue("InternalName", out var nameInfo))
                        {
                            label = nameInfo.GetValue(encoding);
                        }
                    }
                }
            }

            return label;
        }

        static CultureInfo FindCulture(int language)
        {
            if(language > 0)
            {
                try{
                    return CultureInfo.GetCultureInfo(language);
                }catch(CultureNotFoundException)
                {

                }
            }
            return null;
        }

        class EntryInfo
        {
            public string Name { get; }
            public ArraySegment<byte> Value { get; }
            public string ValueString { get; }
            public IReadOnlyDictionary<string, EntryInfo> Children { get; }
            public int Length => Value.Count;

            public EntryInfo(ArraySegment<byte> dataSegment)
            {
                var data = dataSegment.AsSpan();
                var header = MemoryMarshal.Cast<byte, ushort>(data);
                var length = header[0];
                var valueLength = header[1];
                data = data.Slice(0, length);
                var nameSpan = MemoryMarshal.Cast<byte, ushort>(data.Slice(4));
                var dataType = nameSpan[0];
                var unicode = dataType <= 1;
                int dataEnd;
                if(unicode)
                {
                    var name = MemoryMarshal.Cast<ushort, char>(nameSpan.Slice(1));
                    dataEnd = name.IndexOf('\0');
                    Name = name.Slice(0, dataEnd).ToString();
                    dataEnd = (dataEnd + 1) * sizeof(char) + sizeof(ushort);
                }else{
                    var name = MemoryMarshal.Cast<ushort, byte>(nameSpan);
                    dataEnd = name.IndexOf((byte)0);
                    Name = Encoding.ASCII.GetString(dataSegment.Array, dataSegment.Offset + 4, dataEnd);
                    dataEnd += 1;
                }
                dataEnd += 4;
                Align(ref dataEnd);

                if(unicode && dataType == 1)
                {
                    valueLength *= 2;
                }

                Value = new ArraySegment<byte>(dataSegment.Array, dataSegment.Offset + dataEnd, valueLength);

                if(unicode && dataType == 1)
                {
                    ValueString = MemoryMarshal.Cast<byte, char>(Value.AsSpan()).ToString().TrimEnd('\0');
                }

                dataEnd += valueLength;

                var children = new Dictionary<string, EntryInfo>();
                
                while(dataEnd < length)
                {
                    Align(ref dataEnd);
                    var childLength = BitConverter.ToUInt16(dataSegment.Array, dataSegment.Offset + dataEnd);
                    var childSegment = new ArraySegment<byte>(dataSegment.Array, dataSegment.Offset + dataEnd, childLength);
                    var info = new EntryInfo(childSegment);
                    children[info.Name] = info;
                    dataEnd += childLength;
                }
                Children = children;
            }

            public string GetValue(Encoding encoding)
            {
                return ValueString ?? encoding.GetString(Value.Array, Value.Offset, Value.Count).TrimEnd('\0');
            }

            public override string ToString()
            {
                return Name;
            }
        }

        static void Align(ref int offset)
        {
            int rem = offset % sizeof(int);
            if(rem > 0) offset += sizeof(int) - rem;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct VS_FIXEDFILEINFO
        {
            public readonly uint dwSignature;
            public readonly uint dwStrucVersion;
            public readonly uint dwFileVersionMS;
            public readonly uint dwFileVersionLS;
            public readonly uint dwProductVersionMS;
            public readonly uint dwProductVersionLS;
            public readonly uint dwFileFlagsMask;
            public readonly uint dwFileFlags;
            public readonly uint dwFileOS;
            public readonly uint dwFileType;
            public readonly uint dwFileSubtype;
            public readonly uint dwFileDateMS;
            public readonly uint dwFileDateLS;
        }
        
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
    }
}
