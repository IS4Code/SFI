using DiscUtils;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzer of file systems, as instances of <see cref="IFileSystem"/>.
    /// </summary>
    public class FileSystemAnalyzer : MediaObjectAnalyzer<IFileSystem>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public FileSystemAnalyzer() : base(Classes.FilesystemImage, Classes.Filesystem)
        {

        }

        public async override ValueTask<AnalysisResult> Analyze(IFileSystem filesystem, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            try{ node.Set(Properties.FreeSpace, filesystem.AvailableSpace); }
            catch(NotSupportedException) { }
            try{ node.Set(Properties.OccupiedSpace, filesystem.UsedSpace); }
            catch(NotSupportedException) { }
            try{ node.Set(Properties.TotalSpace, filesystem.AvailableSpace + filesystem.UsedSpace); }
            catch(NotSupportedException) { }

            string label = null;
            if(filesystem is DiscFileSystem disc)
            {
                node.Set(Properties.VolumeLabel, label = disc.VolumeLabel);
                node.Set(Properties.FilesystemType, disc.FriendlyName);
            }

            var info = new FileSystemWrapper(filesystem);
            var result = await analyzers.Analyze(info, context.WithNode(node));
            result.Label = label ?? result.Label;
            return result;
        }

        class FileSystemWrapper : RootDirectoryInfo
        {
            readonly IFileSystem fileSystem;

            public FileSystemWrapper(IFileSystem fileSystem)
            {
                this.fileSystem = fileSystem;
            }

            public override IEnumerable<IFileNodeInfo> Entries {
                get {
                    foreach(var file in fileSystem.GetFiles(null))
                    {
                        yield return new FileInfoWrapper(fileSystem.GetFileInfo(file));
                    }

                    foreach(var directory in fileSystem.GetDirectories(null))
                    {
                        yield return new DirectoryInfoWrapper(fileSystem.GetDirectoryInfo(directory));
                    }
                }
            }

            public override object ReferenceKey => fileSystem;

            public override object DataKey => null;
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

            public string SubName => Revision?.ToString();

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

            public FileKind Kind => FileKind.None;

            public override string ToString()
            {
                return "/" + Path;
            }
        }

        class FileInfoWrapper : FileSystemInfoWrapper<DiscFileInfo>, IFileInfo
        {
            public FileInfoWrapper(DiscFileInfo info) : base(info)
            {

            }

            public long Length => info.Length;

            public bool IsEncrypted => false;

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
