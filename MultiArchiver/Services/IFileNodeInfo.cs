using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    public interface IFileNodeInfo : IPersistentKey
    {
        ILinkedNode Container { get; }
        string Name { get; }
        string Path { get; }
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }

        IFileNodeInfo WithContainer(ILinkedNode container);
    }

    public interface IFileInfo : IFileNodeInfo, IStreamFactory
    {
        long Length { get; }
    }

    public interface IDirectoryInfo : IFileNodeInfo
    {
        IEnumerable<IFileNodeInfo> Entries { get; }
    }
}
