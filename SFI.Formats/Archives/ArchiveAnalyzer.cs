using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes archives as instances of <see cref="IArchiveFile"/>.
    /// </summary>
    [Description("Analyzes archives.")]
    public class ArchiveAnalyzer : MediaObjectAnalyzer<IArchiveFile>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ArchiveAnalyzer() : base(Common.ArchiveClasses)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IArchiveFile archive, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var info = new ArchiveRoot(archive);

            var result = await analyzers.Analyze(info, context.WithNode(node));

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

            public override object? ReferenceKey => archive;

            public override object? DataKey => null;
        }
    }
}
