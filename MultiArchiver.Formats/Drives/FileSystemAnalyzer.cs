using DiscUtils;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class FileSystemAnalyzer : FormatAnalyzer<IFileSystem>
    {
        public FileSystemAnalyzer() : base(Classes.FilesystemImage)
        {

        }

        public override ILinkedNode Analyze(ILinkedNode parent, IFileSystem filesystem, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(parent, filesystem, nodeFactory);
            if(node != null)
            {
                foreach(var file in filesystem.GetFiles(null))
                {
                    var info = filesystem.GetFileInfo(file);
                    var node2 = nodeFactory.Create(node, new FileInfoWrapper(info));
                    if(node2 != null)
                    {
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }

                foreach(var directory in filesystem.GetDirectories(null))
                {
                    var info = filesystem.GetDirectoryInfo(directory);
                    var node2 = nodeFactory.Create(node, new DirectoryInfoWrapper(info));
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
            readonly DiscFileInfo info;

            public FileInfoWrapper(DiscFileInfo info)
            {
                this.info = info;
            }

            public string Name => info.Name;

            public string Path => info.FullName;

            public long Length => info.Length;

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            public bool IsThreadSafe => info.FileSystem.IsThreadSafe;

            object IPersistentKey.ReferenceKey => info.FileSystem;

            object IPersistentKey.DataKey => info.FullName;

            public Stream Open()
            {
                return info.Open(FileMode.Open, FileAccess.Read);
            }
        }

        class DirectoryInfoWrapper : IDirectoryInfo
        {
            readonly DiscDirectoryInfo info;

            public DirectoryInfoWrapper(DiscDirectoryInfo info)
            {
                this.info = info;
            }

            public string Name => info.Name;

            public string Path => info.FullName;

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            public IEnumerable<IFileNodeInfo> Entries =>
                info.GetFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                    info.GetDirectories().Select(d => new DirectoryInfoWrapper(d))
                    );

            object IPersistentKey.ReferenceKey => info.FileSystem;

            object IPersistentKey.DataKey => info.FullName;
        }
    }
}
