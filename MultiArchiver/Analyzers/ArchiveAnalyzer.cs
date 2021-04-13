using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : FormatAnalyzer<ZipArchive>
    {
        public ArchiveAnalyzer() : base(Classes.Archive)
        {

        }

        public override void Analyze(ILinkedNode node, ZipArchive archive, ILinkedNodeFactory nodeFactory)
        {
            foreach(var group in archive.Entries.Select(e => (d: GetFirstDir(e.FullName), e)).GroupBy(p => p.d.dir))
            {
                if(group.Key == null)
                {
                    foreach(var entry in group)
                    {
                        var node2 = nodeFactory.Create(node, new ZipEntryInfo(entry.e));
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
        }

        static (string dir, string subpath) GetFirstDir(string path)
        {
            int index = path.IndexOf('/');
            if(index == -1) return (null, path);
            return (path.Substring(0, index), path.Substring(index + 1));
        }

        class ZipDirectoryInfo : IDirectoryInfo
        {
            readonly ZipArchiveEntry container;
            readonly IGrouping<string, ((string dir, string subpath) d, ZipArchiveEntry e)> entries;

            protected ZipDirectoryInfo(ZipArchiveEntry container, IGrouping<string, ((string dir, string subpath) d, ZipArchiveEntry e)> entries)
            {
                this.container = container;
                this.entries = entries;
            }

            public IEnumerable<IFileNodeInfo> Entries{
                get{
                    foreach(var group in entries.Select(p => (d: GetFirstDir(p.d.subpath), p.e)).GroupBy(p => p.d.dir))
                    {
                        if(group.Key == null)
                        {
                            foreach(var entry in group)
                            {
                                if(!String.IsNullOrWhiteSpace(entry.d.subpath))
                                {
                                    yield return new ZipEntryInfo(entry.e);
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

            public object ReferenceKey => container?.Archive;

            public object DataKey => container?.FullName;

            public static ZipDirectoryInfo Create(IGrouping<string, ((string dir, string subpath) d, ZipArchiveEntry e)> group)
            {
                var container = group.FirstOrDefault(p => String.IsNullOrEmpty(p.d.subpath));
                if(container.e != null && container.e.Length > 0)
                {
                    return new ZipFileDirectoryInfo(container.e, group);
                }else{
                    return new ZipDirectoryInfo(container.e, group);
                }
            }
        }

        class ZipFileDirectoryInfo : ZipDirectoryInfo, IFileInfo
        {
            readonly ZipEntryInfo entryInfo;

            public ZipFileDirectoryInfo(ZipArchiveEntry container, IGrouping<string, ((string, string), ZipArchiveEntry)> entries) : base(container, entries)
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
