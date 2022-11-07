using IS4.SFI.Services;
using System;
using System.Collections.Generic;

namespace IS4.SFI.Media
{
    /// <summary>
    /// A general representation of an archive.
    /// </summary>
    public interface IArchiveInfo
    {
        /// <summary>
        /// True if the archive is complete, i.e. there are no following parts.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// True if the files in the archive are compressed as one single block.
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
}
