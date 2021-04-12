using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : FormatAnalyzer<ZipArchive>
    {
        public ArchiveAnalyzer() : base(Classes.Archive)
        {

        }

        public override void Analyze(ILinkedNode node, ZipArchive archive, ILinkedNodeFactory nodeFactory)
        {
            foreach(var entry in archive.Entries)
            {
                var node2 = nodeFactory.Create(node, new ZipEntryInfo(entry));
                if(node2 != null)
                {
                    node2.SetClass(Classes.ArchiveItem);
                    node2.Set(Properties.BelongsToContainer, node);
                }
            }
        }

        class ZipEntryInfo : IFileInfo
        {
            readonly ZipArchiveEntry entry;
            readonly Lazy<byte[]> data;

            public ZipEntryInfo(ZipArchiveEntry entry) : this(entry, new Lazy<byte[]>(() => {
                var buffer = new byte[entry.Length];
                using(var stream = new MemoryStream(buffer, true))
                {
                    using(var inner = entry.Open())
                    {
                        inner.CopyTo(stream);
                    }
                    return buffer;
                }
            }))
            {

            }

            public ZipEntryInfo(ZipArchiveEntry entry, Lazy<byte[]> data)
            {
                this.entry = entry;
                this.data = data;
            }

            public string Name => entry.Name;

            public string Path => entry.FullName;

            public long Length => entry.Length;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => entry.LastWriteTime.UtcDateTime;

            public DateTime? LastAccessTime => null;

            public bool IsThreadSafe => true;

            object IPersistentKey.ReferenceKey => entry.Archive;

            object IPersistentKey.DataKey => entry.FullName;

            public Stream Open()
            {
                return new MemoryStream(data.Value, false);
            }
        }
    }
}
