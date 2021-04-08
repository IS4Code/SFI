using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO;
using System.Threading;
using Cryptography = System.Security.Cryptography;

namespace IS4.MultiArchiver.Tools
{
    public class BuiltInHash : HashAlgorithm
    {
        public static readonly IHashAlgorithm MD5 = new BuiltInHash(Cryptography.MD5.Create, Individuals.MD5, "urn:md5:");
        public static readonly IHashAlgorithm SHA1 = new BuiltInHash(Cryptography.SHA1.Create, Individuals.SHA1, "urn:sha1:");
        public static readonly IHashAlgorithm SHA256 = new BuiltInHash(Cryptography.SHA256.Create, Individuals.SHA256, "urn:sha256:");
        public static readonly IHashAlgorithm SHA384 = new BuiltInHash(Cryptography.SHA384.Create, Individuals.SHA384, "urn:sha384:");
        public static readonly IHashAlgorithm SHA512 = new BuiltInHash(Cryptography.SHA512.Create, Individuals.SHA512, "urn:sha512:");

        readonly ThreadLocal<Cryptography.HashAlgorithm> algorithm;

        public BuiltInHash(Func<Cryptography.HashAlgorithm> factory, Individuals identifier, string prefix) : base(identifier, prefix, FormattingMethod.Hex)
        {
            algorithm = new ThreadLocal<Cryptography.HashAlgorithm>(factory);
        }

        public override byte[] ComputeHash(Stream input)
        {
            return algorithm.Value.ComputeHash(input);
        }
    }
}
