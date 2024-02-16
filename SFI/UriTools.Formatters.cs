using IS4.SFI.Services;
using System;
using System.Security.Cryptography;

namespace IS4.SFI
{
    partial class UriTools
    {
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

        /// <summary>
        /// A formatter which uses <see cref="CreatePublicId(string)"/> to convert
        /// <c>PUBLIC</c> identifiers to <c>urn:publicid:</c>.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> PublicIdFormatter = new PublicIdFormatterClass();
        
        /// <summary>
        /// A formatter which uses <see cref="CreateDataUri(string?, string?, ArraySegment{byte})"/>
        /// to create a <c>data:</c> URI from its components.
        /// </summary>
        public static readonly IIndividualUriFormatter<(string? mediaType, string? charset, ArraySegment<byte> data)> DataUriFormatter = new DataUriFormatterClass();

        /// <summary>
        /// A formatter producing <c>urn:oid:</c> URIs from instances of <see cref="Oid"/>.
        /// </summary>
        public static readonly IGenericUriFormatter<Oid> OidUriFormatter = new OidUriFormatterClass();

        /// <summary>
        /// A formatter producing <c>urn:uuid:</c> URIs from instances of <see cref="Guid"/>.
        /// </summary>
        public static readonly IGenericUriFormatter<Guid> UuidUriFormatter = new UuidUriFormatterClass();

        class PublicIdFormatterClass : IIndividualUriFormatter<string>
        {
            public Uri this[string value] => CreatePublicId(value);
        }

        class DataUriFormatterClass : IIndividualUriFormatter<(string?, string?, ArraySegment<byte>)>
        {
            public Uri this[(string?, string?, ArraySegment<byte>) value] {
                get {
                    var (mediaType, charset, bytes) = value;
                    return CreateDataUri(mediaType, charset, bytes);
                }
            }
        }

        class OidUriFormatterClass : IGenericUriFormatter<Oid>
        {
            public Uri this[Oid value] => new("urn:oid:" + value.Value, UriKind.Absolute);
        }
        
        class UuidUriFormatterClass : IGenericUriFormatter<Guid>
        {
            public Uri this[Guid value] => CreateUuid(value);
        }
    }
}
