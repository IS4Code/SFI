using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveReaderAnalyzer : MediaObjectAnalyzer<IArchiveReader>
    {
        public ArchiveReaderAnalyzer() : base(Common.ArchiveClasses)
        {

        }

        public override async ValueTask<AnalysisResult> Analyze(IArchiveReader reader, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);

            try{
                while(reader.MoveNext())
                {
                    var entry = reader.Current;
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
