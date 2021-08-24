using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Security.Cryptography;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveReaderAnalyzer : MediaObjectAnalyzer<IArchiveReader>
    {
        public ArchiveReaderAnalyzer() : base(Common.ArchiveClasses)
        {

        }

        public override AnalysisResult Analyze(IArchiveReader reader, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);

            try{
                while(reader.MoveNext())
                {
                    var entry = reader.Current;
                    var entryNode = globalAnalyzer.Analyze(entry, context.WithNode(node[entry.Path])).Node;
                    if(!entry.Path.Contains("/"))
                    {
                        entryNode?.Set(Properties.BelongsToContainer, node);
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
