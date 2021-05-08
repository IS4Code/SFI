using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace IS4.MultiArchiver
{
    public static class BitTorrentHashCache
    {
        static IDataHashAlgorithm HashAlgorithm => BitTorrentHash.HashAlgorithm;

        static readonly ConcurrentDictionary<int, PersistenceStore<IFileNodeInfo, FileInfo>> cache = new ConcurrentDictionary<int, PersistenceStore<IFileNodeInfo, FileInfo>>();

        public static FileInfo GetCachedInfo(int blockSize, IFileNodeInfo file)
        {
            return cache.GetOrAdd(blockSize, l => new PersistenceStore<IFileNodeInfo, FileInfo>(f => new FileInfo(l, f)))[file];
        }

        public static List<byte[]> HashData(int blockSize, Stream stream, out int padding, out long length)
        {
            var hashAlgorithm = HashAlgorithm;
            var buffer = new byte[blockSize];
            var list = new List<byte[]>();
            int read;
            int pos = 0;
            length = 0;
            while((read = stream.Read(buffer, pos, buffer.Length - pos)) > 0)
            {
                pos += read;
                length += read;
                if(pos == buffer.Length)
                {
                    list.Add(hashAlgorithm.ComputeHash(buffer, 0, pos));
                    pos = 0;
                }
            }
            if(pos > 0)
            {
                Array.Clear(buffer, pos, buffer.Length - pos);
                list.Add(hashAlgorithm.ComputeHash(buffer));
                list.Add(hashAlgorithm.ComputeHash(buffer, 0, pos));
                padding = buffer.Length - pos;
            }else{
                list.Add(null);
                padding = 0;
            }
            return list;
        }

        public class FileInfo
        {
            public IReadOnlyList<byte[]> BlockHashes { get; }
            public byte[] LastHash { get; }
            public byte[] LastHashPadded { get; }
            public int Padding { get; }
            public long Length { get; }

            public FileInfo(int blockSize, IFileNodeInfo fileInfo)
            {
                if(!(fileInfo is IStreamFactory file)) throw new ArgumentException(null, nameof(fileInfo));
                List<byte[]> list;
                using(var stream = file.Open())
                {
                    list = HashData(blockSize, stream, out var padding, out var length);
                    Padding = padding;
                    Length = length;
                }
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
        }
    }
}
