using IS4.MultiArchiver.Services;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;

namespace IS4.MultiArchiver.WebApp
{
    public class BrowserFileInfo : IFileInfo
    {
        readonly IBrowserFile file;

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
