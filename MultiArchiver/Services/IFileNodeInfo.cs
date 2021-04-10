using System;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileNodeInfo
    {
        ILinkedNode Container { get; }
        string Name { get; }
        string Path { get; }
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }

        IFileNodeInfo WithContainer(ILinkedNode container);
    }

    public interface IFileInfo : IFileNodeInfo
    {
        long? Length { get; }
        Stream Open();
    }

    public interface IDirectoryInfo : IFileNodeInfo
    {
        IEnumerable<IFileNodeInfo> Entries { get; }
    }
}
