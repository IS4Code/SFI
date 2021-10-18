using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Security.Cryptography.X509Certificates;

namespace IS4.MultiArchiver.Analyzers
{
    public class X509CertificateAnalyzer : MediaObjectAnalyzer<X509Certificate>
    {
        public X509CertificateAnalyzer() : base(Classes.X509Certificate)
        {

        }

        public override AnalysisResult Analyze(X509Certificate certificate, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);

            node.Set(Properties.Notation, certificate.ToString());

            var hash = certificate.GetCertHash();
            Services.HashAlgorithm.AddHash(node, Services.HashAlgorithm.FromLength(hash.Length), hash, context.NodeFactory);

            return new AnalysisResult(node);
        }
    }
}
