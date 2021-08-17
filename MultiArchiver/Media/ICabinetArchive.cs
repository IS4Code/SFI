using System;
using System.IO;

namespace IS4.MultiArchiver.Media
{
    public interface ICabinetArchive
    {
        ICabinetArchiveFile GetNextFile();
    }

    public interface ICabinetArchiveFile
    {
        string Name { get; }
        Stream Stream { get; }
        DateTime Date { get; }
        uint Size { get; }
        FileAttributes Attributes { get; }
    }
}
