using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of X.509 certificates, expressed as instances of <see cref="X509Certificate"/>.
    /// </summary>
    public class X509CertificateAnalyzer : MediaObjectAnalyzer<X509Certificate>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public X509CertificateAnalyzer() : base(Classes.X509Certificate)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(X509Certificate certificate, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            node.Set(Properties.Notation, certificate.ToString());

            var hash = certificate.GetCertHash();
            Services.HashAlgorithm.AddHash(node, Services.HashAlgorithm.FromLength(hash.Length), hash, context.NodeFactory);

            return new AnalysisResult(node);
        }
    }
}
