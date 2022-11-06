using IS4.MultiArchiver.Formats.Modules;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of MS-DOS modules, as instances of <see cref="DosModule"/>.
    /// </summary>
    public class DosModuleAnalyzer : MediaObjectAnalyzer<DosModule>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public DosModuleAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        public async override ValueTask<AnalysisResult> Analyze(DosModule module, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var uncompressed = module.GetCompressedContents();
            if(uncompressed != null)
            {
                var infoNode = (await analyzers.Analyze<IFileInfo>(uncompressed, context.WithParent(node))).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.BelongsToContainer, infoNode);
                }
            }
            return new AnalysisResult(node);
        }
    }
}
