using IS4.SFI.Services;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// Provides an implementation of <see cref="IFileInfo"/> for
    /// an instance of <see cref="IBrowserFile"/>.
    /// </summary>
    public class BrowserFileInfo : IFileInfo
    {
        readonly IBrowserFile file;

        /// <summary>
        /// Creates a new instance of the file info from a file in a browser.
        /// </summary>
        /// <param name="file">The underlying file to wrap.</param>
        public BrowserFileInfo(IBrowserFile file)
        {
            this.file = file;
        }

        /// <inheritdoc/>
        public string? Name => file.Name;

        /// <inheritdoc/>
        public string? SubName => null;

        /// <inheritdoc/>
        public string? Path => null;

        /// <inheritdoc/>
        public int? Revision => null;

        /// <inheritdoc/>
        public DateTime? CreationTime => null;

        /// <inheritdoc/>
        public DateTime? LastWriteTime => file.LastModified.UtcDateTime;

        /// <inheritdoc/>
        public DateTime? LastAccessTime => null;

        /// <inheritdoc/>
        public FileKind Kind => FileKind.None;

        /// <inheritdoc/>
        public long Length => file.Size;

        /// <inheritdoc/>
        public FileAttributes Attributes => FileAttributes.Normal;

        /// <inheritdoc/>
        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        /// <inheritdoc/>
        public object? ReferenceKey => file;

        /// <inheritdoc/>
        public object? DataKey => null;

        /// <inheritdoc/>
        public Stream Open()
        {
            return file.OpenReadStream(Int64.MaxValue);
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return Name;
        }
    }
}
