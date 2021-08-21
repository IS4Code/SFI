using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<FileInfo>, IEntityAnalyzer<DirectoryInfo>, IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<IDirectoryInfo>, IEntityAnalyzer<IFileNodeInfo>
    {
        public ICollection<IFileHashAlgorithm> HashAlgorithms { get; } = new List<IFileHashAlgorithm>();

        public FileAnalyzer()
        {

        }

        private ILinkedNode CreateNode(IFileNodeInfo info, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            if(context.Node != null) return context.Node;
            var name = Uri.EscapeDataString(info.Name ?? "");
            if(info.SubName is string subName)
            {
                name += ":" + Uri.EscapeDataString(subName);
            }
            return context.Parent?[name] ?? context.NodeFactory.NewGuidNode();
        }

        private ILinkedNode AnalyzeFileNode(IFileNodeInfo info, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            var node = CreateNode(info, context, analyzer);

            node.SetClass(Classes.FileDataObject);

            if(info.Path != null)
            {
                LinkDirectories(node, info.Path, false, context.NodeFactory);
            }else if(info.Name != null)
            {
                LinkDirectories(node, info.Name, false, context.NodeFactory);
            }
            node.Set(Properties.PrefLabel, DataTools.ReplaceControlCharacters(info.ToString(), null));

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

        public AnalysisResult Analyze(IFileInfo file, AnalysisContext context, IEntityAnalyzer analyzer)
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
                    var content = analyzer.Analyze<IStreamFactory>(file, context.WithParent(node)).Node;
                    if(content != null)
                    {
                        content.Set(Properties.IsStoredAs, node);
                    }
                }

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(node, alg, alg.ComputeHash(file), context.NodeFactory);
                }

                if(file is IDirectoryInfo directory)
                {
                    AnalyzeDirectory(node, directory, context, analyzer);
                }
            }
            return new AnalysisResult(node);
        }

        private void AnalyzeDirectory(ILinkedNode node, IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            var folder = AnalyzeContents(node, directory, context, analyzer);

            if(folder != null)
            {
                folder.Set(Properties.IsStoredAs, node);
            }

            foreach(var alg in HashAlgorithms)
            {
                HashAlgorithm.AddHash(node, alg, alg.ComputeHash(directory, false), context.NodeFactory);
            }
        }

        public AnalysisResult Analyze(IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            var node = AnalyzeFileNode(directory, context, analyzer);
            if(node != null)
            {
                AnalyzeDirectory(node, directory, context, analyzer);
            }
            return new AnalysisResult(node);
        }

        private ILinkedNode AnalyzeContents(ILinkedNode parent, IDirectoryInfo directory, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            var folder = parent?[""] ?? context.NodeFactory.NewGuidNode();

            if(folder != null)
            {
                folder.SetClass(Classes.Folder);

                folder.Set(Properties.IsStoredAs, parent);

                folder.Set(Properties.PrefLabel, DataTools.ReplaceControlCharacters(directory.ToString() + "/", null));

                LinkDirectories(folder, directory.Path, true, context.NodeFactory);

                foreach(var alg in HashAlgorithms)
                {
                    HashAlgorithm.AddHash(folder, alg, alg.ComputeHash(directory, true), context.NodeFactory);
                }

                foreach(var entry in directory.Entries)
                {
                    var node2 = Analyze(entry, context.WithParent(parent), analyzer).Node;
                    if(node2 != null)
                    {
                        node2.Set(Properties.BelongsToContainer, folder);
                    }
                }
            }

            return folder;
        }

        private void LinkDirectories(ILinkedNode initial, string path, bool directory, ILinkedNodeFactory nodeFactory)
        {
            if(path == null) return;
            var parts = path.Split('/');
            if(parts.Length == 0) return;
            for(int i = 0; i < parts.Length; i++)
            {
                var local = String.Join("/", parts.Skip(i).Select(Uri.EscapeDataString)) + (directory ? "/" : "");
                var file = nodeFactory.Create(Vocabularies.File, local);
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

        public AnalysisResult Analyze(FileInfo entity, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            return Analyze(new FileInfoWrapper(entity), context, analyzer);
        }

        public AnalysisResult Analyze(DirectoryInfo entity, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            return Analyze(new DirectoryInfoWrapper(entity), context, analyzer);
        }

        public AnalysisResult Analyze(IFileNodeInfo entity, AnalysisContext context, IEntityAnalyzer analyzer)
        {
            switch(entity)
            {
                case IFileInfo file: return Analyze(file, context, analyzer);
                case IDirectoryInfo dir: return Analyze(dir, context, analyzer);
                default: return new AnalysisResult(CreateNode(entity, context, analyzer));
            }
        }
    }
}
