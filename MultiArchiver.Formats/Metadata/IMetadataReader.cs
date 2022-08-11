using IS4.MultiArchiver.Services;
using MetadataExtractor;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats.Metadata
{
    public interface IMetadataReader<in T> where T : Directory
    {
        ValueTask<string> Describe(ILinkedNode node, T directory, AnalysisContext context, IEntityAnalyzers analyzers);
    }
}
