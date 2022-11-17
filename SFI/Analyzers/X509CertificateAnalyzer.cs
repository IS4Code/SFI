using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Globalization;
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
        public async override ValueTask<AnalysisResult> Analyze(X509Certificate cert, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            node.Set(Properties.Notation, cert.ToString());

            var hash = cert.GetCertHash();
            await HashAlgorithm.AddHash(node, HashAlgorithm.FromLength(hash.Length), hash, context.NodeFactory, OnOutputFile);

            if(!String.IsNullOrWhiteSpace(cert.Subject))
            {
                node.Set(Properties.Subject, cert.Subject);
            }
            if(!String.IsNullOrWhiteSpace(cert.Issuer))
            {
                node.Set(Properties.Creator, cert.Issuer);
            }

            if(cert is X509Certificate2 cert2)
            {
                if(!String.IsNullOrWhiteSpace(cert2.FriendlyName))
                {
                    node.Set(Properties.Name, cert2.FriendlyName);
                }
                node.Set(Properties.Created, cert2.NotBefore);
                node.Set(Properties.Expiration, cert2.NotAfter);

                var language = new LanguageCode(CultureInfo.InstalledUICulture);
                foreach(var extension in cert2.Extensions)
                {
                    var value = extension.Format(false);
                    node.Set(UriTools.OidUriFormatter, extension.Oid, value, language);
                }
                foreach(var extension in cert2.Extensions)
                {
                    var propNode = context.NodeFactory?.Create(UriTools.OidUriFormatter, extension.Oid);
                    if(propNode != null)
                    {
                        propNode.Set(Properties.Label, extension.Oid.FriendlyName, language);
                    }
                }
            }

            return new AnalysisResult(node);
        }
    }
}
