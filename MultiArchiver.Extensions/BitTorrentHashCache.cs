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

        static readonly ConcurrentDictionary<int, PersistenceStore<IFileInfo, FileInfo>> cache = new ConcurrentDictionary<int, PersistenceStore<IFileInfo, FileInfo>>();

        public static FileInfo GetCachedInfo(int blockSize, IFileInfo file)
        {
            return cache.GetOrAdd(blockSize, l => new PersistenceStore<IFileInfo, FileInfo>(f => new FileInfo(l, f)))[file];
        }

        public static List<byte[]> HashData(int blockSize, Stream stream, out int padding)
        {
            var hashAlgorithm = HashAlgorithm;
            var buffer = new byte[blockSize];
            var list = new List<byte[]>();
            int read;
            int pos = 0;
            while((read = stream.Read(buffer, pos, buffer.Length - pos)) > 0)
            {
                pos += read;
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

            public FileInfo(int blockSize, IFileInfo file)
            {
                List<byte[]> list;
                using(var stream = file.Open())
                {
                    list = HashData(blockSize, stream, out var padding);
                    Padding = padding;
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
