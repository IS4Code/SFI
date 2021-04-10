using DiscUtils;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class FileSystemAnalyzer : ClassRecognizingAnalyzer<IFileSystem>
    {
        public FileSystemAnalyzer() : base(Classes.FilesystemImage)
        {

        }

        public override ILinkedNode Analyze(IFileSystem filesystem, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(filesystem, nodeFactory);
            if(node != null)
            {
                foreach(var file in filesystem.GetFiles(null))
                {
                    var info = filesystem.GetFileInfo(file);
                    var node2 = nodeFactory.Create(new FileInfoWrapper(info, node));
                    if(node2 != null)
                    {
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }

                foreach(var directory in filesystem.GetDirectories(null))
                {
                    var info = filesystem.GetDirectoryInfo(directory);
                    var node2 = nodeFactory.Create(new DirectoryInfoWrapper(info, node));
                    if(node2 != null)
                    {
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }
            }
            return node;
        }

        class FileInfoWrapper : IFileInfo
        {
            public ILinkedNode Container { get; }
            readonly DiscFileInfo info;

            public FileInfoWrapper(DiscFileInfo info, ILinkedNode container)
            {
                Container = container;
                this.info = info;
            }

            public string Name => info.Name;

            public string Path => info.FullName;

            public long? Length => info.Length;

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            public Stream Open()
            {
                return info.Open(FileMode.Open, FileAccess.Read);
            }

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new FileInfoWrapper(info, container);
            }
        }

        class DirectoryInfoWrapper : IDirectoryInfo
        {
            public ILinkedNode Container { get; }
            readonly DiscDirectoryInfo info;

            public DirectoryInfoWrapper(DiscDirectoryInfo info, ILinkedNode container)
            {
                Container = container;
                this.info = info;
            }

            public string Name => info.Name;

            public string Path => info.FullName;

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            public IEnumerable<IFileNodeInfo> Entries =>
                info.GetFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f, null)).Concat(
                    info.GetDirectories().Select(d => new DirectoryInfoWrapper(d, null))
                    );

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new DirectoryInfoWrapper(info, container);
            }
        }
    }
}
