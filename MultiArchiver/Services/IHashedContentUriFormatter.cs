using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.MultiArchiver.Services
{
    public interface IHashedContentUriFormatter : IIndividualUriFormatter<(IHashAlgorithm algorithm, byte[] hash, bool isBinary)>
    {
        IEnumerable<IHashAlgorithm> SupportedAlgorithms { get; }
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
                var identifier = new List<byte>(2 + hash.Length);
                identifier.AddRange(DataTools.Varint((ulong)id));
                identifier.AddRange(DataTools.Varint((ulong)hash.Length));
                identifier.AddRange(hash);

                var sb = new StringBuilder();
                DataTools.Base58(identifier, sb);
                return Vocabularies.Ad[sb.ToString()];
            }
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
                if(!(algorithm.NiName is string name))
                {
                    return null;
                }
                var sb = new StringBuilder("ni:///");
                sb.Append(Uri.EscapeDataString(name));
                sb.Append(';');
                DataTools.Base64(hash, sb);
                sb.Append(isBinary ? "?ct=application/octet-stream" : "?ct=text/plain");
                return new Uri(sb.ToString(), UriKind.Absolute);
            }
        }

        public NiHashedContentUriFormatter(params IHashAlgorithm[] supportedAlgorithms) : this((IEnumerable<IHashAlgorithm>)supportedAlgorithms)
        {

        }

        public NiHashedContentUriFormatter(IEnumerable<IHashAlgorithm> supportedAlgorithms)
        {
            SupportedAlgorithms = supportedAlgorithms;
        }
    }
}
