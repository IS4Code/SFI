using IS4.MultiArchiver.Media.Modules;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using static Vanara.PInvoke.VersionDll;

namespace IS4.MultiArchiver.Analyzers
{
    public class WinVersionAnalyzer : EntityAnalyzer, IEntityAnalyzer<WinVersionInfo>
    {
        public AnalysisResult Analyze(WinVersionInfo entity, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);
            return new AnalysisResult(node, ReadVersion(node, entity.Data));
        }
        
        static readonly byte[] ansiVersionString = Encoding.ASCII.GetBytes("VS_VERSION_INFO");
        static readonly byte[] unicodeVersionString = Encoding.Unicode.GetBytes("\0VS_VERSION_INFO");

        static readonly Dictionary<string, (PropertyUri prop, bool lang)> predefinedProperties = new Dictionary<string, (PropertyUri, bool)>
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

        unsafe string ReadVersion(ILinkedNode node, ArraySegment<byte> versionData)
        {
            string label = null;
            using(var stream = versionData.AsStream(false))
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
                } else if(signature.StartsWith(unicodeVersionString))
                {
                    useAnsi = false;
                } else
                {
                    return null;
                }

                fixed (byte* data = bufferData)
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
                            CultureInfo culture;
                            if(trans.wLanguage > 0)
                            {
                                try
                                {
                                    culture = CultureInfo.GetCultureInfo(trans.wLanguage);
                                } catch(CultureNotFoundException)
                                {
                                    culture = null;
                                }
                            } else
                            {
                                culture = null;
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
                                    if(culture != null && pair.Value.lang)
                                    {
                                        node.Set(pair.Value.prop, value, new LanguageCode(culture));
                                    } else
                                    {
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
            if(result && ((byte*)lplpBuffer < (byte*)pBlock - length || (byte*)lplpBuffer >= (byte*)pBlock + 2 * length))
            {
                throw new InvalidOperationException("Version block encoding was not correctly detected; unrelated memory has been corrupted as a result.");
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
    }
}
