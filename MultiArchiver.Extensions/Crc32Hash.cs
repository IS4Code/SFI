using Force.Crc32;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// The CRC32 checksum algorithm, using <see cref="Crc32Algorithm"/>.
    /// </summary>
    public class Crc32Hash : StreamDataHash<uint>
    {
        /// <summary>
        /// The singleton instance of the algorithm.
        /// </summary>
        public static readonly Crc32Hash Instance = new Crc32Hash();

        private Crc32Hash() : base(Individuals.Crc32, 4, "urn:crc32:", FormattingMethod.Hex)
        {

        }

        protected override uint Initialize()
        {
            return 0;
        }

        protected override void Append(ref uint instance, ArraySegment<byte> segment)
        {
            instance = Crc32Algorithm.Append(instance, segment.Array, segment.Offset, segment.Count);
        }

        protected override byte[] Output(ref uint instance)
        {
            return BitConverter.GetBytes(instance);
        }

        protected override void Finalize(ref uint instance)
        {

        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key = null)
        {
            return new ValueTask<byte[]>(BitConverter.GetBytes(Crc32Algorithm.Compute(data)));
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null)
        {
            return new ValueTask<byte[]>(BitConverter.GetBytes(Crc32Algorithm.Compute(data, offset, count)));
        }
    }
}
