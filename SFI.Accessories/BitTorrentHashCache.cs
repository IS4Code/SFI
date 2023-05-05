using IS4.SFI.Services;
using IS4.SFI.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// The cache of file hashes shared by <see cref="BitTorrentHash"/> and
    /// <see cref="PaddedBlockHash"/>.
    /// </summary>
    public static class BitTorrentHashCache
    {
        static IDataHashAlgorithm HashAlgorithm => BitTorrentHash.HashAlgorithm;

        static readonly ConcurrentDictionary<int, IdentityStore<IIdentityKey, Task<FileInfo>>> cache = new();

        static IdentityStore<IIdentityKey, Task<FileInfo>> GetCache(int blockSize)
        {
            return cache.GetOrAdd(blockSize, l => new IdentityStore<IIdentityKey, Task<FileInfo>>(f => FileInfo.Create(l, f)));
        }

        /// <summary>
        /// Retrieves the cached info for a given file.
        /// </summary>
        /// <param name="blockSize">The block size of individually hashed sections.</param>
        /// <param name="file">The file to hash.</param>
        /// <returns>The information about the file.</returns>
        public static async ValueTask<FileInfo> GetCachedInfo(int blockSize, IFileNodeInfo file)
        {
            return await GetCache(blockSize)[file];
        }

        /// <summary>
        /// Retrieves the cached info for a given file.
        /// </summary>
        /// <param name="blockSize">The block size of individually hashed sections.</param>
        /// <param name="stream">To stream to hash.</param>
        /// <param name="key">The key in the cache.</param>
        /// <returns>The information about the file.</returns>
        public static Task<FileInfo> GetCachedInfo(int blockSize, Stream stream, IIdentityKey? key)
        {
            async Task<FileInfo> Inner()
            {
                return new FileInfo(await HashData(blockSize, stream));
            }
            var task = Inner();
            if(key != null)
            {
                GetCache(blockSize)[key] = task;
            }
            return task;
        }

        static async Task<(List<byte[]?> list, int padding, long length)> HashData(int blockSize, Stream stream)
        {
            var hashAlgorithm = HashAlgorithm;
            var buffer = new byte[blockSize];
            var list = new List<byte[]?>();
            int read;
            int pos = 0;
            int padding;
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

        /// <summary>
        /// The information about the hash of a file.
        /// </summary>
        public class FileInfo
        {
            /// <summary>
            /// The list of hashes for the consecutive whole blocks in the file.
            /// </summary>
            public IReadOnlyList<byte[]> BlockHashes { get; }

            /// <summary>
            /// The hash of the last block with its original size.
            /// </summary>
            public byte[] LastHash { get; } = Array.Empty<byte>();

            /// <summary>
            /// The hash of the last block padded with zeros.
            /// </summary>
            public byte[] LastHashPadded { get; } = Array.Empty<byte>();

            /// <summary>
            /// The length of the padding in bytes.
            /// </summary>
            public int Padding { get; }

            /// <summary>
            /// The length of the file.
            /// </summary>
            public long Length { get; }

            /// <summary>
            /// Creates a new instance of the record.
            /// </summary>
            /// <param name="hashData">The data of the hashed blocks.</param>
            public FileInfo((List<byte[]?> list, int padding, long length) hashData)
            {
                var (list, padding, length) = hashData;
                Padding = padding;
                Length = length;

                if(list.Count > 0)
                {
                    var lastHash = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    if(lastHash != null)
                    {
                        LastHash = lastHash;
                        LastHashPadded = list[list.Count - 1]!;
                        list.RemoveAt(list.Count - 1);
                    }
                }
                BlockHashes = list!;
            }

            /// <summary>
            /// Creates and retrieves hashed file information pertatining to <paramref name="key"/>.
            /// </summary>
            /// <param name="blockSize">The hashed block size to use.</param>
            /// <param name="key">The object to retrieve the information for. Must be an instance of <see cref="IStreamFactory"/>.</param>
            /// <returns>An instance of <see cref="FileInfo"/> storing the hash information.</returns>
            public static async Task<FileInfo> Create(int blockSize, IIdentityKey key)
            {
                if(key is not IStreamFactory file) throw new ArgumentException($"The object must derive from {typeof(IStreamFactory)}.", nameof(key));
                using var stream = file.Open();
                return new FileInfo(await HashData(blockSize, stream));
            }
        }
    }
}
