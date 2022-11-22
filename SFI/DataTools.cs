using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace IS4.SFI
{
    /// <summary>
    /// Stores many utility methods for manipulating data and deriving information from it.
    /// </summary>
    public static class DataTools
    {
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
        public static int FindBom(ReadOnlySpan<byte> data)
        {
            foreach(var bom in knownBoms)
            {
                if(data.StartsWith(bom)) return bom.Length;
            }
            return 0;
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
            int end;
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

                switch(c)
                {
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
        public static string? ExtractSignature(ArraySegment<byte> header)
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
        static readonly Regex firstLineRegex = new(@"^(.*?)(?m)\r?$", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the first line from <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The first line, not counting the trailing CR or LF character..</returns>
        public static string? ExtractFirstLine(string text)
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
        static readonly Regex interpreterRegex = new(@"^#!(?:[^\s/]*/|env\s+)*([^\s/]+)(?:\s.*)?(?m)\r?$", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the interpreter command from a shebang sequence at the beginning of <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The input string, expected to begin with a shebang (#!) sequence.</param>
        /// <returns>
        /// The command part of the shebang sequence, in the form "#!{path}/{command} {arguments}"
        /// or "#!{path}/env {command} {arguments}".
        /// </returns>
        public static string? ExtractInterpreter(string text)
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
        static readonly Regex controlReplacement = new(
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
        public static string ReplaceControlCharacters(string str, Encoding? originalEncoding)
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
            var sha1 = BuiltInHash.SHA1 ?? throw new NotSupportedException($"{nameof(BuiltInHash)}.{nameof(BuiltInHash.SHA1)} is not supported!");
            byte[] hash;
            using(var buffer = new MemoryStream())
            {
                buffer.Write(namespaceBytes, 0, namespaceBytes.Length);
                using(var writer = new StreamWriter(buffer, Encoding.UTF8))
                {
                    writer.Write(name);
                    writer.Flush();
                    buffer.Position = 0;
                    hash = sha1.ComputeHash(buffer, null).Result;
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
        static readonly Regex unsafeStringRegex = new(@"^[\p{M}\u200D]|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x84\x86-\x9F]|(^|[^\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF]($|[^\uDC00-\uDFFF])", RegexOptions.Compiled | RegexOptions.Multiline);

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
        /// Stores the qualified name of the &lt;x:xmpmeta&gt; element.
        /// </summary>
        public static readonly XmlQualifiedName XmpMetaName = new("xmpmeta", "adobe:ns:meta/");

        /// <summary>
        /// Stores the qualified name of the &lt;rdf:RDF&gt; element.
        /// </summary>
        public static readonly XmlQualifiedName RdfName = new("RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        /// <summary>
        /// Describes <paramref name="node"/> using RDF/XML data stored in an XMP stream.
        /// </summary>
        /// <param name="node">The node to describe.</param>
        /// <param name="stream">The XMP stream.</param>
        /// <param name="subjectUris">A collection of URIs that represent the subject.</param>
        public static void DescribeAsXmp(ILinkedNode node, Stream stream, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            using(var reader = XmlReader.Create(stream))
            {
                DescribeAsXmp(node, reader, subjectUris);
            }
        }

        /// <inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})"/>
        /// <param name="node"><inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})" path="/param[@name='node']"/></param>
        /// <param name="reader">The XML text stream reader.</param>
        /// <param name="subjectUris"><inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})" path="/param[@name='subjectUris']"/></param>
        public static void DescribeAsXmp(ILinkedNode node, TextReader reader, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            using(var xmlReader = XmlReader.Create(reader))
            {
                DescribeAsXmp(node, xmlReader, subjectUris);
            }
        }

        /// <inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})"/>
        /// <param name="node"><inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})" path="/param[@name='node']"/></param>
        /// <param name="xmlReader">The XML reader, positioned at the beginning of the document.</param>
        /// <param name="subjectUris"><inheritdoc cref="DescribeAsXmp(ILinkedNode, Stream, IReadOnlyCollection{Uri})" path="/param[@name='subjectUris']"/></param>
        public static void DescribeAsXmp(ILinkedNode node, XmlReader xmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            if(xmlReader.MoveToContent() == XmlNodeType.Element)
            {
                if(xmlReader.NamespaceURI == XmpMetaName.Namespace && xmlReader.LocalName == XmpMetaName.Name)
                {
                    // The root element is <x:xmpmeta>
                    while(xmlReader.Read())
                    {
                        if(xmlReader.MoveToContent() == XmlNodeType.Element)
                        {
                            if(xmlReader.NamespaceURI == RdfName.Namespace && xmlReader.LocalName == RdfName.Name)
                            {
                                // Found an <rdf:RDF> element
                                node.Describe(xmlReader, subjectUris);
                                continue;
                            }
                            // Found an unknown element, skip
                            xmlReader.Skip();
                        }
                    }
                }
            }
        }
    }
}
