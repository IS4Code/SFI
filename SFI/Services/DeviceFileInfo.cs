using System;
using System.IO;

namespace IS4.SFI.Services
{
    /// <summary>
    /// This class represents a device as a file.
    /// </summary>
    public class DeviceFileInfo : IFileInfo
    {
        readonly Func<Stream> openFunc;

        /// <summary>
        /// Creates a new instance of the info.
        /// </summary>
        /// <param name="streamFactory">The function to use when opening the device.</param>
        public DeviceFileInfo(Func<Stream> streamFactory)
        {
            this.openFunc = streamFactory;
        }

        /// <inheritdoc/>
        public virtual string? Name => null;

        /// <inheritdoc/>
        public virtual string? SubName => null;

        /// <inheritdoc/>
        public virtual string? Path => null;

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
        public virtual long Length => -1;

        /// <inheritdoc/>
        public virtual FileAttributes Attributes => FileAttributes.Device;

        /// <inheritdoc/>
        public virtual StreamFactoryAccess Access => StreamFactoryAccess.Single;

        /// <inheritdoc/>
        public object? ReferenceKey => openFunc;

        /// <inheritdoc/>
        public object? DataKey => null;

        /// <inheritdoc/>
        public Stream Open()
        {
            return openFunc();
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return null;
        }
    }
}
