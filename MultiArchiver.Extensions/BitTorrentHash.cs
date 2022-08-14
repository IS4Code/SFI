using BencodeNET.Objects;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// The hash algorithm that produces urn:btih: hashes.
    /// </summary>
    public class BitTorrentHash : FileHashAlgorithm, IHasFileOutput
    {
        public static readonly IDataHashAlgorithm HashAlgorithm = BuiltInHash.SHA1;

        public int BlockSize { get; set; } = 262144;

        public event OutputFileDelegate OutputFile;

        /// <summary>
        /// Creates a new instance of the algorithm.
        /// </summary>
        public BitTorrentHash() : base(Individuals.BTIH, HashAlgorithm.GetHashSize(0), "urn:btih:", FormattingMethod.Hex)
        {

        }

        public override int GetHashSize(long fileSize)
        {
            return HashAlgorithm.GetHashSize(fileSize);
        }

        public override async ValueTask<byte[]> ComputeHash(IFileInfo file)
        {
            var dict = await CreateDictionary(file, true);
            var hash = await HashAlgorithm.ComputeHash(dict.EncodeAsBytes());
            OnOutputFile(dict, hash);
            return hash;
        }

        public override async ValueTask<byte[]> ComputeHash(IDirectoryInfo directory, bool content)
        {
            var dict = await CreateDictionary(directory, content);
            var hash = await HashAlgorithm.ComputeHash(dict.EncodeAsBytes());
            OnOutputFile(dict, hash);
            return hash;
        }

        protected virtual void OnOutputFile(BDictionary info, byte[] hash)
        {
            var name = $"{BitConverter.ToString(hash).Replace("-", "")}.torrent";
            OutputFile?.Invoke(name, true, null, async stream => {
                var dict = new BDictionary();
                dict["info"] = info;
                await dict.EncodeToAsync(stream);
            });
        }

        private async Task<BDictionary> CreateDictionary(IFileNodeInfo fileNode, bool content)
        {
            var dict = new BDictionary();
            byte[] buffer;
            dict["piece length"] = new BNumber(BlockSize);
            dict["name"] = new BString(content ? fileNode.Name : "");
            switch(fileNode)
            {
                case IFileInfo file:
                    var info = await BitTorrentHashCache.GetCachedInfo(BlockSize, file);
                    dict["length"] = new BNumber(file.Length);
                    buffer = new byte[info.BlockHashes.Count * HashAlgorithm.GetHashSize(file.Length) + info.LastHash.Length];
                    for(int i = 0; i < info.BlockHashes.Count; i++)
                    {
                        info.BlockHashes[i].CopyTo(buffer, i * HashAlgorithm.GetHashSize(file.Length));
                    }
                    info.LastHash.CopyTo(buffer, buffer.Length - info.LastHash.Length);
                    break;
                case IDirectoryInfo directory:
                    var files = new List<(BitTorrentHashCache.FileInfo cached, BList<BString> path)>();
                    long bufferLength = 0;
                    var root = new BList<BString>();
                    if(!content)
                    {
                        root.Add(fileNode.Name);
                    }
                    foreach(var (file, path) in FindFiles(directory, root))
                    {
                        var cachedInfo = await BitTorrentHashCache.GetCachedInfo(BlockSize, file);
                        files.Add((cachedInfo, path));
                        bufferLength += cachedInfo.BlockHashes.Count * HashAlgorithm.GetHashSize((file as IFileInfo)?.Length ?? 0) + cachedInfo.LastHashPadded.Length;
                    }
                    buffer = new byte[bufferLength];
                    int pos = 0;
                    var filesList = new BList<BDictionary>();
                    foreach(var (cachedInfo, path) in files)
                    {
                        var fileDict = new BDictionary();
                        fileDict["path"] = path;
                        fileDict["length"] = new BNumber(cachedInfo.Length);
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

        private IEnumerable<(IFileNodeInfo info, BList<BString> path)> FindFiles(IDirectoryInfo dir, BList<BString> current)
        {
            foreach(var entry in dir.Entries.OrderBy(e => e.Name, StringComparer.Ordinal))
            {
                var path = new BList<BString>(current);
                path.Add(entry.Name);
                switch(entry)
                {
                    case IDirectoryInfo directory:
                        foreach(var inner in FindFiles(directory, path))
                        {
                            yield return inner;
                        }
                        break;
                    default:
                        yield return (entry, path);
                        break;
                }
            }
        }
    }
}
