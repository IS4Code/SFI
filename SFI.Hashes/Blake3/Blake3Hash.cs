using Blake3;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IS4.SFI.Hashes
{
    /// <summary>
    /// The BLAKE3 hash algorithm, using <see cref="Blake3.Hasher"/>.
    /// </summary>
    [Description("The BLAKE3 hash algorithm.")]
    public class Blake3Hash : StreamDataHash<Hasher>
    {
        /// <summary>
        /// <c><see cref="Vocabularies.Uri.At"/>:blake3-256</c>.
        /// </summary>
        [Uri(Vocabularies.Uri.At, "blake3-256")]
        public static readonly IndividualUri Blake3;

        /// <inheritdoc/>
        public override int? NumericIdentifier => 0x1e;

        /// <inheritdoc cref="StreamDataHash{T}.StreamDataHash(IndividualUri, int, string, FormattingMethod)"/>
        public Blake3Hash() : base(Blake3, 32, "urn:blake3:", FormattingMethod.Base32)
        {

        }

        /// <inheritdoc/>
        protected override Hasher Initialize()
        {
            return Hasher.New();
        }

        /// <inheritdoc/>
        protected override void Append(ref Hasher instance, ArraySegment<byte> segment)
        {
            instance.Update(segment.AsSpan());
        }

        /// <inheritdoc/>
        protected override byte[] Output(ref Hasher instance)
        {
            var hash = instance.Finalize();
            return hash.AsSpan().ToArray();
        }

        /// <inheritdoc/>
        protected override void Finalize(ref Hasher instance)
        {
            instance.Dispose();
        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, IIdentityKey? key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data));
            return hash.AsSpan().ToArray();
        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IIdentityKey? key = null)
        {
            var hash = Hasher.Hash(new ReadOnlySpan<byte>(data, offset, count));
            return hash.AsSpan().ToArray();
        }

        static Blake3Hash()
        {
            typeof(Blake3Hash).InitializeUris();
        }
    }
}
