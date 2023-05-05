using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace IS4.SFI
{
    /// <summary>
    /// Stores utility methods for manipulating URIs, as instances of <see cref="Uri"/>.
    /// </summary>
    public static class UriTools
    {
        const string publicid = "publicid:";

        /// <summary>
        /// A URI formatter which uses a prefix to prepend its argument when forming
        /// the resulting URI.
        /// </summary>
        public class PrefixFormatter<T> : IGenericUriFormatter<T>
        {
            readonly string prefix;

            /// <summary>
            /// Creates a new insntace of the formatter, with the chosen prefix.
            /// </summary>
            /// <param name="prefix">The URI prefix to use.</param>
            public PrefixFormatter(string prefix)
            {
                this.prefix = prefix;
            }

            /// <summary>
            /// Creates a new URI from the stored prefix, appended with
            /// <paramref name="value"/>.
            /// </summary>
            /// <param name="value">The value to append to the prefix.</param>
            /// <returns>The full URI formed from the prefix and <paramref name="value"/>.</returns>
            public Uri this[T value] => new(prefix + value, UriKind.Absolute);
        }

        static readonly Regex pubIdRegex = new(@"(^\s+|\s+$)|(\s+)|(\/\/)|(::)|([+:\/;'?#%])", RegexOptions.Compiled);

        /// <summary>
        /// A formatter which uses <see cref="CreatePublicId(string)"/> to convert
        /// PUBLIC identifiers to urn:publicid:.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> PublicIdFormatter = new PublicIdFormatterClass();

        class PublicIdFormatterClass : IIndividualUriFormatter<string>
        {
            public Uri this[string value] => CreatePublicId(value);
        }

        /// <summary>
        /// A formatter which uses <see cref="CreateDataUri(string?, string?, ArraySegment{byte})"/>
        /// to create a data: URI from its components.
        /// </summary>
        public static readonly IIndividualUriFormatter<(string? mediaType, string? charset, ArraySegment<byte> data)> DataUriFormatter = new DataUriFormatterClass();
        
        class DataUriFormatterClass : IIndividualUriFormatter<(string?, string?, ArraySegment<byte>)>
        {
            public Uri this[(string?, string?, ArraySegment<byte>) value] {
                get {
                    var (mediaType, charset, bytes) = value;
                    return CreateDataUri(mediaType, charset, bytes);
                }
            }
        }

        /// <summary>
        /// Creates a data: URI from the provided components. The type of the data:
        /// URI (base64 or percent-encoded) is chosen based on which
        /// variant is shorter. The result <see cref="Uri"/>
        /// also implements <see cref="IIndividualUriFormatter{T}"/> of
        /// <see cref="IFormatObject"/>.
        /// </summary>
        /// <param name="mediaType">The media type of the resource.</param>
        /// <param name="charset">The character set of the resource, if textual.</param>
        /// <param name="bytes">The binary data of the resource.</param>
        /// <returns>A newly created URI encoding the arguments.</returns>
        public static Uri CreateDataUri(string? mediaType, string? charset, ArraySegment<byte> bytes)
        {
            string base64Encoded = ";base64," + bytes.ToBase64String();
            string uriEncoded = "," + EscapeDataBytes(bytes);

            string data = uriEncoded.Length <= base64Encoded.Length ? uriEncoded : base64Encoded;

            switch(charset?.ToLowerInvariant())
            {
                case null:
                    return new DataUri(mediaType ?? "application/octet-stream", data);
                case "ascii":
                case "us-ascii":
                    return new DataUri(mediaType, data);
                default:
                    return new DataUri(mediaType + ";charset=" + charset, data);
            }
        }
        
        /// <summary>
        /// The data: URI produced by <see cref="CreateDataUri(string?, string?, ArraySegment{byte})"/>.
        /// Formatting it using <see cref="IFormatObject"/> will replace
        /// the media type stored by the URI.
        /// </summary>
        class DataUri : EncodedUri, IIndividualUriFormatter<IFormatObject>
        {
            readonly string data;

            public DataUri(string? type, string data) : base(CreateUri(type, data), UriKind.Absolute)
            {
                this.data = data;
            }

            public Uri? this[IFormatObject value] {
                get {
                    var type = value.MediaType;
                    return type == null ? null : new DataUri(type, data);
                }
            }

            static string CreateUri(string? type, string data)
            {
                var sb = new StringBuilder("data:");
                sb.Append(type);
                sb.Append(data);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates a new urn:publicid: URI from a PUBLIC identifier,
        /// according to RFC 3151.
        /// </summary>
        /// <param name="id">The PUBLIC identifier to encode.</param>
        /// <returns>The URI encoding <paramref name="id"/>.</returns>
        public static Uri CreatePublicId(string id)
        {
            return new Uri("urn:" + publicid + TranscribePublicId(id), UriKind.Absolute);
        }

        static string TranscribePublicId(string id)
        {
            return pubIdRegex.Replace(id, m => {
                if(m.Groups[1].Success)
                {
                    return "";
                }else if(m.Groups[2].Success)
                {
                    return "+";
                }else if(m.Groups[3].Success)
                {
                    return ":";
                }else if(m.Groups[4].Success)
                {
                    return ";";
                }else{
                    return Uri.EscapeDataString(m.Value);
                }
            });
        }

        static readonly Regex uriPubIdRegex = new(@"(\+)|(:)|(;)|((?:%[a-fA-F0-9]{2})+)", RegexOptions.Compiled);

        /// <summary>
        /// Decodes a PUBLIC identifier from the corresponding urn:publicid: URI,
        /// encoded according to RFC 3151.
        /// </summary>
        /// <param name="uri">The urn:publicid: URI.</param>
        /// <returns>The identifier stored in <paramref name="uri"/>, or <see langword="null"/>.</returns>
        public static string? ExtractPublicId(Uri uri)
        {
            if(uri.IsAbsoluteUri && uri.Scheme == "urn" && String.IsNullOrEmpty(uri.Fragment))
            {
                var path = uri.AbsolutePath;
                if(path.StartsWith(publicid))
                {
                    path = path.Substring(publicid.Length);
                    return uriPubIdRegex.Replace(path, m => {
                        if(m.Groups[1].Success)
                        {
                            return " ";
                        }else if(m.Groups[2].Success)
                        {
                            return "//";
                        }else if(m.Groups[3].Success)
                        {
                            return "::";
                        }else{
                            return Uri.UnescapeDataString(m.Value);
                        }
                    });
                }
            }
            return null;
        }

        static readonly Regex urlRegex = new(@"%[a-f0-9]{2}|\+", RegexOptions.Compiled);

        /// <summary>
        /// Percent-encodes <paramref name="bytes"/> and returns it as a string.
        /// The encoding follows that of <see cref="HttpUtility.UrlEncode(byte[], int, int)"/>,
        /// but the space character is encoded as %20 and hex digits are uppercase.
        /// </summary>
        /// <param name="bytes">The byte sequence to encode.</param>
        /// <returns>
        /// The encoded string, with each character invalid in a URI encoded.
        /// </returns>
        public static string EscapeDataBytes(ArraySegment<byte> bytes)
        {
            return EscapeDataBytes(bytes.Array, bytes.Offset, bytes.Count);
        }

        /// <summary>
        /// Percent-encodes <paramref name="bytes"/> and returns it as a string.
        /// The encoding follows that of <see cref="HttpUtility.UrlEncode(byte[], int, int)"/>,
        /// but the space character is encoded as %20 and hex digits are uppercase.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <param name="offset">The position in the array to start encoding.</param>
        /// <param name="length">The number of bytes to encode.</param>
        /// <returns>
        /// The encoded string, with each character invalid in a URI encoded.
        /// </returns>
        public static string EscapeDataBytes(byte[] bytes, int offset, int length)
        {
            return urlRegex.Replace(HttpUtility.UrlEncode(bytes, offset, length), m => {
                if(m.Value == "+") return "%20";
                return m.Value.ToUpperInvariant();
            });
        }

        static readonly Regex urlPathRegex = new(@"[^!$&-;=@A-Z_a-z~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+", RegexOptions.Compiled);

        /// <summary>
        /// Escapes characters in <paramref name="uriString"/> that are invalid in a URI path
        /// component or URI in general.
        /// </summary>
        /// <param name="uriString">The string to escape.</param>
        /// <returns>
        /// The escaped string.
        /// </returns>
        public static string EscapePathString(string uriString)
        {
            return urlPathRegex.Replace(uriString, m => {
                return Uri.EscapeDataString(m.Value);
            });
        }

        /// <summary>
        /// A formatter producing urn:oid: URIs from instances of <see cref="Oid"/>.
        /// </summary>
        public static readonly IGenericUriFormatter<Oid> OidUriFormatter = new OidUriFormatterClass();

        class OidUriFormatterClass : IGenericUriFormatter<Oid>
        {
            public Uri this[Oid value] => new("urn:oid:" + value.Value, UriKind.Absolute);
        }

        /// <summary>
        /// Creates a new urn:uuid: URI from a UUID stored as <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The UUID to store.</param>
        /// <returns>The resulting urn:uuid: URI encoding <paramref name="guid"/>.</returns>
        public static Uri CreateUuid(Guid guid)
        {
            return new Uri("urn:uuid:" + guid.ToString("D"), UriKind.Absolute);
        }

        static string ShortenUriPart(string str, int maxPartLength)
        {
            if(str.Length > maxPartLength)
            {
                var first = maxPartLength / 2;
                var last = maxPartLength - first - 1;
                return $"{str.Substring(0, first)}\u2026{str.Substring(str.Length - last)}";
            }
            return str;
        }

        /// <summary>
        /// Shortens individual parts of <paramref name="uri"/>, specifically
        /// <see cref="UriBuilder.Path"/>, <see cref="UriBuilder.Query"/>,
        /// and <see cref="UriBuilder.Fragment"/>.
        /// </summary>
        /// <param name="uri">The URI to shorten.</param>
        /// <param name="maxPartLength">The maximum length of each individual component.</param>
        /// <param name="additionalFragment">Additional text to append after <see cref="Uri.Fragment"/>.</param>
        /// <returns>The shortened URI.</returns>
        public static Uri ShortenUri(Uri uri, int maxPartLength, string additionalFragment)
        {
            var builder = new UriBuilder(uri);
            builder.Path = ShortenUriPart(builder.Path, maxPartLength);
            builder.Query = ShortenUriPart(builder.Query, maxPartLength);
            builder.Fragment = ShortenUriPart(builder.Fragment, maxPartLength) + additionalFragment;
            return builder.Uri;
        }

        static readonly byte[] urlNamespace = { 0x6b, 0xa7, 0xb8, 0x11, 0x9d, 0xad, 0x11, 0xd1, 0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8 };

        /// <summary>
        /// Creates a v5 urn:uuid: URI representing <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI to convert to the UUID.</param>
        /// <returns>The encoded urn:uuid: URI created from the UUID as the result of <see cref="UuidFromUri(Uri)"/>.</returns>
        public static Uri UriToUuidUri(Uri uri)
        {
            return CreateUuid(UuidFromUri(uri));
        }

        /// <summary>
        /// Creates a v5 UUID representing <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI to convert to the UUID.</param>
        /// <returns>A v5 (SHA-1) based UUID, created using <see cref="DataTools.GuidFromName(byte[], string)"/>.</returns>
        public static Guid UuidFromUri(Uri uri)
        {
            return DataTools.GuidFromName(urlNamespace, uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString);
        }

        /// <summary>
        /// Produces a URI that is located under <paramref name="uri"/>
        /// with a specific local name stored in <paramref name="component"/>.
        /// </summary>
        /// <param name="uri">The parent URI.</param>
        /// <param name="component">The name of the component under <paramref name="uri"/>.</param>
        /// <param name="formRoot">
        /// Whether the component should form a new hierarchy. If <see langword="true"/>,
        /// <paramref name="uri"/> and <paramref name="component"/> may be
        /// joined with <c>#/</c>.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Uri"/>, created by joining
        /// <paramref name="uri"/> and <paramref name="component"/>
        /// with <c>#</c>, <c>/</c> or <c>#/</c>, depending on their syntax
        /// and the value of <paramref name="formRoot"/>.
        /// </returns>
        public static Uri MakeSubUri(Uri uri, string component, bool formRoot = true)
        {
            string prefix;
            if(component.StartsWith("#"))
            {
                if(String.IsNullOrEmpty(uri.Fragment))
                {
                    prefix = "";
                }else{
                    prefix = "/";
                    component = component.Substring(1);
                }
            }else if(String.IsNullOrEmpty(uri.Fragment) && (String.IsNullOrEmpty(uri.Authority) || !String.IsNullOrEmpty(uri.Query)))
            {
                prefix = (component.StartsWith("/") || !formRoot) ? "#" : "#/";
            }else{
                prefix = component.StartsWith("/") ? "" : "/";
            }
            return new EncodedUri(uri.AbsoluteUri + prefix + component);
        }
    }
}
