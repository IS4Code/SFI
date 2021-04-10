using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FileAnalyzer : IEntityAnalyzer<IFileInfo>, IEntityAnalyzer<FileInfo>, IEntityAnalyzer<IDirectoryInfo>, IEntityAnalyzer<DirectoryInfo>
    {
        public FileAnalyzer()
        {

        }

        private ILinkedNode Analyze(IFileNodeInfo info, ILinkedNodeFactory nodeFactory)
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

        public ILinkedNode Analyze(IFileInfo file, ILinkedNodeFactory nodeFactory)
        {
            var node = Analyze((IFileNodeInfo)file, nodeFactory);
            if(node != null)
            {
                if(file.Length is long len)
                {
                    node.Set(Properties.FileSize, len);
                }
                var content = nodeFactory.Create<Func<Stream>>(() => file.Open());
                if(content != null)
                {
                    content.Set(Properties.IsStoredAs, node);
                }
            }
            return node;
        }

        public ILinkedNode Analyze(IDirectoryInfo directory, ILinkedNodeFactory nodeFactory)
        {
            var node = Analyze((IFileNodeInfo)directory, nodeFactory);
            if(node != null)
            {
                var folder = nodeFactory.Root[Guid.NewGuid().ToString("D")];

                if(folder != null)
                {
                    folder.SetClass(Classes.Folder);

                    folder.Set(Properties.IsStoredAs, node);

                    foreach(var entry in directory.Entries)
                    {
                        var node2 = nodeFactory.Create(entry.WithContainer(folder));
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

            public long? Length => baseInfo.Length;

            public DateTime? CreationTime => baseInfo.CreationTimeUtc;

            public DateTime? LastWriteTime => baseInfo.LastWriteTimeUtc;

            public DateTime? LastAccessTime => baseInfo.LastAccessTimeUtc;

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

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new DirectoryInfoWrapper(baseInfo, container);
            }
        }
    }
}
