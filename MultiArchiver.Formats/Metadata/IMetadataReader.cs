using IS4.MultiArchiver.Services;
using MetadataExtractor;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats.Metadata
{
    /// <summary>
    /// Represents a reader capable of interpreting metadata for
    /// a specific type of <see cref="Directory"/>.
    /// </summary>
    /// <typeparam name="T">The supported type.</typeparam>
    public interface IMetadataReader<in T> where T : Directory
    {
        /// <summary>
        /// Describes the instance of <typeparamref name="T"/>, using the supplied node.
        /// </summary>
        /// <param name="node">The node for collecting the description of the metadata.</param>
        /// <param name="directory">The specific metadata directory.</param>
        /// <param name="context">The context of the analysis.</param>
        /// <param name="analyzers">The collection of analyzers for nested objects.</param>
        /// <returns>The label of the directory.</returns>
        ValueTask<string> Describe(ILinkedNode node, T directory, AnalysisContext context, IEntityAnalyzers analyzers);
    }
}
