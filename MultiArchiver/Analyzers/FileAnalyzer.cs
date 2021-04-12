using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<FileInfo>, IEntityAnalyzer<DirectoryInfo>, IEntityAnalyzer<IFileNodeInfo>
    {
        public ICollection<IFileHashAlgorithm> HashAlgorithms { get; } = new List<IFileHashAlgorithm>();

        public FileAnalyzer()
        {

        }

        private ILinkedNode AnalyzeInner(IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            var name = Uri.EscapeUriString(info.Name);
            var node = info.Container?[name] ?? nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.SetClass(Classes.FileDataObject);

            node.Set(Properties.Broader, Vocabularies.File, name);
            //handler.HandleTriple(fileNode, this[Properties.Broader], this[pathUri]);
            //handler.HandleTriple(this[pathUri], this[Properties.Broader], this[fileUri]);

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

            return node;
        }

        private void HashInfo(ILinkedNode node, IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
        {
            foreach(var alg in HashAlgorithms)
            {
                var hash = alg.ComputeHash(info);
                var hashNode = nodeFactory.Create(alg, hash);

                hashNode.SetClass(Classes.Digest);

                hashNode.Set(Properties.DigestAlgorithm, alg.Identifier);
                hashNode.Set(Properties.DigestValue, Convert.ToBase64String(hash), Datatypes.Base64Binary);

                node.Set(Properties.Broader, hashNode);
            }
        }

        private ILinkedNode Analyze(IFileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeInner(file, nodeFactory);
            if(node != null)
            {
                HashInfo(node, file, nodeFactory);

                if(file.Length is long len)
                {
                    node.Set(Properties.FileSize, len);
                }
                var content = nodeFactory.Create<IStreamFactory>(file);
                if(content != null)
                {
                    content.Set(Properties.IsStoredAs, node);
                }
            }
            return node;
        }

        private ILinkedNode Analyze(IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var node = AnalyzeInner(directory, nodeFactory);
            if(node != null)
            {
                var folder = node[""] ?? nodeFactory.Root[Guid.NewGuid().ToString("D")];

                if(folder != null)
                {
                    folder.SetClass(Classes.Folder);

                    folder.Set(Properties.IsStoredAs, node);

                    HashInfo(folder, directory, nodeFactory);

                    foreach(var entry in directory.Entries)
                    {
                        var node2 = Analyze(entry.WithContainer(node), nodeFactory);
                        if(node2 != null)
                        {
                            node2.Set(Properties.BelongsToContainer, folder);
                        }
                    }
                }
            }
            return node;
        }

        public ILinkedNode Analyze(FileInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(new FileInfoWrapper(entity, null), nodeFactory);
        }

        public ILinkedNode Analyze(DirectoryInfo entity, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(new DirectoryInfoWrapper(entity, null), nodeFactory);
        }

        public ILinkedNode Analyze(IFileNodeInfo entity, ILinkedNodeFactory nodeFactory)
        {
            switch(entity)
            {
                case IFileInfo file: return Analyze(file, nodeFactory);
                case IDirectoryInfo dir: return Analyze(dir, nodeFactory);
                default: return null;
            }
        }

        class FileInfoWrapper : IFileInfo
        {
            public ILinkedNode Container { get; }

            readonly FileInfo baseInfo;

            public FileInfoWrapper(FileInfo baseInfo, ILinkedNode container)
            {
                this.baseInfo = baseInfo;
                Container = container;
            }

            public string Name => baseInfo.Name;

            public string Path => baseInfo.FullName.Substring(System.IO.Path.GetPathRoot(baseInfo.FullName).Length);

            public long Length => baseInfo.Length;

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

            public bool IsThreadSafe => true;

            object IPersistentKey.ReferenceKey => AppDomain.CurrentDomain;

            object IPersistentKey.DataKey => baseInfo.FullName;

            public Stream Open()
            {
                return baseInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new FileInfoWrapper(baseInfo, container);
            }
        }

        class DirectoryInfoWrapper : IDirectoryInfo
        {
            public ILinkedNode Container { get; }

            readonly DirectoryInfo baseInfo;

            public DirectoryInfoWrapper(DirectoryInfo baseInfo, ILinkedNode container)
            {
                this.baseInfo = baseInfo;
                Container = container;
            }

            public string Name => baseInfo.Name;

            public string Path => baseInfo.FullName.Substring(System.IO.Path.GetPathRoot(baseInfo.FullName).Length);

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

            public IEnumerable<IFileNodeInfo> Entries =>
                baseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f, null)).Concat(
                    baseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d, null))
                    );

            object IPersistentKey.ReferenceKey => AppDomain.CurrentDomain;

            object IPersistentKey.DataKey => baseInfo.FullName;

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new DirectoryInfoWrapper(baseInfo, container);
            }
        }
    }
}
