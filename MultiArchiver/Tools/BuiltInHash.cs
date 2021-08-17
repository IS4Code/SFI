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
        public static readonly IDataHashAlgorithm MD5 = new BuiltInHash(Cryptography.MD5.Create, Individuals.MD5, 0xd5, "urn:md5:", FormattingMethod.Hex);
        public static readonly IDataHashAlgorithm SHA1 = new BuiltInHash(Cryptography.SHA1.Create, Individuals.SHA1, 0x11, "urn:sha1:");
        public static readonly IDataHashAlgorithm SHA256 = new BuiltInHash(Cryptography.SHA256.Create, Individuals.SHA256, 0x12, "urn:sha256:");
        public static readonly IDataHashAlgorithm SHA384 = new BuiltInHash(Cryptography.SHA384.Create, Individuals.SHA384, null, "urn:sha384:");
        public static readonly IDataHashAlgorithm SHA512 = new BuiltInHash(Cryptography.SHA512.Create, Individuals.SHA512, 0x13, "urn:sha512:");

        readonly ThreadLocal<Cryptography.HashAlgorithm> algorithm;

        public BuiltInHash(Func<Cryptography.HashAlgorithm> factory, IndividualUri identifier, int? numericIdentifier, string prefix, FormattingMethod formattingMethod = FormattingMethod.Base32) : base(identifier, numericIdentifier, GetHashSize(factory), prefix, formattingMethod)
        {
            algorithm = new ThreadLocal<Cryptography.HashAlgorithm>(factory);
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
            return algorithm.Value.ComputeHash(input);
        }

        public override byte[] ComputeHash(byte[] data, IPersistentKey key)
        {
            return algorithm.Value.ComputeHash(data);
        }

        public override byte[] ComputeHash(byte[] data, int offset, int count, IPersistentKey key)
        {
            return algorithm.Value.ComputeHash(data, offset, count);
        }
    }
}
