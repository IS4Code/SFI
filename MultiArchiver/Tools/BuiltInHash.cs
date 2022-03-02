using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading;
using Cryptography = System.Security.Cryptography;

namespace IS4.MultiArchiver.Tools
{
    public class BuiltInHash : DataHashAlgorithm
    {
        public static readonly BuiltInHash MD5 = Create(Cryptography.MD5.Create, Individuals.MD5, "urn:md5:", 0xd5, formattingMethod: FormattingMethod.Hex);
        public static readonly BuiltInHash SHA1 = Create(Cryptography.SHA1.Create, Individuals.SHA1, "urn:sha1:", 0x11);
        public static readonly BuiltInHash SHA256 = Create(Cryptography.SHA256.Create, Individuals.SHA256, "urn:sha256:", 0x12, "sha-256");
        public static readonly BuiltInHash SHA384 = Create(Cryptography.SHA384.Create, Individuals.SHA384, "urn:sha384:", niName: "sha-384");
        public static readonly BuiltInHash SHA512 = Create(Cryptography.SHA512.Create, Individuals.SHA512, "urn:sha512:", 0x13, "sha-512");

        readonly ThreadLocal<Cryptography.HashAlgorithm> algorithm;

        public Cryptography.HashAlgorithm Algorithm => algorithm.Value;

        public override int? NumericIdentifier { get; }

        public override string NiName { get; }

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

        public override byte[] ComputeHash(Stream input, IPersistentKey key)
        {
            return Algorithm.ComputeHash(input);
        }

        public override byte[] ComputeHash(byte[] data, IPersistentKey key)
        {
            return Algorithm.ComputeHash(data);
        }

        public override byte[] ComputeHash(byte[] data, int offset, int count, IPersistentKey key)
        {
            return Algorithm.ComputeHash(data, offset, count);
        }
    }
}
