using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Net.Http;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of HTTP response messages, as instances of <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class HttpResponseAnalyzer : HttpMessageAnalyzer<HttpResponseMessage>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public HttpResponseAnalyzer() : base(Classes.HttpResponse, Classes.Message)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(HttpResponseMessage message, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            return new AnalysisResult(node);
        }
    }
}
