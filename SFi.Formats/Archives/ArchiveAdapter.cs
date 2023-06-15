using IS4.SFI.Services;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ISharpCompressArchiveEntry = SharpCompress.Archives.IArchiveEntry;

namespace IS4.SFI.Formats.Archives
{
    using IArchiveEntryGrouping = IGrouping<string?, DirectoryTools.EntryInfo<ISharpCompressArchiveEntry>>;

    /// <summary>
    /// Adapts an instance of <see cref="SharpCompress.Archives.IArchive"/> using
    /// an implementation of <see cref="IArchiveFile"/>.
    /// </summary>
    public class ArchiveAdapter : IArchiveFile
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool IsComplete => archive.IsComplete;

        /// <inheritdoc/>
        public bool IsSolid => archive.IsSolid;

        /// <summary>
        /// Creates a new instance of the archive.
        /// </summary>
        /// <param name="archive">The underlying archive to use.</param>
        public ArchiveAdapter(SharpCompress.Archives.IArchive archive)
        {
            this.archive = archive;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return archive.ToString();
        }

        /// <summary>
        /// Extracts the path of an archive entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the path from.</param>
        /// <returns>
        /// The normalized value of <see cref="IEntry.Key"/>. If the entry
        /// is a directory, the path ends on '/'.
        /// </returns>
        internal static string? ExtractPath(IEntry entry)
        {
            var path = ExtractPathSimple(entry);
            if(path != null && entry != null && entry.IsDirectory && !path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        static readonly char[] trimChars = { '/' };

        /// <summary>
        /// Extracts the path of an archive entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the path from.</param>
        /// <returns>
        /// The normalized value of <see cref="IEntry.Key"/>. If the entry
        /// is a directory, the path doesn't end on '/'.
        /// </returns>
        internal static string? ExtractPathSimple(IEntry? entry)
        {
            if(entry?.Key == null) return null;
            var path = entry.Key.Replace(Path.DirectorySeparatorChar, '/');
            if(entry.IsDirectory) path = path.TrimEnd(trimChars);
            return path;
        }

        abstract class ArchiveEntryInfo : IArchiveEntry
        {
            protected ISharpCompressArchiveEntry? Entry { get; }

            public ArchiveEntryInfo(ISharpCompressArchiveEntry? entry)
            {
                Entry = entry;
            }

            /// <inheritdoc/>
            public virtual string? Name => Entry != null ? System.IO.Path.GetFileName(Path) : null;

            /// <inheritdoc/>
            public string? SubName => null;

            /// <inheritdoc/>
            public virtual string? Path => ExtractPathSimple(Entry);

            /// <inheritdoc/>
            public DateTime? CreationTime => Entry?.CreatedTime;

            /// <inheritdoc/>
            public DateTime? LastWriteTime => Entry?.LastModifiedTime;

            /// <inheritdoc/>
            public DateTime? LastAccessTime => Entry?.LastAccessedTime;

            /// <inheritdoc/>
            public DateTime? ArchivedTime => Entry?.LastAccessedTime;

            /// <inheritdoc/>
            public int? Revision => null;

            /// <inheritdoc cref="IIdentityKey.ReferenceKey"/>
            protected virtual object? ReferenceKey => Entry?.Archive;

            /// <inheritdoc/>
            object? IIdentityKey.ReferenceKey => ReferenceKey;

            /// <inheritdoc/>
            object? IIdentityKey.DataKey => Entry?.Key;

            /// <inheritdoc/>
            public FileKind Kind => FileKind.ArchiveItem;

            /// <inheritdoc/>
            public abstract FileAttributes Attributes { get; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return "/" + Path;
            }
        }

        class ArchiveDirectoryInfo : ArchiveEntryInfo, IDirectoryInfo
        {
            readonly IArchiveEntryGrouping entries;

            readonly string? path;

            public override string? Name => base.Name ?? entries.Key;

            public override string? Path => base.Path ?? path + entries.Key;

            protected ArchiveDirectoryInfo(ISharpCompressArchiveEntry? container, string? path, IArchiveEntryGrouping entries) : base(container)
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

            /// <inheritdoc/>
            public override FileAttributes Attributes => FileAttributes.Directory;

            /// <inheritdoc/>
            public Environment.SpecialFolder? SpecialFolderType => null;

            protected override object? ReferenceKey => base.ReferenceKey ?? entries.FirstOrDefault().Entry?.Archive;

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

            public override FileAttributes Attributes => FileAttributes.Directory | (Entry!.IsEncrypted ? FileAttributes.Encrypted : 0);

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

            public long Length => Entry!.Size;

            public override FileAttributes Attributes => Entry!.IsEncrypted ? FileAttributes.Encrypted : FileAttributes.Normal;

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public Stream Open()
            {
                return Entry!.OpenEntryStream();
            }
        }
    }
}
