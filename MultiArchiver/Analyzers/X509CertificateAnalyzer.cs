using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of X.509 certificates, expressed as instances of <see cref="X509Certificate"/>.
    /// </summary>
    public class X509CertificateAnalyzer : MediaObjectAnalyzer<X509Certificate>
    {
        /// <summary>
        /// Creates a new instance of the analyzer.
        /// </summary>
        public X509CertificateAnalyzer() : base(Classes.X509Certificate)
        {

        }

        public override ValueTask<AnalysisResult> Analyze(X509Certificate certificate, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);

            node.Set(Properties.Notation, certificate.ToString());

            var hash = certificate.GetCertHash();
            Services.HashAlgorithm.AddHash(node, Services.HashAlgorithm.FromLength(hash.Length), hash, context.NodeFactory);

            return new ValueTask<AnalysisResult>(new AnalysisResult(node));
        }
    }
}
