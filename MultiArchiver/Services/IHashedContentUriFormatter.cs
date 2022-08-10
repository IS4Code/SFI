using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IHashedContentUriFormatter : IIndividualUriFormatter<(IHashAlgorithm algorithm, byte[] hash, bool isBinary)>
    {
        IEnumerable<IHashAlgorithm> SupportedAlgorithms { get; }
        int? EstimateUriSize(IHashAlgorithm algorithm, int hashSize);
    }

    public class AdHashedContentUriFormatter : IHashedContentUriFormatter
    {
        public IEnumerable<IHashAlgorithm> SupportedAlgorithms { get; }

        public Uri this[(IHashAlgorithm, byte[], bool) value] {
            get {
                var (algorithm, hash, _) = value;
                if(!(algorithm.NumericIdentifier is int id))
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

        public int? EstimateUriSize(IHashAlgorithm algorithm, int hashSize)
        {
            if(algorithm.NumericIdentifier is int id)
            {
                var identifier = DataTools.EncodeMultihash((ulong)id, Array.Empty<byte>(), hashSize);
                return Vocabularies.Uri.Ad.Length + (int)Math.Ceiling((hashSize + identifier.Count) * log58byte);
            }
            return null;
        }

        public AdHashedContentUriFormatter(params IHashAlgorithm[] supportedAlgorithms) : this((IEnumerable<IHashAlgorithm>)supportedAlgorithms)
        {

        }

        public AdHashedContentUriFormatter(IEnumerable<IHashAlgorithm> supportedAlgorithms)
        {
            SupportedAlgorithms = supportedAlgorithms;
        }
    }

    public class NiHashedContentUriFormatter : IHashedContentUriFormatter
    {
        public IEnumerable<IHashAlgorithm> SupportedAlgorithms { get; }

        public Uri this[(IHashAlgorithm, byte[], bool) value] {
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

        public NiHashedContentUriFormatter(params IHashAlgorithm[] supportedAlgorithms) : this((IEnumerable<IHashAlgorithm>)supportedAlgorithms)
        {

        }

        public NiHashedContentUriFormatter(IEnumerable<IHashAlgorithm> supportedAlgorithms)
        {
            SupportedAlgorithms = supportedAlgorithms;
        }

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
                sb.Append(type);
                return sb.ToString();
            }

            public Uri this[IFormatObject value] {
                get {
                    var type = value.MediaType;
                    return type == null ? null : new NiUri(hashName, hashValue, value.MediaType);
                }
            }
        }
    }
}
