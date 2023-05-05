using System.IO;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents a concrete file in a file system.
    /// </summary>
    public interface IFileInfo : IFileNodeInfo, IStreamFactory
    {

    }

    /// <summary>
    /// Wraps a <see cref="FileInfo"/> instance.
    /// </summary>
    public class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo
    {
        /// <inheritdoc/>
        public FileInfoWrapper(FileInfo baseInfo, IIdentityKey? key = null) : base(baseInfo, key)
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
}
