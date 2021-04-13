using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;

namespace IS4.MultiArchiver
{
    public class PaddedBlockHash : DataHashAlgorithm
    {
        public int BlockSize { get; }

        public PaddedBlockHash(Individuals identifier, string prefix, int blockSize) : base(identifier, sizeof(int) + 2 * BitTorrentHash.HashAlgorithm.HashSize, prefix, FormattingMethod.Base64)
        {
            BlockSize = blockSize;
        }

        public override byte[] ComputeHash(Stream input)
        {
            using(var output = new MemoryStream())
            {
                var list = BitTorrentHashCache.HashData(BlockSize, input, out var padding);
                var enc = BitConverter.GetBytes(padding);
                output.Write(enc, 0, enc.Length);
                foreach(var block in list)
                {
                    if(block != null) output.Write(block, 0, block.Length);
                }
                return output.ToArray();
            }
        }

        public override byte[] ComputeHash(byte[] data)
        {
            using(var stream = new MemoryStream(data, false))
            {
                return ComputeHash(stream);
            }
        }

        public override byte[] ComputeHash(byte[] data, int offset, int count)
        {
            using(var stream = new MemoryStream(data, offset, count, false))
            {
                return ComputeHash(stream);
            }
        }
    }
}
