using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IS4.MultiArchiver.Services
{
    public interface IFileNodeInfo : IPersistentKey
    {
        string Name { get; }
        string SubName { get; }
        string Path { get; }
        int? Revision { get; }
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }
    }

    public interface IFileInfo : IFileNodeInfo, IStreamFactory
    {
        bool IsEncrypted { get; }
    }

    public interface IDirectoryInfo : IFileNodeInfo
    {
        IEnumerable<IFileNodeInfo> Entries { get; }
    }

    public abstract class FileSystemInfoWrapper<TInfo> : IFileNodeInfo where TInfo : FileSystemInfo
    {
        protected TInfo BaseInfo { get; }
        readonly IPersistentKey key;

        public FileSystemInfoWrapper(TInfo baseInfo, IPersistentKey key = null)
        {
            this.BaseInfo = baseInfo;
            this.key = key;
        }

        public string Name => BaseInfo.Name;

        public string SubName => null;

        public string Path => BaseInfo.FullName.Substring(System.IO.Path.GetPathRoot(BaseInfo.FullName).Length).Replace(System.IO.Path.DirectorySeparatorChar, '/');

        public DateTime? CreationTime {
            get {
                try{
                    return BaseInfo.CreationTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public DateTime? LastWriteTime {
            get {
                try{
                    return BaseInfo.LastWriteTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public DateTime? LastAccessTime {
            get {
                try{
                    return BaseInfo.LastAccessTimeUtc;
                }catch(ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public int? Revision => null;

        public bool IsEncrypted => false;

        public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

        object IPersistentKey.ReferenceKey => key != null ? key.ReferenceKey : AppDomain.CurrentDomain;

        object IPersistentKey.DataKey => key != null ? key.DataKey : BaseInfo.FullName;
        
        public override string ToString()
        {
            return "/" + Path;
        }
    }

    public class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo
    {
        public FileInfoWrapper(FileInfo baseInfo, IPersistentKey key = null) : base(baseInfo, key)
        {

        }

        public long Length => BaseInfo.Length;

        public Stream Open()
        {
            return BaseInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
    }

    public class DirectoryInfoWrapper : FileSystemInfoWrapper<DirectoryInfo>, IDirectoryInfo
    {
        public DirectoryInfoWrapper(DirectoryInfo baseInfo, IPersistentKey key = null) : base(baseInfo, key)
        {

        }

        public IEnumerable<IFileNodeInfo> Entries =>
            BaseInfo.EnumerateFiles().Select(f => (IFileNodeInfo)new FileInfoWrapper(f)).Concat(
                BaseInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d))
                );
    }
}
