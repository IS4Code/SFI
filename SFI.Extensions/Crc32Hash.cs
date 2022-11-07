using Force.Crc32;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Threading.Tasks;

namespace IS4.SFI
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

        /// <inheritdoc/>
        protected override uint Initialize()
        {
            return 0;
        }

        /// <inheritdoc/>
        protected override void Append(ref uint instance, ArraySegment<byte> segment)
        {
            instance = Crc32Algorithm.Append(instance, segment.Array, segment.Offset, segment.Count);
        }

        /// <inheritdoc/>
        protected override byte[] Output(ref uint instance)
        {
            return BitConverter.GetBytes(instance);
        }

        /// <inheritdoc/>
        protected override void Finalize(ref uint instance)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey? key = null)
        {
            return BitConverter.GetBytes(Crc32Algorithm.Compute(data));
        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey? key = null)
        {
            return BitConverter.GetBytes(Crc32Algorithm.Compute(data, offset, count));
        }
    }
}
