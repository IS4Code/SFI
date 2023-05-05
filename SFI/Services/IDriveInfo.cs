using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.SFI.Services
{
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
        public virtual Environment.SpecialFolder? SpecialFolderType => null;

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
    /// Wraps a <see cref="System.IO.DriveInfo"/> instance.
    /// </summary>
    public class DriveInfoWrapper : DirectoryInfoWrapper, IDriveInfo
    {
        /// <summary>
        /// The underlying <see cref="System.IO.DriveInfo"/> instance.
        /// </summary>
        public DriveInfo DriveInfo { get; }

        /// <inheritdoc/>
        public DriveInfoWrapper(DriveInfo baseInfo, IIdentityKey? key = null) : base(baseInfo.RootDirectory, key)
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
