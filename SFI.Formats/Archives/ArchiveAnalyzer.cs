using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

            var info = new ArchiveRoot(archive, archive as IFileNodeInfo);

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
            readonly IFileNodeInfo? baseInfo;

            public ArchiveRoot(IArchiveFile archive, IFileNodeInfo? baseInfo)
            {
                this.archive = archive;
                this.baseInfo = baseInfo;
            }

            public override IEnumerable<IFileNodeInfo> Entries => archive.Entries;

            public override object? ReferenceKey => archive;

            public override object? DataKey => null;

            public override FileAttributes Attributes => baseInfo?.Attributes ?? base.Attributes;

            public override DateTime? CreationTime => baseInfo?.CreationTime ?? base.CreationTime;

            public override DateTime? LastAccessTime => baseInfo?.LastAccessTime ?? base.LastAccessTime;

            public override DateTime? LastWriteTime => baseInfo?.LastWriteTime ?? base.LastWriteTime;

            public override string? Name => baseInfo?.Name ?? base.Name;

            public override string Path => baseInfo?.Path ?? base.Path;

            public override int? Revision => baseInfo?.Revision ?? base.Revision;

            public override string? SubName => baseInfo?.SubName ?? base.SubName;
        }
    }
}
