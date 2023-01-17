using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of HTTP request messages, as instances of <see cref="HttpRequestMessage"/>.
    /// </summary>
    public class HttpRequestAnalyzer : HttpMessageAnalyzer<HttpRequestMessage>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public HttpRequestAnalyzer() : base(Classes.HttpRequest, Classes.Message)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(HttpRequestMessage message, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var method = message.Method.Method.ToUpperInvariant();
            node.Set(Properties.HttpMethodName, method);
            node.Set(Properties.HttpMethod, Vocabularies.Httpm, method);

            var target = message.RequestUri;
            if(target.IsAbsoluteUri)
            {
                node.Set(Properties.HttpAbsoluteUri, target);
            }else{
                var targetText = target.OriginalString;
                if(targetText.StartsWith("/"))
                {
                    node.Set(Properties.HttpAbsolutePath, targetText, Datatypes.AnyUri);
                }else if(targetText.IndexOf("/", StringComparison.Ordinal) == -1 && Uri.IsWellFormedUriString("http://" + targetText, UriKind.Absolute))
                {
                    node.Set(Properties.HttpAuthority, targetText);
                }
            }

            node.Set(Properties.HttpVersion, message.Version.ToString(2));

            return new AnalysisResult(node);
        }
    }
}
