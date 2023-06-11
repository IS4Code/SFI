using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Hashing;
using static IS4.SFI.Tools.BuiltInNonCryptographicHash;

namespace IS4.SFI.Hashes
{
    /// <summary>
    /// Provides known non-cryptographic hashes from <see cref="System.IO.Hashing"/>.
    /// </summary>
    public class NonCryptographicHashes : IEnumerable<BuiltInNonCryptographicHash>
    {
        /// <summary>
        /// The CRC-32 hash algorithm, using <see cref="Crc32"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash CRC32 = Create(() => new Crc32(), Crc32.Hash, NonCryptographicHashIndividuals.Crc32, "urn:crc32:", 0x0132, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The CRC-64 hash algorithm, using <see cref="Crc64"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash CRC64 = Create(() => new Crc64(), Crc64.Hash, NonCryptographicHashIndividuals.Crc64, "urn:crc64:", 0x0164, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The XxHash32 hash algorithm, using <see cref="XxHash32"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash XXH32 = Create(() => new XxHash32(), span => XxHash32.Hash(span), NonCryptographicHashIndividuals.XxHash32, "urn:xxh32:", 0xb3e1, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The XxHash64 hash algorithm, using <see cref="XxHash64"/>.
        /// </summary>
        public static readonly BuiltInNonCryptographicHash XXH64 = Create(() => new XxHash64(), span => XxHash64.Hash(span), NonCryptographicHashIndividuals.XxHash64, "urn:xxh64:", 0xb3e2, formattingMethod: FormattingMethod.Hex);

        private static BuiltInNonCryptographicHash Create<THash>(Func<THash> factory, Hasher hasher, IndividualUri identifier, string prefix, int? numericIdentifier = null, string? niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32) where THash : NonCryptographicHashAlgorithm
        {
            return new BuiltInNonCryptographicHashAlgorithm<THash>(factory, hasher, identifier, prefix, numericIdentifier, niName, formattingMethod);
        }

        readonly BuiltInNonCryptographicHash[] hashes = {
            CRC32,
            CRC64,
            XXH32,
            XXH64
        };

        /// <summary>
        /// Enumerates the additional hashes provided by this assembly.
        /// </summary>
        /// <returns>The instances of <see cref="BuiltInNonCryptographicHash"/>.</returns>
        public IEnumerator<BuiltInNonCryptographicHash> GetEnumerator()
        {
            return ((IEnumerable<BuiltInNonCryptographicHash>)hashes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
