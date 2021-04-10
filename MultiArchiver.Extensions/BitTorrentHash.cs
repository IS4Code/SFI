using BencodeNET.Objects;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace IS4.MultiArchiver
{
    class BitTorrentHash : FileHashAlgorithm
    {
        static readonly ThreadLocal<SHA1> sha1 = new ThreadLocal<SHA1>(SHA1.Create);
        readonly PersistenceStore<IFileInfo, FileInfo> cache;
        readonly byte[] paddingHash;

        public long BlockSize { get; }

        public BitTorrentHash(long blockSize = 262144) : base(Individuals.BTIH, "urn:btih:", FormattingMethod.Hex)
        {
            BlockSize = blockSize;
            cache = new PersistenceStore<IFileInfo, FileInfo>(GetInfo);
        }

        public override byte[] ComputeHash(IFileNodeInfo fileNode)
        {
            var dict = GetDictionary(fileNode);
            return sha1.Value.ComputeHash(dict.EncodeAsBytes());
        }

        private BDictionary GetDictionary(IFileNodeInfo fileNode)
        {
            var dict = new BDictionary();
            byte[] buffer;
            dict["piece length"] = new BNumber(BlockSize);
            dict["name"] = new BString(fileNode.Name);
            switch(fileNode)
            {
                case IFileInfo file:
                    var info = cache[file];
                    dict["length"] = new BNumber(file.Length);
                    buffer = new byte[info.BlockHashes.Count * BlockSize + info.LastHash.Length];
                    for(int i = 0; i < info.BlockHashes.Count; i++)
                    {
                        info.BlockHashes[i].CopyTo(buffer, i * BlockSize);
                    }
                    info.LastHash.CopyTo(buffer, buffer.Length - info.LastHash.Length);
                    break;
                case IDirectoryInfo directory:
                    var files = new List<(IFileInfo info, FileInfo cached, BList<BString> path)>();
                    long bufferLength = 0;
                    foreach(var (file, path) in FindFiles(directory, new BList<BString>()))
                    {
                        var cachedInfo = cache[file];
                        files.Add((file, cachedInfo, path));
                        bufferLength += cachedInfo.BlockHashes.Count * BlockSize + cachedInfo.LastHashPadded.Length;
                    }
                    buffer = new byte[bufferLength];
                    int pos = 0;
                    var filesList = new BList<BDictionary>();
                    foreach(var (file, cachedInfo, path) in files)
                    {
                        var fileDict = new BDictionary();
                        fileDict["path"] = path;
                        fileDict["length"] = new BNumber(file.Length);
                        filesList.Add(fileDict);

                        foreach(var hash in cachedInfo.BlockHashes)
                        {
                            hash.CopyTo(buffer, pos);
                            pos += hash.Length;
                        }
                        cachedInfo.LastHashPadded.CopyTo(buffer, pos);
                        pos += cachedInfo.LastHashPadded.Length;

                        if(cachedInfo.Padding > 0)
                        {
                            filesList.Add(new BDictionary
                            {
                                { "path", new BList<BString>{ ".pad", cachedInfo.Padding.ToString() } },
                                { "length", new BNumber(cachedInfo.Padding) }
                            });
                        }
                    }
                    dict["files"] = filesList;
                    break;
                default:
                    throw new NotSupportedException();
            }
            dict["pieces"] = new BString(buffer);
            return dict;
        }

        private IEnumerable<(IFileInfo info, BList<BString> path)> FindFiles(IDirectoryInfo dir, BList<BString> current)
        {
            foreach(var entry in dir.Entries)
            {
                var path = new BList<BString>(current);
                path.Add(entry.Name);
                switch(entry)
                {
                    case IFileInfo file:
                        yield return (file, path);
                        break;
                    case IDirectoryInfo directory:
                        foreach(var inner in FindFiles(directory, path))
                        {
                            yield return inner;
                        }
                        break;
                }
            }
        }

        private FileInfo GetInfo(IFileInfo file)
        {
            return new FileInfo(this, file);
        }

        class FileInfo
        {
            public IReadOnlyList<byte[]> BlockHashes { get; }
            public byte[] LastHash { get; }
            public byte[] LastHashPadded { get; }
            public int Padding { get; set; }

            public FileInfo(BitTorrentHash hashInfo, IFileInfo file)
            {
                var list = new List<byte[]>();
                list.AddRange(HashFile(hashInfo, file));
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

            private IEnumerable<byte[]> HashFile(BitTorrentHash hashInfo, IFileInfo file)
            {
                var hashAlgorithm = sha1.Value;
                var buffer = new byte[hashInfo.BlockSize];
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
