using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class PaddedBlockHash : DataHashAlgorithm
    {
        public int BlockSize { get; }

        public PaddedBlockHash(IndividualUri identifier, string prefix, int blockSize) : base(identifier, sizeof(int) + 2 * BitTorrentHash.HashAlgorithm.HashSize, prefix, FormattingMethod.Base64)
        {
            BlockSize = blockSize;
        }

        public override async ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey key = null)
        {
            using(var output = new MemoryStream())
            {
                var info = await BitTorrentHashCache.GetCachedInfo(BlockSize, input, key);
                var enc = BitConverter.GetBytes(info.Padding);
                output.Write(enc, 0, enc.Length);
                foreach(var block in info.BlockHashes)
                {
                    output.Write(block, 0, block.Length);
                }
                output.Write(info.LastHashPadded, 0, info.LastHashPadded.Length);
                output.Write(info.LastHash, 0, info.LastHash.Length);
                return output.ToArray();
            }
        }

        public override async ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key = null)
        {
            using(var stream = new MemoryStream(data, false))
            {
                return await ComputeHash(stream, key);
            }
        }

        public override async ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null)
        {
            using(var stream = new MemoryStream(data, offset, count, false))
            {
                return await ComputeHash(stream, key);
            }
        }
    }
}
