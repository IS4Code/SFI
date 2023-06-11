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
        /// <c><see cref="Rdf"/>:nil</c>.
        /// </summary>
        [Uri(Rdf)]
        public static readonly IndividualUri Nil;

        /// <summary>
        /// <c><see cref="Dsm"/>:md5</c>.
        /// </summary>
        [Uri(Dsm, "md5")]
        public static readonly IndividualUri MD5;

        /// <summary>
        /// <c><see cref="Ds"/>:sha1</c>.
        /// </summary>
        [Uri(Ds, "sha1")]
        public static readonly IndividualUri SHA1;

        /// <summary>
        /// <c><see cref="Dsm"/>:sha224</c>.
        /// </summary>
        [Uri(Dsm, "sha224")]
        public static readonly IndividualUri SHA224;

        /// <summary>
        /// <c><see cref="Enc"/>:sha256</c>.
        /// </summary>
        [Uri(Enc, "sha256")]
        public static readonly IndividualUri SHA256;

        /// <summary>
        /// <c><see cref="Dsm"/>:sha384</c>.
        /// </summary>
        [Uri(Dsm, "sha384")]
        public static readonly IndividualUri SHA384;

        /// <summary>
        /// <c><see cref="Ds"/>:sha512</c>.
        /// </summary>
        [Uri(Ds, "sha512")]
        public static readonly IndividualUri SHA512;

        /// <summary>
        /// <c><see cref="Enc"/>:ripemd160</c>.
        /// </summary>
        [Uri(Enc, "ripemd160")]
        public static readonly IndividualUri RIPEMD160;

        /// <summary>
        /// <c><see cref="Dsm2"/>:sha3-224</c>.
        /// </summary>
        [Uri(Dsm2, "sha3-224")]
        public static readonly IndividualUri SHA3_224;

        /// <summary>
        /// <c><see cref="Dsm2"/>:sha3-256</c>.
        /// </summary>
        [Uri(Dsm2, "sha3-256")]
        public static readonly IndividualUri SHA3_256;

        /// <summary>
        /// <c><see cref="Dsm2"/>:sha3-384</c>.
        /// </summary>
        [Uri(Dsm2, "sha3-384")]
        public static readonly IndividualUri SHA3_384;

        /// <summary>
        /// <c><see cref="Dsm2"/>:sha3-512</c>.
        /// </summary>
        [Uri(Dsm2, "sha3-512")]
        public static readonly IndividualUri SHA3_512;

        /// <summary>
        /// <c><see cref="Dsm2"/>:whirlpool</c>.
        /// </summary>
        [Uri(Dsm2)]
        public static readonly IndividualUri Whirlpool;

        /// <summary>
        /// <c><see cref="At"/>:dhash</c>.
        /// </summary>
        [Uri(At, "dhash")]
        public static readonly IndividualUri DHash;

        /// <summary>
        /// <c><see cref="At"/>:blake3-256</c>.
        /// </summary>
        [Uri(At, "blake3-256")]
        public static readonly IndividualUri Blake3;

        /// <summary>
        /// <c><see cref="At"/>:crc32</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri Crc32;

        /// <summary>
        /// <c><see cref="At"/>:crc64</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri Crc64;

        /// <summary>
        /// <c><see cref="At"/>:xxhash32</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash32;

        /// <summary>
        /// <c><see cref="At"/>:xxhash64</c>.
        /// </summary>
        [Uri(At)]
        public static readonly IndividualUri XxHash64;

        /// <summary>
        /// <c><see cref="Nfo"/>:losslessCompressionType</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri LosslessCompressionType;

        /// <summary>
        /// <c><see cref="Nfo"/>:lossyCompressionType</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri LossyCompressionType;

        /// <summary>
        /// <c><see cref="Nfo"/>:decryptedStatus</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri DecryptedStatus;

        /// <summary>
        /// <c><see cref="Nfo"/>:encryptedStatus</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly IndividualUri EncryptedStatus;

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
