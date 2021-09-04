using IS4.MultiArchiver.Media.Packages;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;

namespace IS4.MultiArchiver.Analyzers
{
    public class PackageDescriptionAnalyzer : EntityAnalyzer, IEntityAnalyzer<PackageDescription>
    {
        public AnalysisResult Analyze(PackageDescription desc, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);
            node.Set(Properties.Description, desc.Value);
            return default;
        }
    }
}
