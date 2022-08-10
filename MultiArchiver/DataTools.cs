using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Stores many utility methods for manipulating data any deriving information from it.
    /// </summary>
    public static class DataTools
    {
        static readonly string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        /// <summary>
        /// Creates a human-friendly size string using standard 1024-based size units.
        /// </summary>
        /// <param name="value">The size to be formatted.</param>
        /// <param name="decimalPlaces">How many decimal places to include in the size.</param>
        /// <returns>The size formatted as "[-]{value} {unit_prefix}B"</returns>
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

        /// <summary>
        /// The maximum length of a BOM in all Unicode encodings.
        /// </summary>
        public static readonly int MaxBomLength = knownBoms.Max(b => b.Length);

        /// <summary>
        /// Determines if the input starts with a Unicode BOM and returns its length.
        /// </summary>
        /// <param name="data">The input as a span of bytes.</param>
        /// <returns>The length of the BOM, if any, or 0.</returns>
        public static int FindBom(Span<byte> data)
        {
            foreach(var bom in knownBoms)
            {
                if(data.StartsWith(bom)) return bom.Length;
            }
            return 0;
        }

        /// <summary>
        /// Used to split the domain name or IP in the host portion of a URI.
        /// </summary>
        static readonly char[] hostSplitChars = { '.' };

        /// <summary>
        /// Used to split the path portion of a URI if the host is specified.
        /// </summary>
        static readonly char[] slashSplitChars = { '/' };

        /// <summary>
        /// Used to split the path portion of a URI if the host is not specified.
        /// </summary>
        static readonly char[] colonSplitChars = { ':' };

        /// <summary>
        /// Breaks down a URI according to its components in a natural hierarchy,
        /// from the top-level domain name, towards its fragment.
        /// </summary>
        /// <param name="uri">The URI to dissect.</param>
        /// <returns>The sequence of all URI components in order.</returns>
        static IEnumerable<string> GetUriMediaTypeComponents(Uri uri)
        {
            switch(uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    // A domain is split into its individual domain names, starting from the TLD
                    if(!String.IsNullOrEmpty(uri.IdnHost))
                    {
                        foreach(var name in uri.IdnHost.Split(hostSplitChars).Reverse())
                        {
                            yield return name;
                        }
                    }
                    break;
                case UriHostNameType.IPv4:
                    // An IPv4 is split into its component octets
                    if(!String.IsNullOrEmpty(uri.Host))
                    {
                        foreach(var component in uri.Host.Split(hostSplitChars))
                        {
                            yield return component;
                        }
                    }
                    break;
                case UriHostNameType.Unknown:
                    break;
                default:
                    // Any other type of host is returned as is
                    if(!String.IsNullOrEmpty(uri.Host))
                    {
                        yield return uri.Host;
                    }
                    break;
            }
            // The scheme and port are next
            yield return uri.Scheme;
            if(!uri.IsDefaultPort)
            {
                yield return uri.Port.ToString();
            }
            // The path is split by : or / based on the presence of the host (it usually corresponds)
            foreach(var segment in uri.AbsolutePath.Split(uri.HostNameType != UriHostNameType.Unknown ? slashSplitChars : colonSplitChars, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return segment;
            }
            // Followed by the query and fragment
            if(!String.IsNullOrEmpty(uri.Query))
            {
                yield return uri.Query;
            }
            if(!String.IsNullOrEmpty(uri.Fragment))
            {
                yield return uri.Fragment;
            }
        }

        /// <summary>
        /// These characters are not allowed in a MIME type. The &amp; is allowed, but is used for other purposes.
        /// </summary>
        static readonly Regex badMimeCharacters = new Regex(@"[^a-zA-Z0-9_-]+", RegexOptions.Compiled);

        /// <summary>
        /// Creates a fake media type from a namespace URI, PUBLIC identifier,
        /// and the root element name in an XML document.
        /// </summary>
        /// <param name="ns">The root namespace URI (may be null).</param>
        /// <param name="publicId">The PUBLIC identifier (may be null).</param>
        /// <param name="rootName">The name of the root element.</param>
        /// <returns>A MIME type in the form of "application/x.ns.{path}+xml", where path
        /// is formed from the individual components of <paramref name="ns"/>, ending with <paramref name="rootName"/>.
        /// If <paramref name="ns"/> is null and <paramref name="publicId"/> is provided,
        /// the namespace URI is created via <see cref="UriTools.CreatePublicId(string)"/>.
        /// </returns>
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
            var replaced = String.Join(".",
                GetUriMediaTypeComponents(ns)
                .Select(c => badMimeCharacters.Replace(c, m => {
                    return String.Join("", Encoding.UTF8.GetBytes(m.Value).Select(b => $"&{b:X2}"));
                })));
            return $"application/x.ns.{replaced}.{rootName}+xml";
        }

        /// <summary>
        /// Creates a fake media type from a .NET type.
        /// </summary>
        /// <typeparam name="T">The type to use for the media type.</typeparam>
        /// <returns>A MIME type in the form of "application/x.obj.{name}", where name
        /// is formed from the concatenation of the name of the type and names of all
        /// its generic arguments.
        /// </returns>
        public static string GetFakeMediaTypeFromType<T>()
        {
            return FakeTypeNameCache<T>.Name;
        }

        /// <summary>
        /// Matches a capital letter before which a hyphen could be placed.
        /// </summary>
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
                string name;
                if(String.IsNullOrEmpty(type.Namespace))
                {
                    name = type.Name;
                }else{
                    // Get all similarly named types in the assembly
                    var similarTypes = type.Assembly.GetTypes().Where(t => t.IsPublic && !t.Equals(type) && t.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase));
                    // Get the length of the namespace prefix shared by all these types
                    int prefix = similarTypes.Select(t => CommonPrefix(t.Namespace, type.Namespace)).DefaultIfEmpty(type.Namespace.Length).Max();
                    // Prepend the determining part of the namespace to the name
                    name = (prefix == type.Namespace.Length ? "" : type.Namespace.Substring(prefix)) + type.Name;
                }
                // Strip the arity
                int index = name.IndexOf('`');
                if(index != -1) name = name.Substring(0, index);
                // Produce components from the encoded name and its generic arguments
                var components = new List<string>();
                components.Add(FormatName(name));
                components.AddRange(type.GetGenericArguments().Select(GetTypeFriendlyName));
                return String.Join(".", components);
            }

            /// <summary>
            /// Returns the length of the common prefix of <paramref name="a"/> and <paramref name="b"/>.
            /// </summary>
            static int CommonPrefix(string a, string b)
            {
                int max = Math.Min(a.Length, b.Length);
                for(int i = 0; i < max; i++)
                {
                    if(a[i] != b[i]) return i;
                }
                return max;
            }

            /// <summary>
            /// Produces a MIME-friendly name by hyphenating the name, converting to lowercase and encoding unsafe characters.
            /// </summary>
            static string FormatName(string name)
            {
                name = hyphenCharacters.Replace(name, m => (m.Index > 0 ? "-" : "") + m.Value.ToLower());
                name = badMimeCharacters.Replace(name, m =>  String.Join("", Encoding.UTF8.GetBytes(m.Value).Select(b => $"&{b:X2}")));
                return name;
            }
        }

        /// <summary>
        /// Creates a fake media type from a file signature characters.
        /// </summary>
        /// <param name="signature">The signature of the file.</param>
        /// <returns>A MIME type in the form of "application/x.sig.{<paramref name="signature"/>}"
        /// (converted to lowercase).
        /// </returns>
        public static string GetFakeMediaTypeFromSignature(string signature)
        {
            return "application/x.sig." + signature.ToLowerInvariant();
        }

        /// <summary>
        /// Creates a fake media type from an interpreter command.
        /// </summary>
        /// <param name="interpreter">The interpreter command.</param>
        /// <returns>A MIME type in the form of "application/x.exec.{<paramref name="interpreter"/>}"
        /// (converted to lowercase).
        /// </returns>
        public static string GetFakeMediaTypeFromInterpreter(string interpreter)
        {
            return "application/x.exec." + interpreter.ToLowerInvariant();
        }

        /// <summary>
        /// Computes a base32-encoded string from a sequence of bytes.
        /// The alphabet is "QAZ2WSX3EDC4RFV5TGB6YHN7UJM8K9LP".
        /// </summary>
        /// <typeparam name="TList">The type of the byte sequence.</typeparam>
        /// <param name="bytes">The sequence to compute from.</param>
        /// <param name="sb">An instance of <see cref="StringBuilder"/> that receives the output.</param>
        public static void Base32<TList>(TList bytes, StringBuilder sb) where TList : IReadOnlyList<byte>
        {
            const string chars = "QAZ2WSX3EDC4RFV5TGB6YHN7UJM8K9LP";

            byte index;
            int hi = 5;
            int currentByte = 0;

            while(currentByte < bytes.Count)
            {
                if(hi > 8)
                {
                    index = (byte)(bytes[currentByte++] >> (hi - 5));
                    if(currentByte != bytes.Count)
                    {
                        index = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | index);
                    }
                    hi -= 3;
                }else if(hi == 8)
                { 
                    index = (byte)(bytes[currentByte++] >> 3);
                    hi -= 3; 
                }else{
                    index = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
                    hi += 5;
                }
                sb.Append(chars[index]);
            }
        }

        const string base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        static readonly BigInteger base58AlphabetLength = base58Alphabet.Length;

        /// <summary>
        /// Computes a base58-encoded string from a sequence of bytes.
        /// The alphabet is "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".
        /// </summary>
        /// <typeparam name="TList">The type of the byte sequence.</typeparam>
        /// <param name="bytes">The sequence to compute from.</param>
        /// <param name="sb">An instance of <see cref="StringBuilder"/> that receives the output.</param>
        public static void Base58<TList>(TList bytes, StringBuilder sb) where TList : IReadOnlyList<byte>
        {
            int pos = 0;
            while(bytes[pos] == 0)
            {
                sb.Append('1');
                pos++;
            }
            int len = bytes.Count - pos;
            if(len == 0) return;
            var data = new byte[len + (bytes[pos] > SByte.MaxValue ? 1 : 0)];
            for(int i = 0; i < len; i++)
            {
                data[len - 1 - i] = bytes[pos + i];
            }
            var num = new BigInteger(data);
            foreach(var c in Base58Bytes(num).Reverse())
            {
                sb.Append(base58Alphabet[c]);
            }
        }

        static IEnumerable<int> Base58Bytes(BigInteger num)
        {
            while(num > 0)
            {
                num = BigInteger.DivRem(num, base58AlphabetLength, out var rem);
                yield return (int)rem;
            }
        }

        /// <summary>
        /// Computes a base64url-encoded string from a sequence of bytes.
        /// The "+/" characters are replaced with "-_" in the alphabet. Trailing "=" is stripped.
        /// </summary>
        /// <param name="bytes">The sequence to compute from.</param>
        /// <param name="sb">An instance of <see cref="StringBuilder"/> that receives the output.</param>
        public static void Base64Url(ArraySegment<byte> bytes, StringBuilder sb)
        {
            string str = bytes.ToBase64String();
            UriString(str, sb);
        }

        /// <summary>
        /// Computes a base64url-encoded string from a sequence of bytes.
        /// The "+/" characters are replaced with "-_" in the alphabet. Trailing "=" is stripped.
        /// </summary>
        /// <param name="bytes">The sequence to compute from.</param>
        /// <param name="sb">An instance of <see cref="StringBuilder"/> that receives the output.</param>
        public static void Base64Url(byte[] bytes, StringBuilder sb)
        {
            string str = Convert.ToBase64String(bytes);
            UriString(str, sb);
        }

        static void UriString(string str, StringBuilder sb)
        {
            int end = 0;
            for(end = str.Length; end > 0; end--)
            {
                if(str[end - 1] != '=')
                {
                    break;
                }
            }

            for(int i = 0; i < end; i++)
            {
                char c = str[i];

                switch (c) {
                    case '+':
                        sb.Append('-');
                        break;
                    case '/':
                        sb.Append('_');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }

        /// <summary>
        /// Encodes a <see cref="UInt64"/> value as a variable-length integer.
        /// </summary>
        /// <param name="value">The input value to encode.</param>
        /// <returns>
        /// A collection of bytes encoding <paramref name="value"/>,
        /// in the form described by https://github.com/multiformats/unsigned-varint.
        /// </returns>
        public static IEnumerable<byte> Varint(ulong value) 
        {
            while(value >= 0x80)
            {
                yield return (byte)(value | 0x80);
                value >>= 7;
            }
            yield return (byte)value;
        }

        /// <summary>
        /// Encodes a multihash value. See https://github.com/multiformats/multihash for details.
        /// </summary>
        /// <param name="id">The identifier of the particular hash.</param>
        /// <param name="hash">An array storing the bytes of the hash.</param>
        /// <param name="hashLength">The length of the hash as stored in the result.</param>
        /// <returns>A list of bytes representing the multihash.</returns>
        public static List<byte> EncodeMultihash(ulong id, byte[] hash, int? hashLength = null)
        {
            var multihash = new List<byte>(2 + hash.Length);
            multihash.AddRange(Varint(id));
            multihash.AddRange(Varint((ulong)(hashLength ?? hash.Length)));
            multihash.AddRange(hash);
            return multihash;
        }
        
        /// <summary>
        /// Text characters whose presence invalidates a signature.
        /// </summary>
        static readonly ISet<byte> invalidSigBytes = new SortedSet<byte>(
            new byte[] { 0x09, 0x0A, 0x0D, (byte)' ', (byte)'-', (byte)'_' }
        );

        /// <summary>
        /// Characters that are allowed to be recognized as part of a signature.
        /// </summary>
        static readonly ISet<byte> recognizedSigBytes = new SortedSet<byte>(
            Enumerable.Range('a', 26).Concat(
                Enumerable.Range('A', 26)
            ).Concat(
                Enumerable.Range('0', 10)
            ).Select(i => (byte)i).Concat(invalidSigBytes)
        );

        const int maxSignatureLength = 8;

        /// <summary>
        /// Extracts the first initial bytes of <paramref name="header"/> and returns them
        /// as a string if they could correspond to a valid signature.
        /// </summary>
        /// <param name="header">The initial part of a file's data.</param>
        /// <returns>The signature bytes as an ASCII string, or null if there is no valid signature.</returns>
        /// <remarks>
        /// The signature is the portion at the beginning of the header between 2 and 8 characters,
        /// allowing to contain characters a-zA-Z0-9. If a whitespace character, a hyphen or an underscore
        /// character occurs after the signature, it is invalidated.
        /// </remarks>
        public static string ExtractSignature(ArraySegment<byte> header)
        {
            var magicSig = header.Take(maxSignatureLength + 1).TakeWhile(recognizedSigBytes.Contains).ToArray();
            if(magicSig.Length >= 2 && magicSig.Length <= maxSignatureLength && !magicSig.Any(invalidSigBytes.Contains))
            {
                return Encoding.ASCII.GetString(magicSig);
            }
            return null;
        }

        /// <summary>
        /// Matches the first line of a string.
        /// </summary>
        static readonly Regex firstLineRegex = new Regex(@"^(.*?)(?m)\r?$", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the first line from <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The first line, not counting the trailing CR or LF character..</returns>
        public static string ExtractFirstLine(string text)
        {
            var match = firstLineRegex.Match(text);
            if(match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }


        /// <summary>
        /// Matches the first line of a string, capturing the command after a shebang sequence.
        /// </summary>
        static readonly Regex interpreterRegex = new Regex(@"^#!(?:[^\s/]*/|env\s+)*([^\s/]+)(?:\s.*)?(?m)\r?$", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the interpreter command from a shebang sequence at the beginning of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The input string, expected to begin with a shebang (#!) sequence.</param>
        /// <returns>
        /// The command part of the shebang sequence, in the form "#!{path}/{command} {arguments}"
        /// or "#!{path}/env {command} {arguments}".
        /// </returns>
        public static string ExtractInterpreter(string text)
        {
            var match = interpreterRegex.Match(text);
            if(match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }


        /// <summary>
        /// Matches a C0 control character that can't be displayed in a text.
        /// </summary>
        static readonly Regex controlReplacement = new Regex(
            @"[\x00-\x08\x0B\x0C\x0E-\x1F]"
            , RegexOptions.Compiled);

        /// <summary>
        /// Returns a replacement character for a C0 control character, in
        /// the Unicode block starting from U+2400.
        /// </summary>
        static int GetReplacementChar(char c)
        {
            switch(c)
            {
                case '\x7F':
                    return 0x2421;
                default:
                    return c + 0x2400;
            }
        }

        /// <summary>
        /// Replaces undisplayable control characters in <paramref name="str"/> in a way
        /// that is reversible, based on the capabilities of <paramref name="originalEncoding"/>.
        /// </summary>
        /// <param name="str">The string to replace characters in.</param>
        /// <param name="originalEncoding">The original encoding in which the string was decoded, or null.</param>
        /// <returns>
        /// A string formed from the characters in <paramref name="str"/>, but with control characters
        /// replaced with their respective Unicode replacement characters starting from U+2400.
        /// If <paramref name="originalEncoding"/> is a Unicode encoding or other encoding
        /// that can encode the replacement characters, the characters are left unchanged,
        /// because it would be ambiguous whether they were a part of the original text or not.
        /// </returns>
        public static string ReplaceControlCharacters(string str, Encoding originalEncoding)
        {
            return controlReplacement.Replace(str, m => {
                var replacement = ((char)GetReplacementChar(m.Value[0])).ToString();
                if(originalEncoding == null)
                {
                    return replacement;
                }
                try{
                    originalEncoding.GetBytes(replacement);
                }catch(ArgumentException)
                {
                    return replacement;
                }
                return m.Value;
            });
        }

        /// <summary>
        /// Determines whether a byte sequence encodes binary, or non-textual, data.
        /// </summary>
        /// <param name="data">A complete sequence of bytes to be checked.</param>
        /// <returns>True if the data is not textual, false otherwise.</returns>
        /// <remarks>
        /// Usually, text is not allowed to contain the NUL character.
        /// This condition is loosened a bit here: a text may end with
        /// any number of NUL characters, but it cannot contain a non-NUL character
        /// after a NUL character, and it cannot start with a NUL character.
        /// </remarks>
        public static bool IsBinary(ArraySegment<byte> data)
        {
            int index = data.IndexOf((byte)0);
            if(index == 0)
            {
                return true;
            }
            if(index != -1)
            {
                for(int i = index + 1; i < data.Count; i++)
                {
                    if(data.At(i) != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Creates a v5 (SHA1-encoded) UUID from a namespace UUID and a name.
        /// </summary>
        /// <param name="namespaceBytes">The UUID of the namespaces, encoded in bytes.</param>
        /// <param name="name">The name in a form specific to the namespaces.</param>
        /// <returns>
        /// An instance of <see cref="Guid"/> formed by hashing the
        /// <paramref name="namespaceBytes"/> followed by <paramref name="name"/>,
        /// encoded in UTF-8, wrapped in a UUID structure.
        /// </returns>
        public static Guid GuidFromName(byte[] namespaceBytes, string name)
        {
            byte[] hash;
            using(var buffer = new MemoryStream())
            {
                buffer.Write(namespaceBytes, 0, namespaceBytes.Length);
                using(var writer = new StreamWriter(buffer, Encoding.UTF8))
                {
                    writer.Write(name);
                    writer.Flush();
                    buffer.Position = 0;
                    hash = BuiltInHash.SHA1.ComputeHash(buffer, null).Result;
                }
            }
            return GuidFromHash(hash);
        }

        static Guid GuidFromHash(byte[] hash)
        {
            // Variant 5
            hash[6] = (byte)((hash[7] & 0x0F) | (5 << 4));
            // Version 1
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

            var span = hash.AsSpan();
            var f4 = span.MemoryCast<int>();
            // Flip time_low
            f4[0] = (hash[0] << 24) | (hash[1] << 16) | (hash[2] << 8) | hash[3];
            var f2 = f4.Slice(1).MemoryCast<short>();
            // Flip time_mid
            f2[0] = unchecked((short)((hash[4] << 8) | hash[5]));
            // Flip time_hi_and_version
            f2[1] = unchecked((short)((hash[6] << 8) | hash[7]));

            // This should match the internal Guid structure
            return span.MemoryCast<Guid>()[0];
        }

        /// <summary>
        /// Matches a string that is unsafe for embedding or displaying. See <see cref="IsSafeString"/> for details.
        /// </summary>
        static readonly Regex unsafeStringRegex = new Regex(@"^[\p{M}\u200D]|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x84\x86-\x9F]|(^|[^\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF]($|[^\uDC00-\uDFFF])", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Determines whether <paramref name="str"/> is a string safe for displaying or storing as text.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if the string contains unsafe characters, false otherwise.</returns>
        /// <remarks>
        /// XML 1.0 prohibits C0 control codes and discourages the use of C1, with the exception of line separators;
        /// such characters cannot be encoded in RDF/XML and therefore are semantically invalid.
        /// Unpaired surrogate characters are also prohibited (since the input must be a valid UTF-16 string).
        /// Additionally, a leading combining character or ZWJ could cause troubles when displayed.
        /// The string also shall not contain any unassigned Unicode characters.
        /// </remarks>
        public static bool IsSafeString(string str)
        {
            if(unsafeStringRegex.IsMatch(str)) return false;
            var e = StringInfo.GetTextElementEnumerator(str);
            while(e.MoveNext())
            {
                if(Char.GetUnicodeCategory(str, e.ElementIndex) == UnicodeCategory.OtherNotAssigned)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a JSON-LD literal from a string value.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        /// <returns>A valid JSON string with the @value field.</returns>
        public static string CreateLiteralJsonLd(string value)
        {
            return $@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)}}}";
        }

        /// <summary>
        /// Creates a JSON-LD literal from a string value and its datatype.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        /// <param name="datatype">The datatype of the literal.</param>
        /// <returns>A valid JSON string with the @value and @type fields.</returns>
        public static string CreateLiteralJsonLd(string value, Uri datatype)
        {
            return $@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)},""@type"":{HttpUtility.JavaScriptStringEncode(datatype.AbsoluteUri, true)}}}";
        }

        /// <summary>
        /// Creates a JSON-LD literal from a string value and its language tag.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        /// <param name="language">The language tag of the literal.</param>
        /// <returns>A valid JSON string with the @value and @language fields.</returns>
        public static string CreateLiteralJsonLd(string value, string language)
        {
            return $@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)},""@language"":{HttpUtility.JavaScriptStringEncode(language, true)}}}";
        }

        /// <summary>
        /// Returns a user-friendly string representation of an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="entity">The object to retrieve the name from.</param>
        /// <returns>
        /// <para>
        /// If <paramref name="entity"/> is an instance of <see cref="Type"/>,
        /// returns its name expressed in a C#-like syntax.
        /// </para>
        /// <para>
        /// If <typeparamref name="T"/> is <see cref="IStreamFactory"/>,
        /// returns the <see cref="IStreamFactory.Length"/> of <paramref name="entity"/>.
        /// </para>
        /// <para>
        /// Otherwise calls <see cref="Object.ToString"/>; if the returned string
        /// is empty or a result of the default implementation of <see cref="Object.ToString"/>,
        /// calls <see cref="GetUserFriendlyName{T}(T)"/> on the type of <paramref name="entity"/>.
        /// </para>
        /// </returns>
        public static string GetUserFriendlyName<T>(T entity)
        {
            if(entity is Type type)
            {
                if(!type.IsGenericType) return type.Name;
                var sb = new StringBuilder();
                var typeName = type.Name;
                var index = typeName.IndexOf('`');
                if(index != -1)
                {
                    sb.Append(type.Name, 0, index);
                }else{
                    sb.Append(type.Name);
                }
                sb.Append("<");
                bool first = true;
                foreach(var typeArg in type.GetGenericArguments())
                {
                    if(first)
                    {
                        first = false;
                    }else{
                        sb.Append(", ");
                    }
                    sb.Append(GetUserFriendlyName(typeArg));
                }
                sb.Append(">");
                return sb.ToString();
            }

            var name = typeof(T).Equals(typeof(IStreamFactory))
                ? $"Data ({((IStreamFactory)entity).Length} B)" : entity.ToString();

            type = entity.GetType();
            if(String.IsNullOrWhiteSpace(name))
            {
                // no useful name
                return GetUserFriendlyName(type);
            }

            if(name == type.ToString())
            {
                // ToString is not overridden
                return GetUserFriendlyName(type);
            }

            try{
                type = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetType(name, false)).FirstOrDefault(t => t != null);
                if(type != null)
                {
                    return GetUserFriendlyName(type);
                }
            }catch{
                // name is not a valid type name
            }
            return name;
        }

        /// <summary>
        /// Matches any sequence of '*', a single occurence of '?', or a sequence of any other characters.
        /// </summary>
        static readonly Regex wildcardRegex = new Regex(@"(\*+|\?)|[^*?]+", RegexOptions.Compiled);

        /// <summary>
        /// Creates an instance of <see cref="Regex"/> from a wildcard pattern.
        /// </summary>
        /// <param name="pattern">The pattern, using * and ? as special characters.</param>
        /// <returns>
        /// A regular expression matching the whole string, where each occurence of '*'
        /// in <paramref name="pattern"/> is replaced by ".*", each occurence of '?'
        /// is replaced by ".", and the remaining portions are escaped with <see cref="Regex.Escape(string)"/>.
        /// </returns>
        public static Regex ConvertWildcardToRegex(string pattern)
        {
            string Replacer(Match match)
            {
                if(match.Groups[1].Success)
                {
                    switch(match.Groups[1].Value[0])
                    {
                        case '*':
                            return ".*";
                        case '?':
                            return ".";
                    }
                }
                return Regex.Escape(match.Value);
            }

            return new Regex($"^{wildcardRegex.Replace(pattern, Replacer)}$", RegexOptions.Singleline);
        }
    }
}
