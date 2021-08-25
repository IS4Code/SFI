using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ISharpCompressArchiveEntry = SharpCompress.Archives.IArchiveEntry;

namespace IS4.MultiArchiver.Formats.Archives
{
    using IArchiveEntryGrouping = IGrouping<string, DirectoryTools.EntryInfo<ISharpCompressArchiveEntry>>;

    public class ArchiveAdapter : IArchiveFile
    {
        public IEnumerable<IArchiveEntry> Entries {
            get {
                foreach(var group in DirectoryTools.GroupByDirectories(archive.Entries, ExtractPath))
                {
                    if(group.Key == null)
                    {
                        foreach(var entry in group)
                        {
                            yield return new ArchiveFileInfo(entry.Entry);
                        }
                    }else{
                        yield return ArchiveDirectoryInfo.Create("", group);
                    }
                }
            }
        }

        readonly SharpCompress.Archives.IArchive archive;

        public ArchiveAdapter(SharpCompress.Archives.IArchive archive)
        {
            this.archive = archive;
        }

        internal static string ExtractPath(IEntry entry)
        {
            var path = ExtractPathSimple(entry);
            if(entry != null && entry.IsDirectory && !path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        static readonly char[] trimChars = { '/' };

        internal static string ExtractPathSimple(IEntry entry)
        {
            if(entry?.Key == null) return null;
            var path = entry.Key.Replace(Path.DirectorySeparatorChar, '/');
            if(entry.IsDirectory) path = path.TrimEnd(trimChars);
            return path;
        }

        abstract class ArchiveEntryInfo : IArchiveEntry
        {
            protected ISharpCompressArchiveEntry Entry { get; }

            public ArchiveEntryInfo(ISharpCompressArchiveEntry entry)
            {
                Entry = entry;
            }

            public virtual string Name => Entry != null ? System.IO.Path.GetFileName(Path) : null;

            public string SubName => null;

            public virtual string Path => ExtractPathSimple(Entry);

            public DateTime? CreationTime => Entry?.CreatedTime;

            public DateTime? LastWriteTime => Entry?.LastModifiedTime;

            public DateTime? LastAccessTime => Entry?.LastAccessedTime;

            public DateTime? ArchivedTime => Entry?.LastAccessedTime;

            public int? Revision => null;

            protected virtual object ReferenceKey => Entry?.Archive;

            object IPersistentKey.ReferenceKey => ReferenceKey;

            object IPersistentKey.DataKey => Entry?.Key;

            public FileKind Kind => FileKind.ArchiveItem;

            public override string ToString()
            {
                return "/" + Path;
            }
        }

        class ArchiveDirectoryInfo : ArchiveEntryInfo, IDirectoryInfo
        {
            readonly IArchiveEntryGrouping entries;

            readonly string path;

            public override string Name => base.Name ?? entries.Key;

            public override string Path => base.Path ?? path + entries.Key;

            protected ArchiveDirectoryInfo(ISharpCompressArchiveEntry container, string path, IArchiveEntryGrouping entries) : base(container)
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
                            yield return Create(Path + "/", group);
                        }
                    }
                }
            }

            protected override object ReferenceKey => base.ReferenceKey ?? entries.FirstOrDefault().Entry?.Archive;

            public static ArchiveDirectoryInfo Create(string path, IArchiveEntryGrouping group)
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

            public ArchiveFileDirectoryInfo(ISharpCompressArchiveEntry container, IArchiveEntryGrouping entries) : base(container, null, entries)
            {
                entryInfo = new ArchiveFileInfo(container);
            }

            public long Length => entryInfo.Length;

            public bool IsEncrypted => entryInfo.IsEncrypted;

            public StreamFactoryAccess Access => entryInfo.Access;

            public Stream Open()
            {
                return entryInfo.Open();
            }
        }

        class ArchiveFileInfo : ArchiveEntryInfo, IFileInfo
        {
            public ArchiveFileInfo(ISharpCompressArchiveEntry entry) : base(entry)
            {

            }

            public long Length => Entry.Size;

            public bool IsEncrypted => Entry.IsEncrypted;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public Stream Open()
            {
                return Entry.OpenEntryStream();
            }
        }
    }
}
