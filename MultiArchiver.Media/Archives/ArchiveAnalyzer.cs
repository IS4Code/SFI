using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Security.Cryptography;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : MediaObjectAnalyzer<IArchiveFile>
    {
        public ArchiveAnalyzer() : base(Common.ArchiveClasses)
        {

        }

        public override AnalysisResult Analyze(IArchiveFile archive, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);

            try{
                foreach(var entry in archive.Entries)
                {
                    var entryNode = globalAnalyzer.Analyze(entry, context).Node;
                    if(entryNode != null)
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
