using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents an entity in a file system.
    /// </summary>
    public interface IFileNodeInfo : IPersistentKey
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
    /// Represents a concrete file in a file system.
    /// </summary>
    public interface IFileInfo : IFileNodeInfo, IStreamFactory
    {

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

        /// <summary>
        /// The type of the folder, as a value from <see cref="Environment.SpecialFolder"/>
        /// or <see cref="VirtualFolders"/>, if applicable.
        /// </summary>
        Environment.SpecialFolder? SpecialFolderType { get; }
    }

    /// <summary>
    /// Additional values of <see cref="Environment.SpecialFolder"/> for
    /// virtual folders, usable in <see cref="IDirectoryInfo.SpecialFolderType"/>.
    /// </summary>
    public static class VirtualFolders
    {
        /// <summary>
        /// A virtual folder for Internet Explorer.
        /// </summary>
        public const Environment.SpecialFolder InternetExplorer = (Environment.SpecialFolder)1;

        /// <summary>
        /// The virtual folder that contains icons for the Control Panel applications.
        /// </summary>
        public const Environment.SpecialFolder ControlPanel = (Environment.SpecialFolder)3;

        /// <summary>
        /// The virtual folder that contains installed printers.
        /// </summary>
        public const Environment.SpecialFolder Printers = (Environment.SpecialFolder)4;

        /// <summary>
        /// The virtual folder that contains the objects in the user's Recycle Bin.
        /// </summary>
        public const Environment.SpecialFolder RecycleBin = (Environment.SpecialFolder)10;

        /// <summary>
        /// A virtual folder that represents Network Neighborhood, the root of the network namespace hierarchy.
        /// </summary>
        public const Environment.SpecialFolder NetworkNeighborhood = (Environment.SpecialFolder)18;

        /// <summary>
        /// The virtual folder that represents Network Connections, that contains network and dial-up connections.
        /// </summary>
        public const Environment.SpecialFolder NetworkConnections = (Environment.SpecialFolder)48;
    }

    /// <summary>
    /// Represents a directory that is also a logical drive.
    /// </summary>
    public interface IDriveInfo : IDirectoryInfo
    {
        /// <summary>
        /// The format of the drive, e.g. NTFS or FAT32.
        /// </summary>
        string? DriveFormat { get; }

        /// <summary>
        /// The type of the drive.
        /// </summary>
        DriveType DriveType { get; }

        /// <summary>
        /// The number of bytes of remaining space on the drive.
        /// </summary>
        long? TotalFreeSpace { get; }

        /// <summary>
        /// The number of bytes occupied by data on the drive.
        /// </summary>
        long? OccupiedSpace { get; }

        /// <summary>
        /// The total number of bytes on the drive.
        /// </summary>
        long? TotalSize { get; }

        /// <summary>
        /// The drive label.
        /// </summary>
        string? VolumeLabel { get; }
    }

    /// <summary>
    /// An abstract class to use for the directory representing
    /// the root of a file system.
    /// </summary>
    public abstract class RootDirectoryInfo : IDriveInfo
    {
        /// <inheritdoc/>
        public abstract IEnumerable<IFileNodeInfo> Entries { get; }

        /// <inheritdoc/>
        public virtual string? Name => null;

        /// <inheritdoc/>
        public virtual string? SubName => null;

        /// <inheritdoc/>
        public virtual string Path => "";

        /// <inheritdoc/>
        public virtual int? Revision => null;

        /// <inheritdoc/>
        public virtual DateTime? CreationTime => null;

        /// <inheritdoc/>
        public virtual DateTime? LastWriteTime => null;

        /// <inheritdoc/>
        public virtual DateTime? LastAccessTime => null;

        /// <inheritdoc/>
        public virtual FileKind Kind => FileKind.None;

        /// <inheritdoc/>
        public virtual FileAttributes Attributes => FileAttributes.Directory;

        /// <inheritdoc/>
        public Environment.SpecialFolder? SpecialFolderType => null;

        /// <inheritdoc/>
        public virtual string? DriveFormat => null;

        /// <inheritdoc/>
        public virtual DriveType DriveType => DriveType.Unknown;

        /// <inheritdoc/>
        public virtual long? TotalFreeSpace => null;

        /// <inheritdoc/>
        public virtual long? OccupiedSpace => null;

        /// <inheritdoc/>
        public virtual long? TotalSize => null;

        /// <inheritdoc/>
        public virtual string? VolumeLabel => null;

        /// <inheritdoc/>
        public abstract object? ReferenceKey { get; }

        /// <inheritdoc/>
        public abstract object? DataKey { get; }

        /// <inheritdoc/>
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

        readonly IPersistentKey? key;

        /// <summary>
        /// Creates a new instance of the wrapper.
        /// </summary>
        /// <param name="baseInfo">The value of <see cref="BaseInfo"/> to use when delegating properties.</param>
        /// <param name="key">
        /// The implementation of <see cref="IPersistentKey"/> to use.
        /// If none is provided, <see cref="IPersistentKey.ReferenceKey"/> is <see cref="AppDomain.CurrentDomain"/>
        /// and <see cref="IPersistentKey.DataKey"/> is <see cref="FileSystemInfo.FullPath"/>.
        /// </param>
        public FileSystemInfoWrapper(TInfo baseInfo, IPersistentKey? key = null)
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

        object? IPersistentKey.ReferenceKey => key != null ? key.ReferenceKey : AppDomain.CurrentDomain;

        object? IPersistentKey.DataKey => key != null ? key.DataKey : BaseInfo.FullName;

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
    /// Wraps a <see cref="FileInfo"/> instance.
    /// </summary>
    public class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo
    {
        /// <inheritdoc/>
        public FileInfoWrapper(FileInfo baseInfo, IPersistentKey? key = null) : base(baseInfo, key)
        {

        }

        /// <inheritdoc/>
        public long Length => BaseInfo.Length;

        /// <inheritdoc/>
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
        public DirectoryInfoWrapper(DirectoryInfo baseInfo, IPersistentKey? key = null) : base(baseInfo, key)
        {

        }

        /// <inheritdoc/>
        public IEnumerable<IFileNodeInfo> Entries =>
            BaseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                BaseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d))
                );

        /// <inheritdoc/>
        public Environment.SpecialFolder? SpecialFolderType => null;
    }

    /// <summary>
    /// Wraps a <see cref="System.IO.DriveInfo"/> instance.
    /// </summary>
    public class DriveInfoWrapper : DirectoryInfoWrapper, IDriveInfo
    {
        /// <summary>
        /// The underlying <see cref="System.IO.DriveInfo"/> instance.
        /// </summary>
        public DriveInfo DriveInfo { get; }

        /// <inheritdoc/>
        public DriveInfoWrapper(DriveInfo baseInfo, IPersistentKey? key = null) : base(baseInfo.RootDirectory, key)
        {
            DriveInfo = baseInfo;
        }

        /// <inheritdoc/>
        public string? DriveFormat => DriveInfo.IsReady ? DriveInfo.DriveFormat : null;

        /// <inheritdoc/>
        public DriveType DriveType => DriveInfo.DriveType;

        /// <inheritdoc/>
        public long? TotalFreeSpace => DriveInfo.IsReady ? DriveInfo.TotalFreeSpace : null;

        /// <inheritdoc/>
        public long? OccupiedSpace => DriveInfo.IsReady ? DriveInfo.TotalSize - DriveInfo.TotalFreeSpace : null;

        /// <inheritdoc/>
        public long? TotalSize => DriveInfo.IsReady ? DriveInfo.TotalSize : null;

        /// <inheritdoc/>
        public string? VolumeLabel => DriveInfo.IsReady ? DriveInfo.VolumeLabel : null;
    }
}
