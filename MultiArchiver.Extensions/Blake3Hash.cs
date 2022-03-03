using Blake3;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    public class Blake3Hash : StreamDataHash<Hasher>
    {
        public static readonly Blake3Hash Instance = new Blake3Hash();

        public override int? NumericIdentifier => 0x1e;

        public Blake3Hash() : base(Individuals.Blake3, 32, "urn:blake3:", FormattingMethod.Base32)
        {

        }

        protected override Hasher Initialize()
        {
            return Hasher.New();
        }

        protected override void Append(ref Hasher instance, ArraySegment<byte> segment)
        {
            instance.Update(segment.AsSpan());
        }

        protected override byte[] Output(Hasher instance)
        {
            var hash = instance.Finalize();
            return hash.AsSpan().ToArray();
        }

        protected override void Finalize(Hasher instance)
        {
            instance.Dispose();
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data));
            return new ValueTask<byte[]>(hash.AsSpan().ToArray());
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data, offset, count));
            return new ValueTask<byte[]>(hash.AsSpan().ToArray());
        }
    }
}
