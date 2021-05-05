using BencodeNET.Objects;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace IS4.MultiArchiver
{
    public class BitTorrentHash : FileHashAlgorithm
    {
        public static readonly IDataHashAlgorithm HashAlgorithm = BuiltInHash.SHA1;

        public int BlockSize { get; set; } = 262144;

        public delegate void InfoCreatedDelegate(BDictionary info, byte[] hash);

        public event InfoCreatedDelegate InfoCreated;

        public BitTorrentHash() : base(Individuals.BTIH, HashAlgorithm.HashSize, "urn:btih:", FormattingMethod.Hex)
        {

        }

        public override byte[] ComputeHash(IFileInfo file)
        {
            var dict = CreateDictionary(file, true);
            var hash = HashAlgorithm.ComputeHash(dict.EncodeAsBytes());
            InfoCreated?.Invoke(dict, hash);
            return hash;
        }

        public override byte[] ComputeHash(IDirectoryInfo directory, bool content)
        {
            var dict = CreateDictionary(directory, content);
            var hash = HashAlgorithm.ComputeHash(dict.EncodeAsBytes());
            InfoCreated?.Invoke(dict, hash);
            return hash;
        }

        private BDictionary CreateDictionary(IFileNodeInfo fileNode, bool content)
        {
            var dict = new BDictionary();
            byte[] buffer;
            dict["piece length"] = new BNumber(BlockSize);
            dict["name"] = new BString(content ? fileNode.Name : "");
            switch(fileNode)
            {
                case IFileInfo file:
                    var info = BitTorrentHashCache.GetCachedInfo(BlockSize, file);
                    dict["length"] = new BNumber(file.Length);
                    buffer = new byte[info.BlockHashes.Count * HashAlgorithm.HashSize + info.LastHash.Length];
                    for(int i = 0; i < info.BlockHashes.Count; i++)
                    {
                        info.BlockHashes[i].CopyTo(buffer, i * HashAlgorithm.HashSize);
                    }
                    info.LastHash.CopyTo(buffer, buffer.Length - info.LastHash.Length);
                    break;
                case IDirectoryInfo directory:
                    var files = new List<(IFileInfo info, BitTorrentHashCache.FileInfo cached, BList<BString> path)>();
                    long bufferLength = 0;
                    var root = new BList<BString>();
                    if(!content)
                    {
                        root.Add(fileNode.Name);
                    }
                    foreach(var (file, path) in FindFiles(directory, root))
                    {
                        var cachedInfo = BitTorrentHashCache.GetCachedInfo(BlockSize, file);
                        files.Add((file, cachedInfo, path));
                        bufferLength += cachedInfo.BlockHashes.Count * HashAlgorithm.HashSize + cachedInfo.LastHashPadded.Length;
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
            foreach(var entry in dir.Entries.OrderBy(e => e.Name, StringComparer.Ordinal))
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
    }
}
