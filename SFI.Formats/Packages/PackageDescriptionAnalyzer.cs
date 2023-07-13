using IS4.SFI.Formats.Packages;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes instances of <see cref="PackageDescription"/>, describing a directory or archive
    /// storing its description as metadata.
    /// </summary>
    [Description("Analyzes package descriptions describing a directory or archive storing its description as metadata.")]
    public class PackageDescriptionAnalyzer : EntityAnalyzer<PackageDescription>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(PackageDescription desc, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            node.Set(Properties.Description, desc.Value);
            return default(AnalysisResult);
        }
    }
}
