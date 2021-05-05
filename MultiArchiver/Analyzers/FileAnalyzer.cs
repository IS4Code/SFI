using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
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

        private ILinkedNode AnalyzeInner(ILinkedNode parent, IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            var name = Uri.EscapeDataString(info.Name);
            var node = parent?[name] ?? nodeFactory.NewGuidNode();

            node.SetClass(Classes.FileDataObject);

            LinkDirectories(node, info.Path, false, nodeFactory);

            node.Set(Properties.PrefLabel, "/" + info.Path);

            node.Set(Properties.FileName, info.Name);
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

        private void HashInfo(ILinkedNode node, IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            foreach(var alg in HashAlgorithms)
            {
                HashAlgorithm.AddHash(node, alg, alg.ComputeHash(info), nodeFactory);
            }
        }

        public ILinkedNode Analyze(ILinkedNode parent, IFileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeInner(parent, file, nodeFactory);
            if(node != null)
            {
                HashInfo(node, file, nodeFactory);

                if(file.Length is long len)
                {
                    node.Set(Properties.FileSize, len);
                }
                var content = nodeFactory.Create<IStreamFactory>(node, file);
                if(content != null)
                {
                    content.Set(Properties.IsStoredAs, node);
                }

                if(file is IDirectoryInfo directory)
                {
                    var folder = AnalyzeContents(node, directory, nodeFactory);

                    if(folder != null)
                    {
                        folder.Set(Properties.IsStoredAs, node);
                    }
                }
            }
            return node;
        }

        public ILinkedNode Analyze(ILinkedNode parent, IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeInner(parent, directory, nodeFactory);
            if(node != null)
            {
                var folder = AnalyzeContents(node, directory, nodeFactory);

                if(folder != null)
                {
                    folder.Set(Properties.IsStoredAs, node);
                }
            }
            return node;
        }

        private ILinkedNode AnalyzeContents(ILinkedNode parent, IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var folder = parent?[""] ?? nodeFactory.NewGuidNode();

            if(folder != null)
            {
                folder.SetClass(Classes.Folder);

                folder.Set(Properties.IsStoredAs, parent);

                folder.Set(Properties.PrefLabel, "/" + directory.Path + "/");

                LinkDirectories(folder, directory.Path, true, nodeFactory);

                HashInfo(folder, directory, nodeFactory);

                foreach(var entry in directory.Entries)
                {
                    var node2 = Analyze(parent, entry, nodeFactory);
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
            var parts = path.Split('/');
            for(int i = 0; i < parts.Length; i++)
            {
                var local = String.Join("/", parts.Skip(i).Select(Uri.EscapeDataString)) + (directory ? "/" : "");
                var file = nodeFactory.Create(Vocabularies.File, local);
                initial.Set(Properties.PathObject, file);
                initial = file;
            }
        }

        public ILinkedNode Analyze(ILinkedNode parent, FileInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(parent, new FileInfoWrapper(entity), nodeFactory);
        }

        public ILinkedNode Analyze(ILinkedNode parent, DirectoryInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(parent, new DirectoryInfoWrapper(entity), nodeFactory);
        }

        public ILinkedNode Analyze(ILinkedNode parent, IFileNodeInfo entity, ILinkedNodeFactory nodeFactory)
        {
            switch(entity)
            {
                case IFileInfo file: return Analyze(parent, file, nodeFactory);
                case IDirectoryInfo dir: return Analyze(parent, dir, nodeFactory);
                default: return null;
            }
        }

        class FileInfoWrapper : IFileInfo
        {
            readonly FileInfo baseInfo;

            public FileInfoWrapper(FileInfo baseInfo)
            {
                this.baseInfo = baseInfo;
            }

            public string Name => baseInfo.Name;

            public string Path => baseInfo.FullName.Substring(System.IO.Path.GetPathRoot(baseInfo.FullName).Length).Replace(System.IO.Path.DirectorySeparatorChar, '/');

            public long Length => baseInfo.Length;

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

            public int? Revision => null;

            public bool IsThreadSafe => true;

            object IPersistentKey.ReferenceKey => AppDomain.CurrentDomain;

            object IPersistentKey.DataKey => baseInfo.FullName;

            public Stream Open()
            {
                return baseInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }

            public override string ToString()
            {
                return baseInfo.ToString();
            }
        }

        class DirectoryInfoWrapper : IDirectoryInfo
        {
            readonly DirectoryInfo baseInfo;

            public DirectoryInfoWrapper(DirectoryInfo baseInfo)
            {
                this.baseInfo = baseInfo;
            }

            public string Name => baseInfo.Name;

            public string Path => baseInfo.FullName.Substring(System.IO.Path.GetPathRoot(baseInfo.FullName).Length);

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

            public int? Revision => null;

            public IEnumerable<IFileNodeInfo> Entries =>
                baseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                    baseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d))
                    );

            object IPersistentKey.ReferenceKey => AppDomain.CurrentDomain;

            object IPersistentKey.DataKey => baseInfo.FullName;

            public override string ToString()
            {
                return baseInfo.ToString();
            }
        }
    }
}
