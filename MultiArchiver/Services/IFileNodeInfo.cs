﻿using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    public interface IFileNodeInfo : IPersistentKey
    {
        string Name { get; }
        string Path { get; }
        int? Revision { get; }
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }
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
