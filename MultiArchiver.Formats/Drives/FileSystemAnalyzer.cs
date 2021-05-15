using DiscUtils;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IS4.MultiArchiver.Analyzers
{
    public class FileSystemAnalyzer : BinaryFormatAnalyzer<IFileSystem>
    {
        public FileSystemAnalyzer() : base(Classes.FilesystemImage)
        {

        }

        public override string Analyze(ILinkedNode node, IFileSystem filesystem, ILinkedNodeFactory nodeFactory)
        {
            try{ node.Set(Properties.FreeSpace, filesystem.AvailableSpace); }
            catch(NotSupportedException) { }
            try{ node.Set(Properties.OccupiedSpace, filesystem.UsedSpace); }
            catch(NotSupportedException) { }
            try{ node.Set(Properties.TotalSpace, filesystem.AvailableSpace + filesystem.UsedSpace); }
            catch(NotSupportedException) { }

            if(filesystem is DiscFileSystem disc)
            {
                node.Set(Properties.VolumeLabel, disc.VolumeLabel);
                node.Set(Properties.FilesystemType, disc.FriendlyName);
            }

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

            return null;
        }

        abstract class FileSystemInfoWrapper<TInfo> : IFileNodeInfo where TInfo : DiscFileSystemInfo
        {
            protected readonly TInfo info;

            public FileSystemInfoWrapper(TInfo info)
            {
                this.info = info;
            }

            static readonly Regex revisionRegex = new Regex(@";(\d+)$", RegexOptions.Compiled);

            public string Name => revisionRegex.Replace(info.Name, "");

            public virtual string Path => revisionRegex.Replace(info.FullName, "").Replace(System.IO.Path.DirectorySeparatorChar, '/');

            public DateTime? CreationTime => info.CreationTimeUtc;

            public DateTime? LastWriteTime => info.LastWriteTimeUtc;

            public DateTime? LastAccessTime => info.LastAccessTimeUtc;

            public int? Revision {
                get {
                    var match = revisionRegex.Match(info.Name);
                    if(!match.Success) return null;
                    if(Int32.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                    {
                        return result;
                    }
                    return null;
                }
            }

            object IPersistentKey.ReferenceKey => info.FileSystem;

            object IPersistentKey.DataKey => info.FullName;

            public override string ToString()
            {
                return Path;
            }
        }

        class FileInfoWrapper : FileSystemInfoWrapper<DiscFileInfo>, IFileInfo
        {
            public FileInfoWrapper(DiscFileInfo info) : base(info)
            {

            }

            public long Length => info.Length;

            public StreamFactoryAccess Access => info.FileSystem.IsThreadSafe ? StreamFactoryAccess.Parallel : StreamFactoryAccess.Reentrant;

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
