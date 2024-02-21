using IS4.SFI.Services;
using System;
using System.Collections;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace IS4.SFI
{
    partial class UriTools
    {
        /// <summary>
        /// A URI formatter which uses a prefix to prepend its argument when forming
        /// the resulting URI.
        /// </summary>
        public class PrefixFormatter<T> : IUniversalUriFormatter<T>
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

        /// <summary>
        /// A formatter which uses <see cref="CreatePublicId(string)"/> to convert
        /// <c>PUBLIC</c> identifiers to <c>urn:publicid:</c>.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> PublicIdFormatter = new PublicIdFormatterClass();

        class PublicIdFormatterClass : IIndividualUriFormatter<string>
        {
            public Uri this[string value] => CreatePublicId(value);
        }

        /// <summary>
        /// A formatter which uses <see cref="CreateDataUri(string?, string?, ArraySegment{byte})"/>
        /// to create a <c>data:</c> URI from its components.
        /// </summary>
        public static readonly IMediaTypeBasedUriFormatter<ArraySegment<byte>> DataUriFormatter = TypedFormatterClass.Instance;

        /// <summary>
        /// An implementation of <see cref="IIndividualUriFormatter{T}"/> based
        /// on a media type.
        /// </summary>
        /// <typeparam name="T">Additional data relevant to the formatter.</typeparam>
        public interface IMediaTypeBasedUriFormatter<T> :
            IIndividualUriFormatter<(string? mediaType, string? charset, T data)>,
            IIndividualUriFormatter<(ContentType? mediaType, T data)>,
            IIndividualUriFormatter<(IFormatObject? formatObject, T data)>
        {

        }

        /// <summary>
        /// An implementation of <see cref="IIndividualUriFormatter{T}"/> for
        /// formatting media types.
        /// </summary>
        public interface IMediaTypeUriFormatter :
            IMediaTypeBasedUriFormatter<ValueTuple>,
            IIndividualUriFormatter<(string? mediaType, string? charset)>,
            IIndividualUriFormatter<ContentType>,
            IIndividualUriFormatter<IFormatObject>
        {

        }

        /// <summary>
        /// A formatter producing <c>urn:oid:</c> URIs from instances of <see cref="Oid"/>.
        /// </summary>
        public static readonly IUniversalUriFormatter<Oid> OidUriFormatter = TypedFormatterClass.Instance;

        /// <summary>
        /// A formatter producing <c>urn:uuid:</c> URIs from instances of <see cref="Guid"/>.
        /// </summary>
        public static readonly IUniversalUriFormatter<Guid> UuidUriFormatter = TypedFormatterClass.Instance;

        /// <summary>
        /// A formatter producing generic URIs from instances of <see cref="XmlQualifiedName"/>.
        /// </summary>
        public static readonly IUniversalUriFormatter<XmlQualifiedName> QNameFormatter = TypedFormatterClass.Instance;

        sealed class TypedFormatterClass :
            IMediaTypeBasedUriFormatter<ArraySegment<byte>>,
            IUniversalUriFormatter<Oid>,
            IUniversalUriFormatter<Guid>,
            IUniversalUriFormatter<XmlQualifiedName>
        {
            public static readonly TypedFormatterClass Instance = new();

            public Uri this[(string?, string?, ArraySegment<byte>) value] {
                get {
                    var (mediaType, charset, bytes) = value;
                    return CreateDataUri(mediaType, charset, bytes);
                }
            }

            public Uri this[(IFormatObject?, ArraySegment<byte>) value] {
                get {
                    var (formatObject, bytes) = value;
                    if(formatObject?.MediaType == null)
                    {
                        return CreateDataUri(null, null, bytes);
                    }
                    return this[(new ContentType(formatObject.MediaType), bytes)];
                }
            }

            public Uri this[(ContentType?, ArraySegment<byte>) value] {
                get {
                    var (contentType, bytes) = value;
                    if(contentType == null)
                    {
                        return CreateDataUri(null, null, bytes);
                    }
                    var mediaType = new StringBuilder(contentType.MediaType);
                    foreach(DictionaryEntry param in contentType.Parameters)
                    {
                        if("charset".Equals(param.Key))
                        {
                            continue;
                        }
                        mediaType.Append(';');
                        mediaType.Append(param.Key);
                        mediaType.Append('=');
                        mediaType.Append(param.Value);
                    }
                    return CreateDataUri(mediaType.ToString(), contentType.CharSet, bytes);
                }
            }

            public Uri this[Oid value] => new("urn:oid:" + value.Value, UriKind.Absolute);

            public Uri this[Guid value] => CreateUuid(value);

            public Uri this[XmlQualifiedName value] => QNameToUri(value);
        }
    }
}
