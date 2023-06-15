using DiscUtils;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
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

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IFileSystem filesystem, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            string? label = null;
            if(filesystem is DiscFileSystem disc)
            {
                label = disc.VolumeLabel;
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

            public override string? DriveFormat {
                get {
                    switch(fileSystem)
                    {
                        case DiscFileSystem disc:
                            return disc.FriendlyName;
                        default:
                            return null;
                    }
                }
            }

            public override DriveType DriveType => DriveType.Unknown;

            public override long? TotalFreeSpace {
                get {
                    try{
                        return fileSystem.AvailableSpace;
                    }catch(NotSupportedException)
                    {
                        return null;
                    }
                }
            }

            public override long? OccupiedSpace {
                get {
                    try{
                        return fileSystem.UsedSpace;
                    }catch(NotSupportedException)
                    {
                        return null;
                    }
                }
            }

            public override long? TotalSize {
                get {
                    try{
                        return fileSystem.AvailableSpace + fileSystem.UsedSpace;
                    }catch(NotSupportedException)
                    {
                        return null;
                    }
                }
            }

            public override string? VolumeLabel {
                get {
                    switch(fileSystem)
                    {
                        case DiscFileSystem disc:
                            return disc.VolumeLabel;
                        default:
                            return null;
                    }
                }
            }

            public override object? ReferenceKey => fileSystem;

            public override object? DataKey => null;
        }

        abstract class FileSystemInfoWrapper<TInfo> : IFileNodeInfo where TInfo : DiscFileSystemInfo
        {
            protected readonly TInfo info;

            public FileSystemInfoWrapper(TInfo info)
            {
                this.info = info;
            }

            static readonly Regex revisionRegex = new(@";(\d+)$", RegexOptions.Compiled);

            public string? Name => revisionRegex.Replace(info.Name, "");

            public string? SubName => Revision?.ToString();

            public virtual string? Path => revisionRegex.Replace(info.FullName, "").Replace(System.IO.Path.DirectorySeparatorChar, '/');

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

            public FileAttributes Attributes => info.Attributes;

            object IIdentityKey.ReferenceKey => info.FileSystem;

            object IIdentityKey.DataKey => info.FullName;

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

            public override string? Path => base.Path?.TrimEnd('/');

            public IEnumerable<IFileNodeInfo> Entries =>
                info.GetFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                    info.GetDirectories().Select(d => new DirectoryInfoWrapper(d))
                    );

            public Environment.SpecialFolder? SpecialFolderType => null;
        }
    }
}
