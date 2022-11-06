using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of files and directories, as instances of <see cref="IFileNodeInfo"/>,
    /// <see cref="IFileInfo"/>, <see cref="IDirectoryInfo"/>, <see cref="FileInfo"/> or
    /// <see cref="DirectoryInfo"/>.
    /// </summary>
    public sealed class FileAnalyzer : EntityAnalyzer, IEntityAnalyzer<IFileNodeInfo>, IEntityAnalyzer<FileInfo>, IEntityAnalyzer<DirectoryInfo>, IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<IDirectoryInfo>
    {
        /// <summary>
        /// A collection of used hash algorithms, as instances of <see cref="IFileHashAlgorithm"/>,
        /// whose output is used to describe the file object.
        /// </summary>
        public ICollection<IFileHashAlgorithm> HashAlgorithms { get; } = new List<IFileHashAlgorithm>();

        /// <inheritdoc/>
        public FileAnalyzer()
        {

        }

        private ILinkedNode CreateNode(IFileNodeInfo info, AnalysisContext context)
        {
            if(context.Node != null) return context.Node;
            var name = Uri.EscapeDataString(info.Name ?? "");
            if(info.SubName is string subName)
            {
                name += ":" + Uri.EscapeDataString(subName);
            }
            return context.Parent?[name] ?? context.NodeFactory.CreateUnique();
        }

        private ILinkedNode AnalyzeFileNode(IFileNodeInfo info, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = CreateNode(info, context);

            if(info.Path != "")
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
                LinkDirectories(node, info.Path, false, context.NodeFactory);
            }else if(info.Name != null)
            {
                LinkDirectories(node, info.Name, false, context.NodeFactory);
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

            return node;
        }

        public async ValueTask<AnalysisResult> Analyze(IFileInfo file, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = AnalyzeFileNode(file, context, analyzer);
            if(node != null)
            {
                if(file.Length >= 0)
                {
                    node.Set(Properties.FileSize, file.Length);
                }

                if(file.IsEncrypted)
                {
                    node.Set(Properties.EncryptionStatus, Individuals.EncryptedStatus);
                }else{
                    var content = (await analyzer.Analyze<IStreamFactory>(file, context.WithParent(node))).Node;
                    if(content != null)
                    {
                        node.Set(Properties.InterpretedAs, content);
                    }
                }

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(node, alg, await alg.ComputeHash(file), context.NodeFactory);
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
                HashAlgorithm.AddHash(node, alg, await alg.ComputeHash(directory, false), context.NodeFactory);
            }
        }

        public async ValueTask<AnalysisResult> Analyze(IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var node = AnalyzeFileNode(directory, context, analyzer);
            if(node != null)
            {
                await AnalyzeDirectory(node, directory, context, analyzer);
            }
            return new AnalysisResult(node);
        }

        private async ValueTask<ILinkedNode> AnalyzeContents(ILinkedNode parent, IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            var folder = parent?[""] ?? context.NodeFactory.CreateUnique();

            if(folder != null)
            {
                folder.SetClass(Classes.Folder);

                parent.Set(Properties.InterpretedAs, folder);

                folder.Set(Properties.PrefLabel, DataTools.ReplaceControlCharacters(directory.ToString() + "/", null));

                LinkDirectories(folder, directory.Path, true, context.NodeFactory);

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(folder, alg, await alg.ComputeHash(directory, true), context.NodeFactory);
                }

                context = context.WithParent(parent);

                foreach(var entry in directory.Entries)
                {
                    var entryNode = CreateNode(entry, context);

                    await analyzer.Analyze(entry, context.WithNode(entryNode));
                    entryNode.Set(Properties.BelongsToContainer, folder);
                }
            }

            return folder;
        }

        static readonly string[] directoryEnding = { "" };

        private void LinkDirectories(ILinkedNode initial, string path, bool directory, ILinkedNodeFactory nodeFactory)
        {
            if(path == null) return;
            var parts = path.Split('/');
            if(parts.Length == 0) return;
            for(int i = 0; i < parts.Length; i++)
            {
                var escapedParts = parts.Skip(i).Select(Uri.EscapeDataString);
                if(directory)
                {
                    escapedParts = escapedParts.Concat(directoryEnding);
                }
                var local = String.Join("/", escapedParts);
                ILinkedNode file;
                if(directory && local == "/")
                {
                    file = nodeFactory.Create(RootDirectoryUri.Instance, default);
                }else{
                    file = nodeFactory.Create(Vocabularies.File, local);
                }
                initial.Set(Properties.PathObject, file);
                initial = file;
            }

            if(!directory)
            {
                string ext;
                try{
                    ext = Path.GetExtension(parts[parts.Length - 1]);
                }catch(ArgumentException)
                {
                    ext = null;
                }
                if(!String.IsNullOrEmpty(ext))
                {
                    ext = ext.Substring(1).ToLowerInvariant();
                    initial.Set(Properties.ExtensionObject, Vocabularies.Uris, Uri.EscapeDataString(ext));
                }
            }
        }

        public ValueTask<AnalysisResult> Analyze(FileInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            return analyzer.Analyze<IFileInfo>(new FileInfoWrapper(entity), context);
        }

        public ValueTask<AnalysisResult> Analyze(DirectoryInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            return analyzer.Analyze<IDirectoryInfo>(new DirectoryInfoWrapper(entity), context);
        }

        public ValueTask<AnalysisResult> Analyze(IFileNodeInfo entity, AnalysisContext context, IEntityAnalyzers analyzer)
        {
            switch(entity)
            {
                case IFileInfo file: return Analyze(file, context, analyzer);
                case IDirectoryInfo dir: return Analyze(dir, context, analyzer);
                default: return new ValueTask<AnalysisResult>(new AnalysisResult(CreateNode(entity, context)));
            }
        }

        /// <summary>
        /// This class is used to provide a fake URI with the value of
        /// <see cref="RootDirectoryUri.Value"/> when .NET would like to change it.
        /// </summary>
        class RootDirectoryUri : Uri, IIndividualUriFormatter<ValueTuple>
        {
            public const string Value = "file:///./";

            public static readonly RootDirectoryUri Instance = new RootDirectoryUri();

            private RootDirectoryUri() : base(Value, UriKind.Absolute)
            {

            }

            public Uri this[ValueTuple value] => this;

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
