using BencodeNET.Objects;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver
{
    public class PaddedBlockHash : DataHashAlgorithm
    {
        public int BlockSize { get; }

        public PaddedBlockHash(Individuals identifier, string prefix, int blockSize) : base(identifier, BitTorrentHash.HashAlgorithm.HashSize, prefix, FormattingMethod.Base64)
        {
            BlockSize = blockSize;
        }

        public override byte[] ComputeHash(Stream input)
        {
            using(var output = new MemoryStream())
            {
                //using(var gzip = new GZipStream(output, CompressionLevel.Optimal, false))
                {
                    var gzip = output;
                    var list = BitTorrentHashCache.HashData(BlockSize, input, out var padding);
                    var enc = BitConverter.GetBytes(padding);
                    gzip.Write(enc, 0, enc.Length);
                    foreach(var block in list)
                    {
                        if(block != null) gzip.Write(block, 0, block.Length);
                    }
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
