using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : FormatAnalyzer<IArchive>
    {
        public ArchiveAnalyzer() : base(Classes.Archive)
        {

        }

        public override bool Analyze(ILinkedNode node, IArchive archive, ILinkedNodeFactory nodeFactory)
        {
            foreach(var group in DirectoryTools.GroupByDirectories(archive.Entries, ExtractPath))
            {
                if(group.Key == null)
                {
                    foreach(var entry in group)
                    {
                        var node2 = nodeFactory.Create(node, new ArchiveEntryInfo(entry.Entry));
                        if(node2 != null)
                        {
                            node2.SetClass(Classes.ArchiveItem);
                            node2.Set(Properties.BelongsToContainer, node);
                        }
                    }
                }else{
                    var node2 = nodeFactory.Create(node, ArchiveDirectoryInfo.Create(group));
                    if(node2 != null)
                    {
                        node2.SetClass(Classes.ArchiveItem);
                        node2.Set(Properties.BelongsToContainer, node);
                    }
                }
            }
            return false;
        }

        private static string ExtractPath(IArchiveEntry entry)
        {
            var path = ExtractPathSimple(entry);
            if(entry != null && entry.IsDirectory)
            {
                path += "/";
            }
            return path;
        }

        private static string ExtractPathSimple(IArchiveEntry entry)
        {
            if(entry == null) return null;
            return entry.Key.Replace(Path.DirectorySeparatorChar, '/');
        }
        
        class ArchiveDirectoryInfo : IDirectoryInfo
        {
            readonly IArchiveEntry container;
            readonly IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries;

            protected ArchiveDirectoryInfo(IArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries)
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
                                    yield return new ArchiveEntryInfo(entry.Entry);
                                }
                            }
                        }else{
                            yield return Create(group);
                        }
                    }
                }
            }

            public string Name => entries.Key;

            public string Path => ExtractPathSimple(container);

            public DateTime? CreationTime => container?.CreatedTime;

            public DateTime? LastWriteTime => container?.LastModifiedTime;

            public DateTime? LastAccessTime => container?.LastAccessedTime;

            public object ReferenceKey => container?.Archive ?? entries.FirstOrDefault().Entry?.Archive;

            public object DataKey => container?.Key;

            public static ArchiveDirectoryInfo Create(IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> group)
            {
                var container = group.FirstOrDefault(p => String.IsNullOrEmpty(p.SubPath));
                if(container.Entry != null && container.Entry.Size > 0)
                {
                    return new ArchiveFileDirectoryInfo(container.Entry, group);
                }else{
                    return new ArchiveDirectoryInfo(container.Entry, group);
                }
            }
        }

        class ArchiveFileDirectoryInfo : ArchiveDirectoryInfo, IFileInfo
        {
            readonly ArchiveEntryInfo entryInfo;

            public ArchiveFileDirectoryInfo(IArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries) : base(container, entries)
            {
                this.entryInfo = new ArchiveEntryInfo(container);
            }

            public long Length => entryInfo.Length;

            public bool IsThreadSafe => entryInfo.IsThreadSafe;

            public Stream Open()
            {
                return entryInfo.Open();
            }
        }

        class ArchiveEntryInfo : IFileInfo
        {
            readonly IArchiveEntry entry;
            readonly Lazy<byte[]> data;

            public ArchiveEntryInfo(IArchiveEntry entry) : this(entry, new Lazy<byte[]>(() => {
                var buffer = new byte[entry.Size];
                using(var stream = new MemoryStream(buffer, true))
                {
                    using(var inner = entry.OpenEntryStream())
                    {
                        inner.CopyTo(stream);
                    }
                    return buffer;
                }
            }))
            {

            }

            public ArchiveEntryInfo(IArchiveEntry entry, Lazy<byte[]> data)
            {
                this.entry = entry;
                this.data = data;
            }

            public string Name => System.IO.Path.GetFileName(entry.Key);

            public string Path => ExtractPathSimple(entry);

            public long Length => entry.Size;

            public DateTime? CreationTime => entry.CreatedTime;

            public DateTime? LastWriteTime => entry.LastModifiedTime;

            public DateTime? LastAccessTime => entry.LastAccessedTime;

            public bool IsThreadSafe => true;

            object IPersistentKey.ReferenceKey => entry.Archive;

            object IPersistentKey.DataKey => entry.Key;

            public Stream Open()
            {
                return new MemoryStream(data.Value, false);
            }
        }
    }
}
