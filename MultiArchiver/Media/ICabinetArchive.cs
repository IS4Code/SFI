using System;
using System.IO;

namespace IS4.MultiArchiver.Media
{
    /// <summary>
    /// Represents a Cabinet (CAB) archive for implementations compatible with
    /// the WinAPI functions for reading Cabinet files. The instance
    /// acts as a reader.
    /// </summary>
    public interface ICabinetArchive
    {
        /// <summary>
        /// Returns the next file in the archive.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ICabinetArchiveFile"/> identifying the next file,
        /// or null if the archive is at the end.
        /// </returns>
        ICabinetArchiveFile GetNextFile();
    }

    /// <summary>
    /// Stores the properties of a file in a Cabinet archive.
    /// </summary>
    public interface ICabinetArchiveFile
    {
        /// <summary>
        /// The name/path of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The stream that can be used to read the file.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// The modification date and time of the file.
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// The size of the file.
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Additional attributes of the file.
        /// </summary>
        FileAttributes Attributes { get; }
    }
}
