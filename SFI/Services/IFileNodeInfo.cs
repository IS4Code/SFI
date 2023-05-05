using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents an entity in a file system.
    /// </summary>
    public interface IFileNodeInfo : IIdentityKey
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// The sub-name part of the file, discriminating it
        /// from similar files with the same name, but not logically part
        /// of the name.
        /// </summary>
        string? SubName { get; }

        /// <summary>
        /// The full path to the file, not including the initial '/'.
        /// </summary>
        string? Path { get; }

        /// <summary>
        /// The revision of the file, used in some file systems.
        /// </summary>
        int? Revision { get; }

        /// <summary>
        /// The creation time of the file, if present.
        /// </summary>
        DateTime? CreationTime { get; }

        /// <summary>
        /// The modification time of the file, if present.
        /// </summary>
        DateTime? LastWriteTime { get; }

        /// <summary>
        /// The time of last access to the file, if present.
        /// </summary>
        DateTime? LastAccessTime { get; }

        /// <summary>
        /// The kind of the file.
        /// </summary>
        FileKind Kind { get; }

        /// <summary>
        /// The attributes of the file.
        /// </summary>
        FileAttributes Attributes { get; }
    }

    /// <summary>
    /// Specific kind of the file or directory.
    /// </summary>
    public enum FileKind
    {
        /// <summary>
        /// The file has no special kind.
        /// </summary>
        None,

        /// <summary>
        /// The file is embedded in another file.
        /// </summary>
        Embedded,

        /// <summary>
        /// The file is an entry in an archive.
        /// </summary>
        ArchiveItem
    }

    /// <summary>
    /// Wraps an instance of <see cref="FileSystemInfo"/>, implementing <see cref="IFileNodeInfo"/>
    /// through it.
    /// </summary>
    /// <typeparam name="TInfo">The concrete type, either <see cref="FileInfo"/> or <see cref="DirectoryInfo"/>.</typeparam>
    public abstract class FileSystemInfoWrapper<TInfo> : IFileNodeInfo where TInfo : FileSystemInfo
    {
        /// <summary>
        /// The underlying info instance.
        /// </summary>
        public TInfo BaseInfo { get; }

        readonly IIdentityKey? key;

        /// <summary>
        /// Creates a new instance of the wrapper.
        /// </summary>
        /// <param name="baseInfo">The value of <see cref="BaseInfo"/> to use when delegating properties.</param>
        /// <param name="key">
        /// The implementation of <see cref="IIdentityKey"/> to use.
        /// If none is provided, <see cref="IIdentityKey.ReferenceKey"/> is the type of <see cref="FileSystemInfo"/>
        /// and <see cref="IIdentityKey.DataKey"/> is <see cref="FileSystemInfo.FullPath"/>.
        /// </param>
        public FileSystemInfoWrapper(TInfo baseInfo, IIdentityKey? key = null)
        {
            if(!baseInfo.Exists)
            {
                throw new ArgumentException("The supplied argument is not valid!", nameof(baseInfo));
            }
            this.BaseInfo = baseInfo;
            this.key = key;
        }

        /// <inheritdoc/>
        public string? Name => BaseInfo.Name;

        /// <inheritdoc/>
        public string? SubName => null;

        /// <inheritdoc/>
        public string Path => BaseInfo.FullName.Substring(System.IO.Path.GetPathRoot(BaseInfo.FullName).Length).Replace(System.IO.Path.DirectorySeparatorChar, '/');

        /// <inheritdoc/>
        public DateTime? CreationTime {
            get {
                try{
                    return BaseInfo.CreationTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public DateTime? LastWriteTime {
            get {
                try{
                    return BaseInfo.LastWriteTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public DateTime? LastAccessTime {
            get {
                try{
                    return BaseInfo.LastAccessTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public int? Revision => null;

        /// <inheritdoc/>
        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        static readonly Type FileSystemInfoType = typeof(FileSystemInfo);

        object? IIdentityKey.ReferenceKey => key != null ? key.ReferenceKey : FileSystemInfoType;

        object? IIdentityKey.DataKey => key != null ? key.DataKey : BaseInfo.FullName;

        /// <inheritdoc/>
        public FileKind Kind => FileKind.None;

        /// <inheritdoc/>
        public FileAttributes Attributes => BaseInfo.Attributes;

        /// <inheritdoc/>
        public override string ToString()
        {
            return "/" + Path;
        }
    }

    /// <summary>
    /// The extension methods for <see cref="IFileNodeInfo"/>.
    /// </summary>
    public static class FileNodeInfoExtensions
    {
        /// <summary>
        /// Enumerates all files under the <paramref name="fileNode"/> object, by recursively obtaining
        /// <see cref="IDirectoryInfo.Entries"/> on the directories within.
        /// </summary>
        /// <param name="fileNode">An instance of either <see cref="IFileInfo"/> or <see cref="IDirectoryInfo"/>.</param>
        /// <returns>A sequence of all instances of <see cref="IFileInfo"/> in the hierarchy.</returns>
        public static IEnumerable<IFileInfo> EnumerateFiles(this IFileNodeInfo fileNode)
        {
            switch(fileNode)
            {
                case IDirectoryInfo directory:
                    var entries = directory.Entries.SelectMany(EnumerateFiles);
                    return directory is IFileInfo alsoFile
                        ? entries.Concat(new[] { alsoFile })
                        : entries;
                case IFileInfo file:
                    return new[] { file };
                default:
                    return Array.Empty<IFileInfo>();
            }
        }
    }
}
