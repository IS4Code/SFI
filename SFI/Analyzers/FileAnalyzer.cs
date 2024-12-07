using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of files and directories, as instances of <see cref="IFileNodeInfo"/>,
    /// <see cref="IFileInfo"/>, <see cref="IDirectoryInfo"/>, <see cref="FileInfo"/> or
    /// <see cref="DirectoryInfo"/>.
    /// </summary>
    [Description("An analyzer of files and directories.")]
    public sealed class FileAnalyzer : EntityAnalyzer<IFileNodeInfo>, IEntityAnalyzer<FileInfo>, IEntityAnalyzer<DirectoryInfo>, IEntityAnalyzer<DriveInfo>, IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<IDirectoryInfo>
    {
        /// <summary>
        /// A collection of used hash algorithms, as instances of <see cref="IFileHashAlgorithm"/>,
        /// whose output is used to describe the file object.
        /// </summary>
        [ComponentCollection("file-hash")]
        public ICollection<IFileHashAlgorithm> HashAlgorithms { get; } = new List<IFileHashAlgorithm>();

        /// <summary>
        /// Contains names of files that will be ignored when encountered in a directory.
        /// </summary>
        [Description("Contains names of files that will be ignored when encountered in a directory.")]
        public ICollection<string> IgnoredFiles { get; } = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Contains case-insensitive names of files that will be ignored when encountered in a directory.
        /// </summary>
        [Description("Contains case-insensitive names of files that will be ignored when encountered in a directory.")]
        public ICollection<string> IgnoredFilesCaseInsensitive { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public FileAnalyzer()
        {

        }

        private ILinkedNode CreateNode(IFileNodeInfo info, AnalysisContext context)
        {
            var name = Uri.EscapeDataString(info.Name ?? "");
            if(info.SubName is string subName)
            {
                name += ":" + Uri.EscapeDataString(subName);
            }
            return GetNode(name, context);
        }

        private async ValueTask<ILinkedNode> AnalyzeFileNode(IFileNodeInfo info, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = CreateNode(info, context);

            if(info.Path != "" && info is not IDriveInfo)
            {
                // is not root
                node.SetClass(Classes.FileDataObject);
            }

            switch(info.Kind)
            {
                case FileKind.Embedded:
                    node.SetClass(Classes.EmbeddedFileDataObject);
                    break;
                case FileKind.ArchiveItem:
                    node.SetClass(Classes.ArchiveItem);
                    break;
            }

            if(info.Path != null)
            {
                await AnalyzePath(node, info.Path, false, context, analyzer);
            }else if(info.Name != null)
            {
                await AnalyzePath(node, info.Name, false, context, analyzer);
            }

            var label = info.ToString();
            if(!String.IsNullOrEmpty(label))
            {
                node.Set(Properties.PrefLabel, DataTools.ReplaceControlCharacters(label, null));
            }

            if(info.Name != null)
            {
                node.Set(Properties.FileName, info.Name);
            }

            if(info.CreationTime is DateTime dt1)
            {
                node.Set(Properties.FileCreated, dt1);
            }
            if(info.LastWriteTime is DateTime dt2)
            {
                node.Set(Properties.FileLastModified, dt2);
            }
            if(info.LastAccessTime is DateTime dt3)
            {
                node.Set(Properties.FileLastAccessed, dt3);
            }
            if(info.Revision is int rev)
            {
                node.Set(Properties.Version, rev);
            }

            if(info is IDriveInfo drive)
            {
                if(drive.OccupiedSpace is long occupiedSpace)
                {
                    node.Set(Properties.OccupiedSpace, occupiedSpace);
                }
                if(drive.TotalFreeSpace is long freeSpace)
                {
                    node.Set(Properties.FreeSpace, freeSpace);
                }
                if(drive.TotalSize is long totalSize)
                {
                    node.Set(Properties.TotalSpace, totalSize);
                }
                if(drive.VolumeLabel is string volumeLabel)
                {
                    node.Set(Properties.VolumeLabel, volumeLabel);
                }
                if(drive.DriveFormat is string format)
                {
                    node.Set(Properties.FilesystemType, format);
                }
            }

            return node;
        }

        /// <inheritdoc/>
        public async ValueTask<AnalysisResult> Analyze(IFileInfo file, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = await AnalyzeFileNode(file, context, analyzer);
            if(node != null)
            {
                if(file.Length >= 0)
                {
                    node.Set(Properties.FileSize, file.Length);
                }

                if((file.Attributes & FileAttributes.Encrypted) != 0)
                {
                    node.Set(Properties.EncryptionStatus, Individuals.EncryptedStatus);
                }else{
                    await analyzer.Analyze<IStreamFactory>(file, context.WithParentLink(node, Properties.InterpretedAs));
                }

                foreach(var alg in HashAlgorithms)
                {
                    if(alg.Accepts(file))
                    {
                        await HashAlgorithm.AddHash(node, alg, await alg.ComputeHash(file), context.NodeFactory, OnOutputFile);
                    }
                }

                if(file is IDirectoryInfo directory)
                {
                    await AnalyzeDirectory(node, directory, context, analyzer);
                }

                node.SetAsBase();
            }
            return new AnalysisResult(node);
        }

        private async ValueTask AnalyzeDirectory(ILinkedNode node, IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var folder = await AnalyzeContents(node, directory, context, analyzer);

            if(folder != null)
            {
                node.Set(Properties.InterpretedAs, folder);
            }

            foreach(var alg in HashAlgorithms)
            {
                if(alg.Accepts(directory, false))
                {
                    await HashAlgorithm.AddHash(node, alg, await alg.ComputeHash(directory, false), context.NodeFactory, OnOutputFile);
                }
            }
        }

        /// <inheritdoc/>
        public async ValueTask<AnalysisResult> Analyze(IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = await AnalyzeFileNode(directory, context, analyzer);
            if(node != null)
            {
                await AnalyzeDirectory(node, directory, context, analyzer);
            }
            return new AnalysisResult(node);
        }

        private async ValueTask<ILinkedNode?> AnalyzeContents(ILinkedNode? parent, IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var folder = parent?[""] ?? context.NodeFactory.CreateUnique();

            if(folder != null)
            {
                folder.SetClass(Classes.Folder);

                parent?.Set(Properties.InterpretedAs, folder);

                folder.Set(Properties.PrefLabel, DataTools.ReplaceControlCharacters(directory.ToString() + "/", null));

                await AnalyzePath(folder, directory.Path, true, context, analyzer);

                foreach(var alg in HashAlgorithms)
                {
                    if(alg.Accepts(directory, true))
                    {
                        await HashAlgorithm.AddHash(folder, alg, await alg.ComputeHash(directory, true), context.NodeFactory, OnOutputFile);
                    }
                }

                context = context.WithParent(parent);

                foreach(var entry in directory.Entries)
                {
                    var entryName = entry.Name;
                    if(entryName is not null && (IgnoredFiles.Contains(entryName) || IgnoredFilesCaseInsensitive.Contains(entryName)))
                    {
                        continue;
                    }
                    var entryNode = CreateNode(entry, context);
                    entryNode.Set(Properties.BelongsToContainer, folder);
                    await analyzer.Analyze(entry, context.WithNode(entryNode));
                }
            }

            return folder;
        }

        private async ValueTask AnalyzePath(ILinkedNode initial, string? path, bool directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            if(path == null) return;

            var pathObject = new PathObject(path + (directory ? "/" : ""));

            await analyzer.Analyze(pathObject, context.WithParentLink(initial, Properties.PathObject));
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(FileInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            return analyzer.Analyze<IFileInfo>(new FileInfoWrapper(entity), context);
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(DirectoryInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            return analyzer.Analyze<IDirectoryInfo>(new DirectoryInfoWrapper(entity), context);
        }

        /// <inheritdoc/>
        public ValueTask<AnalysisResult> Analyze(DriveInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            return analyzer.Analyze<IDriveInfo>(new DriveInfoWrapper(entity), context);
        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IFileNodeInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            switch(entity)
            {
                case IFileInfo file: return await Analyze(file, context, analyzer);
                case IDirectoryInfo dir: return await Analyze(dir, context, analyzer);
                default: return new AnalysisResult(CreateNode(entity, context));
            }
        }
    }
}
