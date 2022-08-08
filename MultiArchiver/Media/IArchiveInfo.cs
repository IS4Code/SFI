using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Media
{
    public interface IArchiveInfo
    {
        bool IsComplete { get; }
        bool IsSolid { get; }
    }

    public interface IArchiveFile : IArchiveInfo
    {
        IEnumerable<IArchiveEntry> Entries { get; }
    }

    public interface IArchiveReader : IArchiveInfo, IEnumerator<IArchiveEntry>
    {
        void Skip();
    }

    public interface IArchiveEntry : IFileNodeInfo
    {
        DateTime? ArchivedTime { get; }
    }
}
