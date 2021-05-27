using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IS4.MultiArchiver
{
    public static class DataTools
    {
        static readonly string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        public static string SizeSuffix(long value, int decimalPlaces)
        {
            if(value < 0) return "-" + SizeSuffix(-value, decimalPlaces);
            if(value == 0) return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} B", 0);

            int n = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (n * 10));
            if(Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                n += 1;
                adjustedSize /= 1024;
            }
            return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} {{1}}", adjustedSize, units[n]);
        }

        static readonly byte[][] knownBoms = new[]
        {
            Encoding.UTF8,
            Encoding.Unicode,
            Encoding.BigEndianUnicode,
            new UTF32Encoding(true, true),
            new UTF32Encoding(false, true)
        }.Select(e => e.GetPreamble()).ToArray();

        public static readonly int MaxBomLength = knownBoms.Max(b => b.Length);

        public static int FindBom(Span<byte> data)
        {
            foreach(var bom in knownBoms)
            {
                if(data.StartsWith(bom)) return bom.Length;
            }
            return 0;
        }

        static readonly Regex badCharacters = new Regex(@"://|//|[^a-zA-Z0-9._-]", RegexOptions.Compiled);

        public static string GetFakeMediaTypeFromXml(Uri ns, string publicId, string rootName)
        {
            if(ns == null)
            {
                if(publicId != null)
                {
                    ns = UriTools.CreatePublicId(publicId);
                }else{
                    return $"application/x.ns.{rootName}+xml";
                }
            }
            if(ns.HostNameType == UriHostNameType.Dns && !String.IsNullOrEmpty(ns.IdnHost))
            {
                var host = ns.IdnHost;
                var builder = new UriBuilder(ns);
                builder.Host = String.Join(".", host.Split('.').Reverse());
                if(!ns.Authority.EndsWith($":{builder.Port}", StringComparison.Ordinal))
                {
                    builder.Port = -1;
                }
                ns = builder.Uri;
            }
            var replaced = badCharacters.Replace(ns.OriginalString, m => {
                switch(m.Value)
                {
                    case "[":
                    case "]": return "";
                    case "%": return "&";
                    case ":":
                    case "/":
                    case "?":
                    case ";":
                    case "&":
                    case "=":
                    case "//":
                    case "://": return ".";
                    default: return String.Join("", Encoding.UTF8.GetBytes(m.Value).Select(b => $"&{b:X2}"));
                }
            });
            return $"application/x.ns.{replaced}.{rootName}+xml";
        }

        public static string GetFakeMediaTypeFromType<T>()
        {
            return FakeTypeNameCache<T>.Name;
        }

        static readonly Regex hyphenCharacters = new Regex(@"\p{Lu}+($|(?=\p{Lu}))|\p{Lu}(?!\p{Lu})", RegexOptions.Compiled);

        class FakeTypeNameCache<T>
        {
            public static readonly string Name = GetName();

            static string GetName()
            {
                return "application/x.obj." + GetTypeFriendlyName(typeof(T));
            }

            static string GetTypeFriendlyName(Type type)
            {
                var components = new List<string>();
                string name;
                if(String.IsNullOrEmpty(type.Namespace))
                {
                    name = type.Name;
                }else{
                    var similarTypes = type.Assembly.GetTypes().Where(t => t.IsPublic && !t.Equals(type) && t.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase));
                    int prefix = similarTypes.Select(t => CommonPrefix(t.Namespace, type.Namespace)).DefaultIfEmpty(type.Namespace.Length).Max();
                    name = (prefix == type.Namespace.Length ? "" : type.Namespace.Substring(prefix)) + type.Name;
                }
                int index = name.IndexOf('`');
                if(index != -1) name = name.Substring(0, index);
                components.Add(FormatName(name));
                components.AddRange(type.GetGenericArguments().Select(GetTypeFriendlyName));
                return String.Join(".", components);
            }

            static int CommonPrefix(string a, string b)
            {
                int max = Math.Min(a.Length, b.Length);
                for(int i = 0; i < max; i++)
                {
                    if(a[i] != b[i]) return i;
                }
                return max;
            }

            static string FormatName(string name)
            {
                name = hyphenCharacters.Replace(name, m => (m.Index > 0 ? "-" : "") + m.Value.ToLower());
                name = badCharacters.Replace(name, m =>  String.Join("", Encoding.UTF8.GetBytes(m.Value).Select(b => $"&{b:X2}")));
                return name;
            }
        }
    }
}
