using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public static class BitTorrentHashCache
    {
        static IDataHashAlgorithm HashAlgorithm => BitTorrentHash.HashAlgorithm;

        static readonly ConcurrentDictionary<int, PersistenceStore<IPersistentKey, Task<FileInfo>>> cache = new ConcurrentDictionary<int, PersistenceStore<IPersistentKey, Task<FileInfo>>>();

        static PersistenceStore<IPersistentKey, Task<FileInfo>> GetCache(int blockSize)
        {
            return cache.GetOrAdd(blockSize, l => new PersistenceStore<IPersistentKey, Task<FileInfo>>(f => FileInfo.Create(l, f)));
        }

        public static Task<FileInfo> GetCachedInfo(int blockSize, IFileNodeInfo file)
        {
            return GetCache(blockSize)[file];
        }

        public static Task<FileInfo> GetCachedInfo(int blockSize, Stream stream, IPersistentKey key)
        {
            async Task<FileInfo> Inner()
            {
                return new FileInfo(blockSize, await HashData(blockSize, stream));
            }
            var task = Inner();
            if(key != null)
            {
                GetCache(blockSize)[key] = task;
            }
            return task;
        }

        static async Task<(List<byte[]> list, int padding, long length)> HashData(int blockSize, Stream stream)
        {
            var hashAlgorithm = HashAlgorithm;
            var buffer = new byte[blockSize];
            var list = new List<byte[]>();
            int read;
            int pos = 0;
            int padding = 0;
            long length = 0;
            while((read = await stream.ReadAsync(buffer, pos, buffer.Length - pos)) > 0)
            {
                pos += read;
                length += read;
                if(pos == buffer.Length)
                {
                    list.Add(await hashAlgorithm.ComputeHash(buffer, 0, pos));
                    pos = 0;
                }
            }
            if(pos > 0)
            {
                Array.Clear(buffer, pos, buffer.Length - pos);
                list.Add(await hashAlgorithm.ComputeHash(buffer));
                list.Add(await hashAlgorithm.ComputeHash(buffer, 0, pos));
                padding = buffer.Length - pos;
            }else{
                list.Add(null);
                padding = 0;
            }
            return (list, padding, length);
        }

        public class FileInfo
        {
            public IReadOnlyList<byte[]> BlockHashes { get; }
            public byte[] LastHash { get; }
            public byte[] LastHashPadded { get; }
            public int Padding { get; }
            public long Length { get; }

            public FileInfo(int blockSize, (List<byte[]> list, int padding, long length) hashData)
            {
                var (list, padding, length) = hashData;
                Padding = padding;
                Length = length;

                if(list.Count > 0)
                {
                    LastHash = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    if(LastHash != null)
                    {
                        LastHashPadded = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);
                    }else{
                        LastHashPadded = LastHash = Array.Empty<byte>();
                    }
                }
                BlockHashes = list;
            }

            public static async Task<FileInfo> Create(int blockSize, IPersistentKey key)
            {
                if(!(key is IStreamFactory file)) throw new ArgumentException(null, nameof(key));
                using(var stream = file.Open())
                {
                    return new FileInfo(blockSize, await HashData(blockSize, stream));
                }
            }
        }
    }
}
