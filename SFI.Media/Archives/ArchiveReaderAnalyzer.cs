using IS4.SFI.Media;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes archives as instances of <see cref="IArchiveReader"/>.
    /// </summary>
    public class ArchiveReaderAnalyzer : MediaObjectAnalyzer<IArchiveReader>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ArchiveReaderAnalyzer() : base(Common.ArchiveClasses)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IArchiveReader reader, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            try{
                while(reader.MoveNext())
                {
                    var entry = reader.Current!;
                    var entryNode = (await analyzers.Analyze(entry, context.WithNode(node[entry.Path]))).Node;
                    if(entryNode != null && (entry.Path == null || !entry.Path.Contains("/")))
                    {
                        entryNode.SetClass(Classes.ArchiveItem);
                        entryNode.Set(Properties.BelongsToContainer, node);
                    }
                }
            }catch(CryptographicException)
            {
                node.Set(Properties.EncryptionStatus, Individuals.EncryptedStatus);
            }

            return new AnalysisResult(node);
        }
    }
}
