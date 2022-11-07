namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF individuals, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Individuals
    {
        /// <summary>
        /// <see cref="Dsm"/>:md5.
        /// </summary>
        [Uri(Dsm, "md5")]
        public static readonly IndividualUri MD5;

        /// <summary>
        /// <see cref="Ds"/>:sha1.
        /// </summary>
        [Uri(Ds, "sha1")]
        public static readonly IndividualUri SHA1;

        /// <summary>
        /// <see cref="Dsm"/>:sha224.
        /// </summary>
        [Uri(Dsm, "sha224")]
        public static readonly IndividualUri SHA224;

        /// <summary>
        /// <see cref="Enc"/>:sha256.
        /// </summary>
        [Uri(Enc, "sha256")]
        public static readonly IndividualUri SHA256;

        /// <summary>
        /// <see cref="Dsm"/>:sha384.
        /// </summary>
        [Uri(Dsm, "sha384")]
        public static readonly IndividualUri SHA384;

        /// <summary>
        /// <see cref="Ds"/>:sha512.
        /// </summary>
        [Uri(Ds, "sha512")]
        public static readonly IndividualUri SHA512;

        /// <summary>
        /// <see cref="Enc"/>:ripemd160.
        /// </summary>
        [Uri(Enc, "ripemd160")]
        public static readonly IndividualUri RIPEMD160;

        /// <summary>
        /// <see cref="Dsm2"/>:sha3-224.
        /// </summary>
        [Uri(Dsm2, "sha3-224")]
        public static readonly IndividualUri SHA3_224;

        /// <summary>
        /// <see cref="Dsm2"/>:sha3-256.
        /// </summary>
        [Uri(Dsm2, "sha3-256")]
        public static readonly IndividualUri SHA3_256;

        /// <summary>
        /// <see cref="Dsm2"/>:sha3-384.
        /// </summary>
        [Uri(Dsm2, "sha3-384")]
        public static readonly IndividualUri SHA3_384;

        /// <summary>
        /// <see cref="Dsm2"/>:sha3-512.
        /// </summary>
        [Uri(Dsm2, "sha3-512")]
        public static readonly IndividualUri SHA3_512;

        /// <summary>
        /// <see cref="Dsm2"/>:whirlpool.
        /// </summary>
        [Uri(Dsm2)]
        public static readonly IndividualUri Whirlpool;

        /// <summary>
        /// <see cref="At"/>:btih.
        /// </summary>
        [Uri(At, "btih")]
        public static readonly IndividualUri BTIH;

        /// <summary>
        /// <see cref="At"/>:bsha1-256.
        /// </summary>
        [Uri(At, "bsha1-256")]
        public static readonly IndividualUri BSHA1_256;

        /// <summary>
        /// <see cref="At"/>:bsha1-512.
        /// </summary>
        [Uri(At, "bsha1-512")]
        public static readonly IndividualUri BSHA1_512;

        /// <summary>
        /// <see cref="At"/>:bsha1-1024.
        /// </summary>
        [Uri(At, "bsha1-1024")]
        public static readonly IndividualUri BSHA1_1024;

        /// <summary>
        /// <see cref="At"/>:dhash.
        /// </summary>
        [Uri(At, "dhash")]
        public static readonly IndividualUri DHash;

        /// <summary>
        /// <see cref="At"/>:blake3-256.
        /// </summary>
        [Uri(At, "blake3-256")]
        public static readonly IndividualUri Blake3;

        /// <summary>
        /// <see cref="At"/>:crc32.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri Crc32;

        /// <summary>
        /// <see cref="Nfo"/>:losslessCompressionType.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri LosslessCompressionType;

        /// <summary>
        /// <see cref="Nfo"/>:lossyCompressionType.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri LossyCompressionType;

        /// <summary>
        /// <see cref="Nfo"/>:decryptedStatus.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri DecryptedStatus;

        /// <summary>
        /// <see cref="Nfo"/>:encryptedStatus.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri EncryptedStatus;

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
