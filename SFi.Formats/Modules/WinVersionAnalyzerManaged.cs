using IS4.SFI.Formats.Modules;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes a Windows version resources, as an instance of <see cref="WinVersionInfo"/>
    /// storing the VS_VERSIONINFO structure. The parsing of the structure is done
    /// in purely managed code.
    /// </summary>
    public class WinVersionAnalyzerManaged : EntityAnalyzer<WinVersionInfo>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(WinVersionInfo entity, AnalysisContext context, IEntityAnalyzers analyzers)
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

        string? ReadVersion(ILinkedNode node, ArraySegment<byte> versionData)
        {
            string? label = null;
            var info = new EntryInfo(versionData);

            if(info.Name == "VS_VERSION_INFO")
            {
                ref var version = ref info.Value.AsSpan().MemoryCast<VS_FIXEDFILEINFO>()[0];
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
                var transSpan = translation.Value.AsSpan().MemoryCast<LANGANDCODEPAGE>();
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
                                if(IsDefined(propInfo.GetValue(encoding), out var value))
                                {
                                    if(culture != null && useLang)
                                    {
                                        node.Set(propUri, value, new LanguageCode(culture));
                                    }else{
                                        node.Set(propUri, value);
                                    }
                                }
                            }
                        }
                        if(locInfo.Children.TryGetValue("InternalName", out var nameInfo))
                        {
                            if(IsDefined(nameInfo.GetValue(encoding), out var value))
                            {
                                label = value;
                            }
                        }
                    }
                }
            }

            return label;
        }

        static CultureInfo? FindCulture(int language)
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
            public string Name { get; } = "";
            public ArraySegment<byte> Value { get; }
            public string? ValueString { get; }
            public IReadOnlyDictionary<string, EntryInfo> Children { get; }
            public int Length => Value.Count;
            public Exception? Exception { get; }

            static readonly char[] nullCharEnd = { '\0' };

            public EntryInfo(ArraySegment<byte> dataSegment)
            {
                var children = new Dictionary<string, EntryInfo>(StringComparer.OrdinalIgnoreCase);
                Children = children;

                try{ 
                    var data = dataSegment.AsSpan();
                    var header = data.MemoryCast<ushort>();
                    var length = header[0];
                    var valueLength = header[1];
                    data = data.Slice(0, length);
                    var nameSpan = data.Slice(4).MemoryCast<ushort>();
                    var dataType = nameSpan[0];
                    int dataEnd;
                    if(dataType <= 1)
                    {
                        var name = nameSpan.Slice(1).MemoryCast<char>();
                        dataEnd = name.IndexOf('\0');
                        Name = name.Slice(0, dataEnd).ToString();
                        dataEnd = (dataEnd + 1) * sizeof(char) + sizeof(ushort);
                    }else{
                        dataType = 0;
                        var name = nameSpan.MemoryCast<byte>();
                        dataEnd = name.IndexOf((byte)0);
                        Name = Encoding.ASCII.GetString(dataSegment.Array, dataSegment.Offset + 4, dataEnd);
                        dataEnd += 1;
                    }
                    dataEnd += 4;
                    Align(ref dataEnd);

                    Value = dataSegment.Slice(dataEnd, valueLength);

                    if(dataType == 1 &&
                        valueLength > 0 &&
                        dataEnd + valueLength * 2 <= dataSegment.Count &&
                        (valueLength % 2 == 1 || !Value.AsSpan().MemoryCast<char>().EndsWith(nullCharEnd.AsSpan()))
                        )
                    {
                        valueLength *= 2;
                        Value = dataSegment.Slice(dataEnd, valueLength);
                    }

                    if(dataType == 1)
                    {
                        ValueString = Value.AsSpan().MemoryCast<char>().ToString().TrimEnd('\0');
                    }

                    dataEnd += valueLength;

                    Align(ref dataEnd);
                    while(dataEnd < length)
                    {
                        var childLength = BitConverter.ToUInt16(dataSegment.Array, dataSegment.Offset + dataEnd);
                        var childSegment = dataSegment.Slice(dataEnd, childLength);
                        var info = new EntryInfo(childSegment);
                        children[info.Name] = info;
                        dataEnd += childLength;
                        Align(ref dataEnd);
                    }
                }catch(Exception e)
                {
                    Exception = e;
                }
            }

            public string GetValue(Encoding encoding)
            {
                return ValueString ?? encoding.GetString(Value).TrimEnd('\0');
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
        readonly struct VS_FIXEDFILEINFO
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
        readonly struct LANGANDCODEPAGE
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
