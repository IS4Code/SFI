using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : ClassRecognizingAnalyzer<ZipArchive>
    {
        public ArchiveAnalyzer() : base(Classes.Archive)
        {

        }

        public override ILinkedNode Analyze(ZipArchive archive, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(archive, nodeFactory);
            if(node != null)
            {
                foreach(var entry in archive.Entries)
                {
                    var node2 = nodeFactory.Create(new ZipEntryInfo(node, entry));
                    if(node2 != null)
                    {
                        node2.SetClass(Classes.ArchiveItem);
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }
            }
            return node;
        }

        class ZipEntryInfo : IFileInfo
        {
            public ILinkedNode Container { get; }
            readonly ZipArchiveEntry entry;
            readonly Lazy<byte[]> data;

            public ZipEntryInfo(ILinkedNode container, ZipArchiveEntry entry) : this(container, entry, new Lazy<byte[]>(() => {
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

            public ZipEntryInfo(ILinkedNode container, ZipArchiveEntry entry, Lazy<byte[]> data)
            {
                Container = container;
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

            public IFileNodeInfo WithContainer(ILinkedNode container)
            {
                return new ZipEntryInfo(container, entry, data);
            }
        }
    }
}
