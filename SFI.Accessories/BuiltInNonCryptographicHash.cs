using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Represents a non-cryptographic hash algorithm backed using a <see cref="NonCryptographicHashAlgorithm"/>
    /// instance.
    /// </summary>
    public class BuiltInNonCryptographicHash : StreamDataHash<NonCryptographicHashAlgorithm>
    {
        /// <summary>
        /// The CRC-32 hash algorithm, using <see cref="Crc32"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash CRC32 = Create(() => new Crc32(), Crc32.Hash, Individuals.Crc32, "urn:crc32:", 0x0132, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The CRC-64 hash algorithm, using <see cref="Crc64"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash CRC64 = Create(() => new Crc64(), Crc64.Hash, Individuals.Crc64, "urn:crc64:", 0x0164, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The XxHash32 hash algorithm, using <see cref="XxHash32"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash XXH32 = Create(() => new XxHash32(), span => XxHash32.Hash(span), Individuals.XxHash32, "urn:xxh32:", 0xb3e1, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The XxHash64 hash algorithm, using <see cref="XxHash64"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash XXH64 = Create(() => new XxHash64(), span => XxHash64.Hash(span), Individuals.XxHash64, "urn:xxh64:", 0xb3e2, formattingMethod: FormattingMethod.Hex);
        
        readonly Func<NonCryptographicHashAlgorithm> factory;

        readonly Hasher hasher;

        readonly ThreadLocal<NonCryptographicHashAlgorithm> algorithm;

        /// <summary>
        /// Represents a function that returns a hash from a byte span.
        /// </summary>
        /// <param name="source">The span to compute the hash from.</param>
        /// <returns>A new array containing the hash.</returns>
        public delegate byte[] Hasher(ReadOnlySpan<byte> source);

        /// <summary>
        /// Provides the backing instance of <see cref="NonCryptographicHashAlgorithm"/>
        /// for the current thread.
        /// </summary>
        public NonCryptographicHashAlgorithm Algorithm => algorithm.Value;

        /// <summary>
        /// Stores the base type of <see cref="Algorithm"/>,
        /// inheriting from <see cref="NonCryptographicHashAlgorithm"/>.
        /// </summary>
        public virtual Type AlgorithmType => typeof(NonCryptographicHashAlgorithm);

        /// <inheritdoc/>
        public override int? NumericIdentifier { get; }

        /// <inheritdoc/>
        public override string? NiName { get; }

        /// <summary>
        /// Creates a new instance of the hash algorithm from a factory function.
        /// </summary>
        /// <param name="factory">Function to create instances of <see cref="NonCryptographicHashAlgorithm"/> when needed.</param>
        /// <param name="hasher">Function to compute the hash from byte array.</param>
        /// <param name="identifier">The individual identifier of the algorithm.</param>
        /// <param name="prefix">The URI prefix used when creating URIs of hashes.</param>
        /// <param name="numericIdentifier">The value of <see cref="NumericIdentifier"/>.</param>
        /// <param name="niName">The value of <see cref="NiName"/>.</param>
        /// <param name="formattingMethod">The formatting method for creating URIs.</param>
        public BuiltInNonCryptographicHash(Func<NonCryptographicHashAlgorithm> factory, Hasher hasher, IndividualUri identifier, string prefix, int? numericIdentifier = null, string? niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32) : base(identifier, GetHashSize(factory), prefix, formattingMethod)
        {
            this.factory = factory;
            this.hasher = hasher;
            algorithm = new(factory);
            NumericIdentifier = numericIdentifier;
            NiName = niName;
        }

        private static BuiltInNonCryptographicHash Create<THash>(Func<THash> factory, Hasher hasher, IndividualUri identifier, string prefix, int? numericIdentifier = null, string? niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32) where THash : NonCryptographicHashAlgorithm
        {
            return new BuiltInNonCryptographicHashAlgorithm<THash>(factory, hasher, identifier, prefix, numericIdentifier, niName, formattingMethod);
        }

        private static int GetHashSize(Func<NonCryptographicHashAlgorithm> factory)
        {
            return factory().HashLengthInBytes;
        }

        /// <inheritdoc/>
        protected override NonCryptographicHashAlgorithm Initialize()
        {
            return factory();
        }

        /// <inheritdoc/>
        protected override void Finalize(ref NonCryptographicHashAlgorithm instance)
        {
            instance.Reset();
        }

        /// <inheritdoc/>
        protected override void Append(ref NonCryptographicHashAlgorithm instance, ArraySegment<byte> segment)
        {
            instance.Append(segment.AsSpan());
        }

        /// <inheritdoc/>
        protected override byte[] Output(ref NonCryptographicHashAlgorithm instance)
        {
            return instance.GetCurrentHash();
        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey? key = null)
        {
            return hasher(data.AsSpan());
        }

        /// <inheritdoc/>
        public async override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey? key = null)
        {
            return hasher(new ReadOnlySpan<byte>(data, offset, count));
        }
    }

    /// <summary>
    /// Represents a non-cryptographic hash algorithm backed using a <typeparamref name="THash"/>
    /// instance, inheriting from <see cref="NonCryptographicHashAlgorithm"/>.
    /// </summary>
    /// <typeparam name="THash">
    /// The base type of the inner hash algorithm, also exposed as <see cref="AlgorithmType"/>.
    /// </typeparam>
    public class BuiltInNonCryptographicHashAlgorithm<THash> : BuiltInNonCryptographicHash where THash : NonCryptographicHashAlgorithm
    {
        /// <inheritdoc/>
        public override Type AlgorithmType { get; } = typeof(THash);

        /// <inheritdoc cref="BuiltInNonCryptographicHash.Algorithm"/>
        public new THash Algorithm => (THash)base.Algorithm;

        /// <inheritdoc cref="BuiltInNonCryptographicHash.BuiltInNonCryptographicHash(Func{NonCryptographicHashAlgorithm}, Hasher, IndividualUri, string, int?, string?, FormattingMethod)"/>
        public BuiltInNonCryptographicHashAlgorithm(Func<NonCryptographicHashAlgorithm> factory, Hasher hasher, IndividualUri identifier, string prefix, int? numericIdentifier = null, string? niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32) : base(factory, hasher, identifier, prefix, numericIdentifier, niName, formattingMethod)
        {

        }
    }
}
