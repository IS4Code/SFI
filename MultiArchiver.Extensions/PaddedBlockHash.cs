using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Represents a chunked hash that stores a variable number of SHA-1 hashes
    /// to use to produce BitTorrent files. The hash has a specific structure,
    /// starting with 4 bytes storing the (int32) size of the padding
    /// (<see cref="BitTorrentHashCache.FileInfo.Padding"/>), followed by each
    /// block hash (<see cref="BitTorrentHashCache.FileInfo.BlockHashes"/>) and
    /// the hash of the last block, padded and unpadded
    /// (<see cref="BitTorrentHashCache.FileInfo.LastHashPadded"/> and
    /// <see cref="BitTorrentHashCache.FileInfo.LastHash"/>)
    /// </summary>
    public class PaddedBlockHash : DataHashAlgorithm
    {
        /// <summary>
        /// The size of the individually hashed blocks.
        /// </summary>
        public int BlockSize { get; }

        /// <param name="blockSize">The value of <see cref="BlockSize"/>.</param>
        /// <inheritdoc cref="DataHashAlgorithm.DataHashAlgorithm(IndividualUri, int, string, FormattingMethod)"/>
        public PaddedBlockHash(IndividualUri identifier, string prefix, int blockSize) : base(identifier, BitTorrentHash.HashAlgorithm.GetHashSize(blockSize), prefix, FormattingMethod.Base64)
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

        public override int GetHashSize(long fileSize)
        {
            return sizeof(int) + (int)(fileSize / BlockSize + 1) * HashSize;
        }
    }
}
