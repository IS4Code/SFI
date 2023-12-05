using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// A general representation of an archive.
    /// </summary>
    public interface IArchiveInfo
    {
        /// <summary>
        /// <see langword="true"/> if the archive is complete, i.e. there are no following parts.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// <see langword="true"/> if the files in the archive are compressed as one single block.
        /// </summary>
        bool IsSolid { get; }
    }

    /// <summary>
    /// Represents a browsable archive in memory.
    /// </summary>
    public interface IArchiveFile : IArchiveInfo
    {
        /// <summary>
        /// Contains the collection of all entries in the archive,
        /// as instances of <see cref="IArchiveEntry"/>.
        /// </summary>
        IEnumerable<IArchiveEntry> Entries { get; }
    }

    /// <summary>
    /// Represents an archive that can be only read once, as an
    /// <see cref="IEnumerator{T}"/>.
    /// </summary>
    public interface IArchiveReader : IArchiveInfo, IEnumerator<IArchiveEntry?>
    {
        /// <summary>
        /// Skips the current entry in the archive, without reading it.
        /// </summary>
        void Skip();
    }

    /// <summary>
    /// Represents an entry in an archive, usually also either
    /// a <see cref="IFileInfo"/> or <see cref="IDirectoryInfo"/>.
    /// </summary>
    public interface IArchiveEntry : IFileNodeInfo
    {
        /// <summary>
        /// The date and time when the file was archived, if present.
        /// </summary>
        DateTime? ArchivedTime { get; }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IArchiveEntry"/>,
    /// wrapping a <typeparamref name="TEntry"/> instance.
    /// </summary>
    /// <typeparam name="TEntry">The type of the archive entry.</typeparam>
    public abstract class ArchiveEntryWrapper<TEntry> : IArchiveEntry
    {
        /// <summary>
        /// The archive entry wrapped by the instance.
        /// </summary>
        protected TEntry? Entry { get; }

        /// <summary>
        /// Creates a new instance of the wrapper.
        /// </summary>
        /// <param name="entry">The entry instance to wrap.</param>
        public ArchiveEntryWrapper(TEntry? entry = default)
        {
            Entry = entry;
        }

        /// <inheritdoc/>
        public virtual string? Name => System.IO.Path.GetFileName(Path);

        /// <inheritdoc/>
        public virtual string? SubName => null;

        /// <inheritdoc/>
        public abstract string? Path { get; }

        /// <inheritdoc/>
        public virtual DateTime? CreationTime => null;

        /// <inheritdoc/>
        public virtual DateTime? LastWriteTime => null;

        /// <inheritdoc/>
        public virtual DateTime? LastAccessTime => null;

        /// <inheritdoc/>
        public virtual DateTime? ArchivedTime => null;

        /// <inheritdoc/>
        public virtual int? Revision => null;

        /// <inheritdoc cref="IIdentityKey.ReferenceKey"/>
        protected abstract object? ReferenceKey { get; }

        /// <inheritdoc cref="IIdentityKey.DataKey"/>
        protected abstract object? DataKey { get; }

        /// <inheritdoc/>
        object? IIdentityKey.ReferenceKey => ReferenceKey;

        /// <inheritdoc/>
        object? IIdentityKey.DataKey => DataKey;

        /// <inheritdoc/>
        public virtual FileKind Kind => FileKind.ArchiveItem;

        /// <inheritdoc/>
        public abstract FileAttributes Attributes { get; }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return Path == null ? null : "/" + Path;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IDirectoryInfo"/> as
    /// an archive entry, storing a grouping of <typeparamref name="TEntry"/>.
    /// </summary>
    /// <typeparam name="TArchive">The type of the archive.</typeparam>
    /// <typeparam name="TEntry">The type of the archive entry.</typeparam>
    public abstract class ArchiveDirectoryWrapper<TArchive, TEntry> : ArchiveEntryWrapper<TEntry>, IDirectoryInfo
    {
        readonly IGrouping<string?, DirectoryTools.EntryInfo<TEntry>> entries;

        readonly string? path;

        /// <summary>
        /// Provides properties of the entry represented as a file.
        /// </summary>
        protected ArchiveFileWrapper<TEntry>? AsFile { get; }

        /// <inheritdoc/>
        public override string? Name => AsFile?.Name ?? base.Name ?? entries.Key;

        /// <inheritdoc/>
        public override string? Path => AsFile?.Path ?? (path + entries.Key);

        /// <summary>
        /// The archive storing the directory.
        /// </summary>
        protected TArchive Archive { get; }

        /// <summary>
        /// Creates a new instance of the wrapper.
        /// </summary>
        /// <param name="archive">The value of <see cref="Archive"/>.</param>
        /// <param name="container">The <typeparamref name="TEntry"/> instance corresponding to the directory, if any.</param>
        /// <param name="path">The path of the directory.</param>
        /// <param name="entries">The grouping of the directory's entries.</param>
        public ArchiveDirectoryWrapper(TArchive archive, TEntry? container, string? path, IGrouping<string?, DirectoryTools.EntryInfo<TEntry>> entries) : base(container)
        {
            Archive = archive;
            this.entries = entries;
            this.path = path;
            if(IsValidFile(container))
            {
                AsFile = CreateFileWrapper(container);
            }
        }

        /// <summary>
        /// Checks whether <paramref name="entry"/> corresponds to a valid file.
        /// </summary>
        /// <param name="entry">The archive entry to check.</param>
        /// <returns><see langword="true"/> if <paramref name="entry"/> is a valid archive entry file, <see langword="false"/>, otherwise.</returns>
        /// <remarks>This method controls whether <see cref="AsFile"/> is stored or not.</remarks>
        protected abstract bool IsValidFile([NotNullWhen(true)] TEntry? entry);

        /// <inheritdoc/>
        public IEnumerable<IFileNodeInfo> Entries {
            get {
                foreach(var group in DirectoryTools.GroupByDirectories(entries, e => e.SubPath, e => e.Entry))
                {
                    if(group.Key == null)
                    {
                        // Enumerate the individual files in the directory
                        foreach(var entry in group)
                        {
                            if(!String.IsNullOrWhiteSpace(entry.SubPath))
                            {
                                yield return CreateFileWrapper(entry.Entry);
                            }
                        }
                    }else{
                        // Create a subdirectory info
                        yield return CreateDirectoryWrapper(Path + "/", group);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ArchiveFileWrapper{TEntry}"/>
        /// wrapping an arbitrary entry.
        /// </summary>
        /// <param name="entry">The <typeparamref name="TEntry"/> instance to wrap.</param>
        /// <returns>The wrapper info.</returns>
        protected abstract ArchiveFileWrapper<TEntry> CreateFileWrapper(TEntry entry);

        /// <summary>
        /// Creates a new instance of <see cref="ArchiveDirectoryWrapper{TArchive, TEntry}"/>
        /// wrapping a group of files.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="entries">The group of files to wrap.</param>
        /// <returns>The wrapper info.</returns>
        protected abstract ArchiveDirectoryWrapper<TArchive, TEntry> CreateDirectoryWrapper(string path, IGrouping<string?, DirectoryTools.EntryInfo<TEntry>> entries);

        /// <inheritdoc/>
        public Environment.SpecialFolder? SpecialFolderType => null;
        
        /// <inheritdoc/>
        public override FileAttributes Attributes {
            get {
                var entryAttributes = AsFile?.Attributes;
                if((entryAttributes ?? FileAttributes.Normal) == FileAttributes.Normal)
                {
                    return FileAttributes.Directory;
                }
                return entryAttributes.GetValueOrDefault() | FileAttributes.Directory;
            }
        }

        /// <inheritdoc/>
        public override DateTime? ArchivedTime => AsFile?.ArchivedTime ?? base.ArchivedTime;

        /// <inheritdoc/>
        public override DateTime? CreationTime => AsFile?.CreationTime ?? base.CreationTime;

        /// <inheritdoc/>
        public override DateTime? LastAccessTime => AsFile?.LastAccessTime ?? base.LastAccessTime;

        /// <inheritdoc/>
        public override DateTime? LastWriteTime => AsFile?.LastWriteTime ?? base.LastWriteTime;

        /// <inheritdoc/>
        public override int? Revision => AsFile?.Revision ?? base.Revision;

        /// <inheritdoc/>
        public override string? SubName => AsFile?.SubName ?? base.SubName;

        /// <inheritdoc/>
        protected override object? ReferenceKey => AsFile != null ? ((IIdentityKey)AsFile).ReferenceKey : Archive;

        /// <inheritdoc/>
        protected override object? DataKey => AsFile != null ? ((IIdentityKey)AsFile).DataKey : Path;
    }

    /// <summary>
    /// Provides an implementation of <see cref="IFileInfo"/> as
    /// an archive directory, storing both a grouping of <typeparamref name="TEntry"/>
    /// and custom data.
    /// </summary>
    /// <typeparam name="TArchive">The type of the archive.</typeparam>
    /// <typeparam name="TEntry">The type of the archive entry.</typeparam>
    public abstract class ArchiveFileDirectoryWrapper<TArchive, TEntry> : ArchiveDirectoryWrapper<TArchive, TEntry>, IFileInfo
    {
        /// <inheritdoc cref="ArchiveDirectoryWrapper{TArchive, TEntry}.AsFile"/>
        protected new ArchiveFileWrapper<TEntry> AsFile => base.AsFile!;

        /// <inheritdoc cref="ArchiveDirectoryWrapper{TArchive, TEntry}.ArchiveDirectoryWrapper(TArchive, TEntry, string, IGrouping{string, DirectoryTools.EntryInfo{TEntry}})"/>
        public ArchiveFileDirectoryWrapper(TArchive archive, TEntry container, IGrouping<string?, DirectoryTools.EntryInfo<TEntry>> entries) : base(archive, container, null, entries)
        {

        }

        /// <inheritdoc/>
        public long Length => AsFile.Length;

        /// <inheritdoc/>
        public StreamFactoryAccess Access => AsFile.Access;

        /// <inheritdoc/>
        public Stream Open()
        {
            return AsFile.Open();
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IFileInfo"/> as
    /// an archive file entry.
    /// </summary>
    /// <typeparam name="TEntry">The type of the archive entry.</typeparam>
    public abstract class ArchiveFileWrapper<TEntry> : ArchiveEntryWrapper<TEntry>, IFileInfo
    {
        /// <inheritdoc cref="ArchiveEntryWrapper{TEntry}.Entry"/>
        protected new TEntry Entry => base.Entry!;

        /// <inheritdoc cref="ArchiveEntryWrapper{TEntry}.ArchiveEntryWrapper(TEntry)"/>
        public ArchiveFileWrapper(TEntry entry) : base(entry)
        {

        }

        /// <inheritdoc/>
        public abstract long Length { get; }

        /// <inheritdoc/>
        public abstract StreamFactoryAccess Access { get; }

        /// <inheritdoc/>
        public abstract Stream Open();
    }
}
