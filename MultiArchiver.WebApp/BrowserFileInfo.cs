using IS4.MultiArchiver.Services;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;

namespace IS4.MultiArchiver.WebApp
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

        public bool IsEncrypted => false;

        public string Name => file.Name;

        public string SubName => null;

        public string Path => null;

        public int? Revision => null;

        public DateTime? CreationTime => null;

        public DateTime? LastWriteTime => file.LastModified.UtcDateTime;

        public DateTime? LastAccessTime => null;

        public FileKind Kind => FileKind.None;

        public long Length => file.Size;

        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        public object ReferenceKey => file;

        public object DataKey => null;

        public Stream Open()
        {
            return file.OpenReadStream(Int64.MaxValue);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
