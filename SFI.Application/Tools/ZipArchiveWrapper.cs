using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace IS4.SFI.Application.Tools
{
    internal class ZipArchiveWrapper : IDirectoryInfo
    {
        readonly ZipArchive archive;

        public ZipArchiveWrapper(ZipArchive archive)
        {
            this.archive = archive;
        }

        public IEnumerable<IFileNodeInfo> Entries => archive.Entries.Select(e => new ZipEntryWrapper(e));

        public string? Name => null;
        public string? SubName => null;
        public string? Path => null;
        public int? Revision => null;
        public DateTime? CreationTime => null;
        public DateTime? LastWriteTime => null;
        public DateTime? LastAccessTime => null;
        public FileKind Kind => FileKind.ArchiveItem;
        public FileAttributes Attributes => FileAttributes.Directory;
        public Environment.SpecialFolder? SpecialFolderType => null;
        public object? ReferenceKey => archive;
        public object? DataKey => null;

        class ZipEntryWrapper : IFileInfo
        {
            readonly ZipArchiveEntry entry;

            public ZipEntryWrapper(ZipArchiveEntry entry)
            {
                this.entry = entry;
            }

            public string? Name => entry.Name;
            public string? SubName => null;
            public string? Path => entry.FullName;
            public int? Revision => null;
            public DateTime? CreationTime => null;
            public DateTime? LastWriteTime => entry.LastWriteTime.UtcDateTime;
            public DateTime? LastAccessTime => null;
            public FileKind Kind => FileKind.ArchiveItem;
            public FileAttributes Attributes => FileAttributes.Normal;
            public long Length => entry.Length;
            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;
            public object? ReferenceKey => entry;
            public object? DataKey => null;

            public Stream Open()
            {
                // DEFLATE stream does not support Length, so use a buffer
                var buffer = new MemoryStream();
                using(var stream = entry.Open())
                {
                    stream.CopyTo(buffer);
                }
                if(!buffer.TryGetBuffer(out var segment))
                {
                    segment = new ArraySegment<byte>(buffer.ToArray());
                }
                return new MemoryStream(segment.Array!, segment.Offset, segment.Count, false);
            }
        }
    }
}
