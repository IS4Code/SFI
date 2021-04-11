using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace IS4.MultiArchiver
{
    public static class BitTorrentHashCache
    {
        static IDataHashAlgorithm HashAlgorithm => BitTorrentHash.HashAlgorithm;

        static readonly ConcurrentDictionary<long, PersistenceStore<IFileInfo, FileInfo>> cache = new ConcurrentDictionary<long, PersistenceStore<IFileInfo, FileInfo>>();

        public static FileInfo GetCachedInfo(long blockSize, IFileInfo file)
        {
            return cache.GetOrAdd(blockSize, l => new PersistenceStore<IFileInfo, FileInfo>(f => new FileInfo(l, f)))[file];
        }

        public class FileInfo
        {
            public IReadOnlyList<byte[]> BlockHashes { get; }
            public byte[] LastHash { get; }
            public byte[] LastHashPadded { get; }
            public int Padding { get; set; }

            public FileInfo(long blockSize, IFileInfo file)
            {
                var list = new List<byte[]>();
                list.AddRange(HashFile(blockSize, file));
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

            private IEnumerable<byte[]> HashFile(long blockSize, IFileInfo file)
            {
                var hashAlgorithm = HashAlgorithm;
                var buffer = new byte[blockSize];
                using(var stream = file.Open())
                {
                    int read;
                    int pos = 0;
                    while((read = stream.Read(buffer, pos, buffer.Length - pos)) > 0)
                    {
                        pos += read;
                        if(pos == buffer.Length)
                        {
                            yield return hashAlgorithm.ComputeHash(buffer, 0, pos);
                            pos = 0;
                        }
                    }
                    if(pos > 0)
                    {
                        Array.Clear(buffer, pos, buffer.Length - pos);
                        yield return hashAlgorithm.ComputeHash(buffer);
                        yield return hashAlgorithm.ComputeHash(buffer, 0, pos);
                        Padding = buffer.Length - pos;
                    }else{
                        yield return null;
                    }
                }
            }
        }
    }
}
