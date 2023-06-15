using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Threading.Tasks;
using static Vanara.PInvoke.Url;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of URL shortcuts, as instances of <see cref="IUniformResourceLocator"/>.
    /// </summary>
    public class InternetShortcutAnalyzer : MediaObjectAnalyzer<IUniformResourceLocator>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IUniformResourceLocator shortcut, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            shortcut.GetUrl(out var url);
            node.Set(Properties.Links, UriFormatter.Instance, url);
            return new AnalysisResult(node);
        }
    }
}
