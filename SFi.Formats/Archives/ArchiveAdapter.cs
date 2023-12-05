using IS4.SFI.Services;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ISharpCompressArchive = SharpCompress.Archives.IArchive;
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
                        yield return ArchiveDirectoryInfo.Create(archive, "", group);
                    }
                }
            }
        }

        readonly ISharpCompressArchive archive;

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

        class ArchiveDirectoryInfo : ArchiveDirectoryWrapper<ISharpCompressArchive, ISharpCompressArchiveEntry>
        {
            protected ArchiveDirectoryInfo(ISharpCompressArchive archive, ISharpCompressArchiveEntry? container, string? path, IArchiveEntryGrouping entries) : base(archive, container, path, entries)
            {

            }

            protected override bool IsValidFile(ISharpCompressArchiveEntry? entry)
            {
                return entry != null;
            }

            protected override ArchiveFileWrapper<ISharpCompressArchiveEntry> CreateFileWrapper(ISharpCompressArchiveEntry entry)
            {
                return new ArchiveFileInfo(entry);
            }

            protected override ArchiveDirectoryWrapper<ISharpCompressArchive, ISharpCompressArchiveEntry> CreateDirectoryWrapper(string path, IArchiveEntryGrouping entries)
            {
                return Create(Archive, path, entries);
            }

            public static ArchiveDirectoryWrapper<ISharpCompressArchive, ISharpCompressArchiveEntry> Create(ISharpCompressArchive archive, string path, IArchiveEntryGrouping group)
            {
                var container = group.FirstOrDefault(p => String.IsNullOrEmpty(p.SubPath));
                if(container.Entry != null && container.Entry.Size > 0)
                {
                    return new ArchiveFileDirectoryInfo(archive, container.Entry, group);
                }else{
                    return new ArchiveDirectoryInfo(archive, container.Entry, path, group);
                }
            }
        }

        class ArchiveFileDirectoryInfo : ArchiveFileDirectoryWrapper<ISharpCompressArchive, ISharpCompressArchiveEntry>
        {
            public ArchiveFileDirectoryInfo(ISharpCompressArchive archive, ISharpCompressArchiveEntry container, IArchiveEntryGrouping entries) : base(archive, container, entries)
            {

            }

            protected override bool IsValidFile(ISharpCompressArchiveEntry? entry)
            {
                return entry != null;
            }

            protected override ArchiveDirectoryWrapper<ISharpCompressArchive, ISharpCompressArchiveEntry> CreateDirectoryWrapper(string path, IArchiveEntryGrouping entries)
            {
                return ArchiveDirectoryInfo.Create(Archive, path, entries);
            }

            protected override ArchiveFileWrapper<ISharpCompressArchiveEntry> CreateFileWrapper(ISharpCompressArchiveEntry entry)
            {
                return new ArchiveFileInfo(entry);
            }
        }

        class ArchiveFileInfo : ArchiveFileWrapper<ISharpCompressArchiveEntry>
        {
            public ArchiveFileInfo(ISharpCompressArchiveEntry entry) : base(entry)
            {

            }

            /// <inheritdoc/>
            public override string? Name => Entry != null ? System.IO.Path.GetFileName(Path) : null;

            /// <inheritdoc/>
            public override string? Path => ExtractPathSimple(Entry);

            /// <inheritdoc/>
            public override DateTime? CreationTime => Entry?.CreatedTime;

            /// <inheritdoc/>
            public override DateTime? LastWriteTime => Entry?.LastModifiedTime;

            /// <inheritdoc/>
            public override DateTime? LastAccessTime => Entry?.LastAccessedTime;

            /// <inheritdoc/>
            public override DateTime? ArchivedTime => Entry?.LastAccessedTime;

            public override long Length => Entry!.Size;

            public override FileAttributes Attributes => Entry!.IsEncrypted ? FileAttributes.Encrypted : FileAttributes.Normal;

            public override StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            protected override object? ReferenceKey => Entry;

            protected override object? DataKey => null;

            public override Stream Open()
            {
                return Entry!.OpenEntryStream();
            }
        }
    }
}
