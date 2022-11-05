using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Services
{
    /// <summary>
    /// Represents an entity in a file system.
    /// </summary>
    public interface IFileNodeInfo : IPersistentKey
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The sub-name part of the file, discriminating it
        /// from similar files with the same name, but not logically part
        /// of the name.
        /// </summary>
        string SubName { get; }

        /// <summary>
        /// The full path to the file, not including the initial '/'.
        /// </summary>
        string Path { get; }

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
    /// Represents a concrete file in a file system.
    /// </summary>
    public interface IFileInfo : IFileNodeInfo, IStreamFactory
    {
        /// <summary>
        /// True if the file is encrypted.
        /// </summary>
        bool IsEncrypted { get; }
    }

    /// <summary>
    /// Represents a concrete directory in a file system.
    /// </summary>
    public interface IDirectoryInfo : IFileNodeInfo
    {
        /// <summary>
        /// Contains the collection of all files and directories
        /// in this directory.
        /// </summary>
        IEnumerable<IFileNodeInfo> Entries { get; }
    }

    /// <summary>
    /// An abstract class to use for the directory representing
    /// the root of a file system.
    /// </summary>
    public abstract class RootDirectoryInfo : IDirectoryInfo
    {
        public abstract IEnumerable<IFileNodeInfo> Entries { get; }

        public virtual string Name => null;

        public virtual string SubName => null;

        public virtual string Path => "";

        public virtual int? Revision => null;

        public virtual DateTime? CreationTime => null;

        public virtual DateTime? LastWriteTime => null;

        public virtual DateTime? LastAccessTime => null;

        public virtual FileKind Kind => FileKind.None;

        public abstract object ReferenceKey { get; }

        public abstract object DataKey { get; }

        public override string ToString()
        {
            return "";
        }
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

        readonly IPersistentKey key;

        /// <summary>
        /// Creates a new instance of the wrapper.
        /// </summary>
        /// <param name="baseInfo">The value of <see cref="BaseInfo"/> to use when delegating properties.</param>
        /// <param name="key">
        /// The implementation of <see cref="IPersistentKey"/> to use.
        /// If none is provided, <see cref="IPersistentKey.ReferenceKey"/> is <see cref="AppDomain.CurrentDomain"/>
        /// and <see cref="IPersistentKey.DataKey"/> is <see cref="FileSystemInfo.FullPath"/>.
        /// </param>
        public FileSystemInfoWrapper(TInfo baseInfo, IPersistentKey key = null)
        {
            this.BaseInfo = baseInfo;
            this.key = key;
        }

        public string Name => BaseInfo.Name;

        public string SubName => null;

        public string Path => BaseInfo.FullName.Substring(System.IO.Path.GetPathRoot(BaseInfo.FullName).Length).Replace(System.IO.Path.DirectorySeparatorChar, '/');

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

        public int? Revision => null;

        public bool IsEncrypted => false;

        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        object IPersistentKey.ReferenceKey => key != null ? key.ReferenceKey : AppDomain.CurrentDomain;

        object IPersistentKey.DataKey => key != null ? key.DataKey : BaseInfo.FullName;

        public FileKind Kind => FileKind.None;

        public override string ToString()
        {
            return "/" + Path;
        }
    }

    /// <summary>
    /// Wraps a <see cref="FileInfo"/> instance.
    /// </summary>
    public class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo
    {
        /// <inheritdoc/>
        public FileInfoWrapper(FileInfo baseInfo, IPersistentKey key = null) : base(baseInfo, key)
        {

        }

        public long Length => BaseInfo.Length;

        public Stream Open()
        {
            return BaseInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
    }

    /// <summary>
    /// Wraps a <see cref="DirectoryInfo"/> instance.
    /// </summary>
    public class DirectoryInfoWrapper : FileSystemInfoWrapper<DirectoryInfo>, IDirectoryInfo
    {
        /// <inheritdoc/>
        public DirectoryInfoWrapper(DirectoryInfo baseInfo, IPersistentKey key = null) : base(baseInfo, key)
        {

        }

        public IEnumerable<IFileNodeInfo> Entries =>
            BaseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                BaseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d))
                );
    }
}
