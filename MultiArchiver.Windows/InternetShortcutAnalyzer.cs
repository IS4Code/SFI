using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using static Vanara.PInvoke.Ole32;
using static Vanara.PInvoke.Url;

namespace IS4.MultiArchiver.Analyzers
{
    public class InternetShortcutAnalyzer : MediaObjectAnalyzer<IUniformResourceLocator>
    {
        static readonly Guid FMTID_Intshcut = Guid.Parse("000214A0-0000-0000-C000-000000000046");
        static readonly PROPSPEC[] PID_IS_URL = { new PROPSPEC(2) };

        public override AnalysisResult Analyze(IUniformResourceLocator shortcut, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);
            shortcut.GetUrl(out var url);
            node.Set(Properties.Links, UriFormatter.Instance, url);
            return new AnalysisResult(node);
        }
    }
}
