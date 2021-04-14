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

        public override void Analyze(ILinkedNode node, IFileSystem filesystem, ILinkedNodeFactory nodeFactory)
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

        abstract class FileSystemInfoWrapper<TInfo> : IFileNodeInfo where TInfo : DiscFileSystemInfo
        {
            protected readonly TInfo info;

            public FileSystemInfoWrapper(TInfo info)
            {
                this.info = info;
            }

            public string Name => info.Name;

            public virtual string Path => info.FullName.Replace(System.IO.Path.DirectorySeparatorChar, '/');

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            object IPersistentKey.ReferenceKey => info.FileSystem;

            object IPersistentKey.DataKey => info.FullName;
        }

        class FileInfoWrapper : FileSystemInfoWrapper<DiscFileInfo>, IFileInfo
        {
            public FileInfoWrapper(DiscFileInfo info) : base(info)
            {

            }

            public long Length => info.Length;

            public bool IsThreadSafe => info.FileSystem.IsThreadSafe;

            public Stream Open()
            {
                return info.Open(FileMode.Open, FileAccess.Read);
            }
        }

        class DirectoryInfoWrapper : FileSystemInfoWrapper<DiscDirectoryInfo>, IDirectoryInfo
        {
            public DirectoryInfoWrapper(DiscDirectoryInfo info) : base(info)
            {

            }

            public override string Path => base.Path.TrimEnd('/');

            public IEnumerable<IFileNodeInfo> Entries =>
                info.GetFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                    info.GetDirectories().Select(d => new DirectoryInfoWrapper(d))
                    );
        }
    }
}
