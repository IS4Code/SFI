using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cryptography = System.Security.Cryptography;

namespace IS4.MultiArchiver.Tools
{
    /// <summary>
    /// Represents a hash algorithm backed using a native <see cref="Cryptography.HashAlgorithm"/>
    /// instance.
    /// </summary>
    public class BuiltInHash : DataHashAlgorithm
    {
        /// <summary>
        /// The MD5 hash algorithm, using <see cref="Cryptography.MD5"/>.
        /// </summary>
        public static readonly BuiltInHash MD5 = Create(Cryptography.MD5.Create, Individuals.MD5, "urn:md5:", 0xd5, formattingMethod: FormattingMethod.Hex);

        /// <summary>
        /// The SHA-1 hash algorithm, using <see cref="Cryptography.SHA1"/>.
        /// </summary>
        public static readonly BuiltInHash SHA1 = Create(Cryptography.SHA1.Create, Individuals.SHA1, "urn:sha1:", 0x11);

        /// <summary>
        /// The SHA-256 hash algorithm, using <see cref="Cryptography.SHA256"/>.
        /// </summary>
        public static readonly BuiltInHash SHA256 = Create(Cryptography.SHA256.Create, Individuals.SHA256, "urn:sha256:", 0x12, "sha-256");

        /// <summary>
        /// The SHA-384 hash algorithm, using <see cref="Cryptography.SHA384"/>.
        /// </summary>
        public static readonly BuiltInHash SHA384 = Create(Cryptography.SHA384.Create, Individuals.SHA384, "urn:sha384:", niName: "sha-384");

        /// <summary>
        /// The SHA-512 hash algorithm, using <see cref="Cryptography.SHA512"/>.
        /// </summary>
        public static readonly BuiltInHash SHA512 = Create(Cryptography.SHA512.Create, Individuals.SHA512, "urn:sha512:", 0x13, "sha-512");

        readonly ThreadLocal<Cryptography.HashAlgorithm> algorithm;

        /// <summary>
        /// Provides the backing instance of <see cref="Cryptography.HashAlgorithm"/>
        /// for the current thread.
        /// </summary>
        public Cryptography.HashAlgorithm Algorithm => algorithm.Value;

        public override int? NumericIdentifier { get; }

        public override string NiName { get; }

        /// <summary>
        /// Creates a new instance of the hash algorithm from a factory function.
        /// </summary>
        /// <param name="factory">Function to create instances of <see cref="Cryptography.HashAlgorithm"/> when needed.</param>
        /// <param name="identifier">The individual identifier of the algorithm.</param>
        /// <param name="prefix">The URI prefix used when creating URIs of hashes.</param>
        /// <param name="numericIdentifier">The value of <see cref="NumericIdentifier"/>.</param>
        /// <param name="niName">The value of <see cref="NiName"/>.</param>
        /// <param name="formatting">The formatting method for creating URIs.</param>
        /// <param name="hashSize">The usual size of the hash.</param>
        public BuiltInHash(Func<Cryptography.HashAlgorithm> factory, IndividualUri identifier, string prefix, int? numericIdentifier = null, string niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32) : base(identifier, GetHashSize(factory), prefix, formattingMethod)
        {
            algorithm = new ThreadLocal<Cryptography.HashAlgorithm>(factory);
            NumericIdentifier = numericIdentifier;
            NiName = niName;
        }

        private static BuiltInHash Create(Func<Cryptography.HashAlgorithm> factory, IndividualUri identifier, string prefix, int? numericIdentifier = null, string niName = null, FormattingMethod formattingMethod = FormattingMethod.Base32)
        {
            try{
                return new BuiltInHash(factory, identifier, prefix, numericIdentifier, niName, formattingMethod);
            }catch(PlatformNotSupportedException)
            {
                return null;
            }
        }

        private static int GetHashSize(Func<Cryptography.HashAlgorithm> factory)
        {
            using(var hash = factory())
            {
                return (hash.HashSize + 7) / 8;
            }
        }

        public override async ValueTask<byte[]> ComputeHash(Stream input, IPersistentKey key)
        {
            var algorithm = Algorithm;

            try{
                const int bufferSize = 4096;
                var buffer = new byte[bufferSize];
                int bytesRead;
                while((bytesRead = await input.ReadAsync(buffer, 0, bufferSize)) != 0)
                {
                    algorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }
                algorithm.TransformFinalBlock(buffer, 0, bytesRead);
                return (byte[])algorithm.Hash.Clone();
            }finally{
                algorithm.Initialize();
            }
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, IPersistentKey key)
        {
            return new ValueTask<byte[]>(Algorithm.ComputeHash(data));
        }

        public override ValueTask<byte[]> ComputeHash(byte[] data, int offset, int count, IPersistentKey key)
        {
            return new ValueTask<byte[]>(Algorithm.ComputeHash(data, offset, count));
        }
    }
}
