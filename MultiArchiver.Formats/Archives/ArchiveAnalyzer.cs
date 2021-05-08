﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : BinaryFormatAnalyzer<IArchive>
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
                        var node2 = nodeFactory.Create(node, new ArchiveFileInfo(entry.Entry));
                        if(node2 != null)
                        {
                            node2.SetClass(Classes.ArchiveItem);
                            node2.Set(Properties.BelongsToContainer, node);
                        }
                    }
                }else{
                    var node2 = nodeFactory.Create(node, ArchiveDirectoryInfo.Create("", group));
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
            if(entry != null && entry.IsDirectory && !path.EndsWith('/'))
            {
                path += "/";
            }
            return path;
        }

        static readonly char[] trimChars = { '/' };

        private static string ExtractPathSimple(IArchiveEntry entry)
        {
            if(entry?.Key == null) return null;
            var path = entry.Key.Replace(Path.DirectorySeparatorChar, '/');
            if(entry.IsDirectory) path = path.TrimEnd(trimChars);
            return path;
        }

        abstract class ArchiveEntryInfo : IFileNodeInfo
        {
            protected IArchiveEntry Entry { get; }

            public ArchiveEntryInfo(IArchiveEntry entry)
            {
                Entry = entry;
            }

            public virtual string Name => Entry != null ? System.IO.Path.GetFileName(Path) : null;

            public virtual string Path => ExtractPathSimple(Entry);

            public DateTime? CreationTime => Entry?.CreatedTime;

            public DateTime? LastWriteTime => Entry?.LastModifiedTime;

            public DateTime? LastAccessTime => Entry?.LastAccessedTime;

            public int? Revision => null;

            protected virtual object ReferenceKey => Entry?.Archive;

            object IPersistentKey.ReferenceKey => ReferenceKey;

            object IPersistentKey.DataKey => Entry?.Key;
        }

        class ArchiveDirectoryInfo : ArchiveEntryInfo, IDirectoryInfo
        {
            readonly IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries;

            readonly string path;

            public override string Name => base.Name ?? entries.Key;

            public override string Path => base.Path ?? path + entries.Key + "/";

            protected ArchiveDirectoryInfo(IArchiveEntry container, string path, IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries) : base(container)
            {
                this.entries = entries;
                this.path = path;
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
                                    yield return new ArchiveFileInfo(entry.Entry);
                                }
                            }
                        }else{
                            yield return Create(Path, group);
                        }
                    }
                }
            }

            protected override object ReferenceKey => base.ReferenceKey ?? entries.FirstOrDefault().Entry?.Archive;

            public static ArchiveDirectoryInfo Create(string path, IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> group)
            {
                var container = group.FirstOrDefault(p => String.IsNullOrEmpty(p.SubPath));
                if(container.Entry != null && container.Entry.Size > 0)
                {
                    return new ArchiveFileDirectoryInfo(container.Entry, group);
                }else{
                    return new ArchiveDirectoryInfo(container.Entry, path, group);
                }
            }
        }

        class ArchiveFileDirectoryInfo : ArchiveDirectoryInfo, IFileInfo
        {
            readonly ArchiveFileInfo entryInfo;

            public ArchiveFileDirectoryInfo(IArchiveEntry container, IGrouping<string, DirectoryTools.EntryInfo<IArchiveEntry>> entries) : base(container, null, entries)
            {
                entryInfo = new ArchiveFileInfo(container);
            }

            public long Length => entryInfo.Length;

            public bool IsThreadSafe => entryInfo.IsThreadSafe;

            public Stream Open()
            {
                return entryInfo.Open();
            }
        }

        class ArchiveFileInfo : ArchiveEntryInfo, IFileInfo
        {
            public ArchiveFileInfo(IArchiveEntry entry) : base(entry)
            {

            }

            public long Length => Entry.Size;

            public bool IsThreadSafe => true;

            public Stream Open()
            {
                return Entry.OpenEntryStream();
            }
        }
    }
}
