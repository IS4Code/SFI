using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.SFI.Services
{
    /// <summary>
    /// A formatter for creating URIs for resources identified by a particular hash,
    /// via its <see cref="IIndividualUriFormatter{T}"/> implementation.
    /// </summary>
    public interface IHashedContentUriFormatter : IIndividualUriFormatter<(IHashAlgorithm algorithm, byte[] hash, bool isBinary)>
    {
        /// <summary>
        /// Stores the collection of algorithms that are suitable to use
        /// for producing the URI (because they are safe for collisions).
        /// </summary>
        IEnumerable<IHashAlgorithm> SuitableAlgorithms { get; }

        /// <summary>
        /// Estimates the size of the resulting URI from a hash algorithm and its output size.
        /// </summary>
        /// <param name="algorithm">The used hash algorithm.</param>
        /// <param name="hashSize">The size of the hash in bytes.</param>
        /// <returns>The size of the URI in characters.</returns>
        int? EstimateUriSize(IHashAlgorithm algorithm, int hashSize);
    }

    /// <summary>
    /// Formats URIs using their multihash representation,
    /// in the <see cref="Vocabularies.Ad"/> vocabulary.
    /// </summary>
    public class AdHashedContentUriFormatter : IHashedContentUriFormatter
    {
        /// <inheritdoc/>
        public IEnumerable<IHashAlgorithm> SuitableAlgorithms { get; }

        /// <summary>
        /// Creates a new URI from a tuple storing the information about the hash.
        /// </summary>
        /// <param name="value">
        /// A tuple storing the hash algorithm, the hash bytes, and whether
        /// the resource is binary or not.
        /// </param>
        /// <returns>The URI identifying the resource.</returns>
        public Uri? this[(IHashAlgorithm algorithm, byte[] hash, bool isBinary) value] {
            get {
                var (algorithm, hash, _) = value;
                if(algorithm.NumericIdentifier is not int id)
                {
                    return null;
                }

                var identifier = DataTools.EncodeMultihash((ulong)id, hash);

                var sb = new StringBuilder();
                DataTools.Base58(identifier, sb);
                return Vocabularies.Ad[sb.ToString()];
            }
        }

        static readonly double log58byte = Math.Log(256, 58);

        /// <inheritdoc/>
        public int? EstimateUriSize(IHashAlgorithm algorithm, int hashSize)
        {
            if(algorithm.NumericIdentifier is int id)
            {
                var identifier = DataTools.EncodeMultihash((ulong)id, Array.Empty<byte>(), hashSize);
                return Vocabularies.Uri.Ad.Length + (int)Math.Ceiling((hashSize + identifier.Count) * log58byte);
            }
            return null;
        }

        /// <summary>
        /// Creates a new instance of the formatter from a collection of suitable algorithms.
        /// </summary>
        /// <param name="suitableAlgorithms">The collecion of algorithms suitable for using.</param>
        public AdHashedContentUriFormatter(params IHashAlgorithm[] suitableAlgorithms) : this((IEnumerable<IHashAlgorithm>)suitableAlgorithms)
        {

        }

        /// <summary>
        /// Creates a new instance of the formatter from a collection of suitable algorithms.
        /// </summary>
        /// <param name="suitableAlgorithms">The collecion of algorithms suitable for using.</param>
        public AdHashedContentUriFormatter(IEnumerable<IHashAlgorithm> suitableAlgorithms)
        {
            SuitableAlgorithms = suitableAlgorithms;
        }
    }

    /// <summary>
    /// Creates <c>ni:</c> URIs, either using the <see cref="IHashAlgorithm.NiName"/>
    /// or via its multihash representation.
    /// </summary>
    public class NiHashedContentUriFormatter : IHashedContentUriFormatter
    {
        /// <inheritdoc/>
        public IEnumerable<IHashAlgorithm> SuitableAlgorithms { get; }

        /// <summary>
        /// Creates a new URI from a tuple storing the information about the hash.
        /// </summary>
        /// <param name="value">
        /// A tuple storing the hash algorithm, the hash bytes, and whether
        /// the resource is binary or not.
        /// </param>
        /// <returns>The URI identifying the resource.</returns>
        public Uri? this[(IHashAlgorithm algorithm, byte[] hash, bool isBinary) value] {
            get {
                var (algorithm, hash, isBinary) = value;
                var mimeType = isBinary ? "application/octet-stream" : "text/plain";
                if(algorithm.NiName is string name)
                {
                    return new NiUri(name, hash, mimeType);
                }
                if(algorithm.NumericIdentifier is int id)
                {
                    var identifier = DataTools.EncodeMultihash((ulong)id, hash);
                    return new NiUri("mh", identifier.ToArray(), mimeType);
                }
                return null;
            }
        }

        /// <inheritdoc/>
        public int? EstimateUriSize(IHashAlgorithm algorithm, int hashSize)
        {
            if(algorithm.NiName is string name)
            {
                return "ni:///;?ct=".Length + name.Length + (hashSize + 2) / 3 * 4;
            }
            if(algorithm.NumericIdentifier is int id)
            {
                var identifier = DataTools.EncodeMultihash((ulong)id, Array.Empty<byte>(), hashSize);
                return "ni:///mh;?ct=".Length + (hashSize + identifier.Count + 2) / 3 * 4;
            }
            return null;
        }

        /// <summary>
        /// Creates a new instance of the formatter from a collection of suitable algorithms.
        /// </summary>
        /// <param name="suitableAlgorithms">The collecion of algorithms suitable for using.</param>
        public NiHashedContentUriFormatter(params IHashAlgorithm[] suitableAlgorithms) : this((IEnumerable<IHashAlgorithm>)suitableAlgorithms)
        {

        }

        /// <summary>
        /// Creates a new instance of the formatter from a collection of suitable algorithms.
        /// </summary>
        /// <param name="suitableAlgorithms">The collecion of algorithms suitable for using.</param>
        public NiHashedContentUriFormatter(IEnumerable<IHashAlgorithm> suitableAlgorithms)
        {
            SuitableAlgorithms = suitableAlgorithms;
        }

        /// <summary>
        /// The <c>ni:</c> URI supports individual formatting based on an instance of <see cref="IFormatObject"/>
        /// to use instead of the default media type specified as part of the URI (via <c>ct</c>).
        /// </summary>
        class NiUri : Uri, IIndividualUriFormatter<IFormatObject>
        {
            readonly string hashName;
            readonly byte[] hashValue;

            public NiUri(string hashName, byte[] hashValue, string type) : base(CreateUri(hashName, hashValue, type), UriKind.Absolute)
            {
                this.hashName = hashName;
                this.hashValue = hashValue;
            }

            static string CreateUri(string hashName, byte[] hashValue, string type)
            {
                var sb = new StringBuilder("ni:///");
                sb.Append(Uri.EscapeDataString(hashName));
                sb.Append(';');
                DataTools.Base64Url(hashValue, sb);
                sb.Append("?ct=");
                sb.Append(UriTools.EscapeQueryString(type));
                return sb.ToString();
            }

            public Uri? this[IFormatObject value] {
                get {
                    var type = value.MediaType;
                    return type == null ? null : new NiUri(hashName, hashValue, type);
                }
            }
        }
    }
}
