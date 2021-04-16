using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ZipArchiveAnalyzer : BinaryFormatAnalyzer<ZipArchive>
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
                        var node2 = nodeFactory.Create(node, new ZipFileInfo(entry.Entry));
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

        abstract class ZipEntryInfo : IFileNodeInfo
        {
            protected ZipArchiveEntry Entry { get; }

            public ZipEntryInfo(ZipArchiveEntry entry)
            {
                Entry = entry;
            }

            public string Name => Entry?.Name;

            public string Path => Entry?.FullName;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => Entry?.LastWriteTime.UtcDateTime;

            public DateTime? LastAccessTime => null;

            protected virtual object ReferenceKey => Entry?.Archive;

            object IPersistentKey.ReferenceKey => ReferenceKey;

            object IPersistentKey.DataKey => Entry?.FullName;
        }

        class ZipDirectoryInfo : ZipEntryInfo, IDirectoryInfo
        {
            readonly IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries;

            protected ZipDirectoryInfo(ZipArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries) : base(container)
            {
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
                                    yield return new ZipFileInfo(entry.Entry);
                                }
                            }
                        }else{
                            yield return Create(group);
                        }
                    }
                }
            }

            protected override object ReferenceKey => base.ReferenceKey ?? entries.FirstOrDefault().Entry?.Archive;

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
            readonly ZipFileInfo entryInfo;

            public ZipFileDirectoryInfo(ZipArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<ZipArchiveEntry>> entries) : base(container, entries)
            {
                entryInfo = new ZipFileInfo(container);
            }

            public long Length => entryInfo.Length;

            public bool IsThreadSafe => entryInfo.IsThreadSafe;

            public Stream Open()
            {
                return entryInfo.Open();
            }
        }

        class ZipFileInfo : ZipEntryInfo, IFileInfo
        {
            readonly Lazy<byte[]> data;

            public ZipFileInfo(ZipArchiveEntry entry) : base(entry)
            {
                data = new Lazy<byte[]>(() => {
                    var buffer = new byte[entry.Length];
                    using(var stream = new MemoryStream(buffer, true))
                    {
                        using(var inner = entry.Open())
                        {
                            inner.CopyTo(stream);
                        }
                        return buffer;
                    }
                });
            }

            public long Length => Entry.Length;

            public bool IsThreadSafe => true;

            public Stream Open()
            {
                return new MemoryStream(data.Value, false);
            }
        }
    }
}
