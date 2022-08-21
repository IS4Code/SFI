using IS4.MultiArchiver.Media.Packages;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzes instances of <see cref="PackageDescription"/>, describing a directory or archive
    /// storing its description as metadata.
    /// </summary>
    public class PackageDescriptionAnalyzer : EntityAnalyzer, IEntityAnalyzer<PackageDescription>
    {
        public ValueTask<AnalysisResult> Analyze(PackageDescription desc, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            node.Set(Properties.Description, desc.Value);
            return new ValueTask<AnalysisResult>(default(AnalysisResult));
        }
    }
}
