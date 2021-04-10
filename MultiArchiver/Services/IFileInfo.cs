using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileInfo
    {
        ILinkedNode Container { get; }
        string Name { get; }
        string Path { get; }
        long? Length { get; }
        Stream Open();
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }
    }
}
