using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;
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

            var info = new ArchiveRoot(archive);

            var result = globalAnalyzer.Analyze(info, context.WithNode(node));

            if(result.Exception.Is<CryptographicException>())
            {
                node.Set(Properties.EncryptionStatus, Individuals.EncryptedStatus);
            }
            result.Node = node;

            return result;
        }

        class ArchiveRoot : RootDirectoryInfo
        {
            readonly IArchiveFile archive;

            public ArchiveRoot(IArchiveFile archive)
            {
                this.archive = archive;
            }

            public override IEnumerable<IFileNodeInfo> Entries => archive.Entries;

            public override object ReferenceKey => archive;

            public override object DataKey => null;
        }
    }
}
