using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ZipArchiveAnalyzer : FormatAnalyzer<ZipArchive>
    {
        public ZipArchiveAnalyzer() : base(Classes.Archive)
        {

        }

        public override bool Analyze(ILinkedNode node, ZipArchive archive, ILinkedNodeFactory nodeFactory)
        {
            foreach(var group in DirectoryTools.GroupByDirectories(archive.Entries, e => e.FullName))
            {
                if(group.Key == null)
                {
                    foreach(var entry in group)
                    {
                        var node2 = nodeFactory.Create(node, new ZipEntryInfo(entry.Entry));
                        if(node2 != null)
                        {
                            node2.SetClass(Classes.ArchiveItem);
                            node2.Set(Properties.BelongsToContainer, node);
                        }
                    }
                }else{
                    var node2 = nodeFactory.Create(node, ZipDirectoryInfo.Create(group));
                    if(node2 != null)
                    {
                        node2.SetClass(Classes.ArchiveItem);
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }
            }
            return false;
        }

        class ZipDirectoryInfo : IDirectoryInfo
        {
            readonly ZipArchiveEntry container;
            readonly IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries;

            protected ZipDirectoryInfo(ZipArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries)
            {
                this.container = container;
                this.entries = entries;
            }

            public IEnumerable<IFileNodeInfo> Entries{
                get{
                    foreach(var group in DirectoryTools.GroupByDirectories(entries, e => e.SubPath, e => e.Entry))
                    {
                        if(group.Key == null)
                        {
                            foreach(var entry in group)
                            {
                                if(!String.IsNullOrWhiteSpace(entry.SubPath))
                                {
                                    yield return new ZipEntryInfo(entry.Entry);
                                }
                            }
                        }else{
                            yield return Create(group);
                        }
                    }
                }
            }

            public string Name => entries.Key;

            public string Path => container?.FullName;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => container?.LastWriteTime.UtcDateTime;

            public DateTime? LastAccessTime => null;

            public object ReferenceKey => container?.Archive ?? entries.FirstOrDefault().Entry?.Archive;

            public object DataKey => container?.FullName;

            public static ZipDirectoryInfo Create(IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> group)
            {
                var container = group.FirstOrDefault(p => String.IsNullOrEmpty(p.SubPath));
                if(container.Entry != null && container.Entry.Length > 0)
                {
                    return new ZipFileDirectoryInfo(container.Entry, group);
                }else{
                    return new ZipDirectoryInfo(container.Entry, group);
                }
            }
        }

        class ZipFileDirectoryInfo : ZipDirectoryInfo, IFileInfo
        {
            readonly ZipEntryInfo entryInfo;

            public ZipFileDirectoryInfo(ZipArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries) : base(container, entries)
            {
                this.entryInfo = new ZipEntryInfo(container);
            }

            public long Length => entryInfo.Length;

            public bool IsThreadSafe => entryInfo.IsThreadSafe;

            public Stream Open()
            {
                return entryInfo.Open();
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
