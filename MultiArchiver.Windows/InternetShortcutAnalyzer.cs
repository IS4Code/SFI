using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Threading.Tasks;
using static Vanara.PInvoke.Url;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of URL shortcuts, as instances of <see cref="IUniformResourceLocator"/>.
    /// </summary>
    public class InternetShortcutAnalyzer : MediaObjectAnalyzer<IUniformResourceLocator>
    {
        public override ValueTask<AnalysisResult> Analyze(IUniformResourceLocator shortcut, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            shortcut.GetUrl(out var url);
            node.Set(Properties.Links, UriFormatter.Instance, url);
            return new ValueTask<AnalysisResult>(new AnalysisResult(node));
        }
    }
}
