namespace IS4.MultiArchiver.Vocabulary
{
    public static class Individuals
    {
        [Uri(Vocabularies.Dsm, "md5")]
        public static readonly IndividualUri MD5;
        [Uri(Vocabularies.Ds, "sha1")]
        public static readonly IndividualUri SHA1;
        [Uri(Vocabularies.Dsm, "sha224")]
        public static readonly IndividualUri SHA224;
        [Uri(Vocabularies.Enc, "sha256")]
        public static readonly IndividualUri SHA256;
        [Uri(Vocabularies.Dsm, "sha384")]
        public static readonly IndividualUri SHA384;
        [Uri(Vocabularies.Ds, "sha512")]
        public static readonly IndividualUri SHA512;
        [Uri(Vocabularies.Enc, "ripemd160")]
        public static readonly IndividualUri RIPEMD160;
        [Uri(Vocabularies.Dsm2, "sha3-224")]
        public static readonly IndividualUri SHA3_224;
        [Uri(Vocabularies.Dsm2, "sha3-256")]
        public static readonly IndividualUri SHA3_256;
        [Uri(Vocabularies.Dsm2, "sha3-384")]
        public static readonly IndividualUri SHA3_384;
        [Uri(Vocabularies.Dsm2, "sha3-512")]
        public static readonly IndividualUri SHA3_512;
        [Uri(Vocabularies.Dsm2)]
        public static readonly IndividualUri Whirlpool;
        [Uri(Vocabularies.At, "btih")]
        public static readonly IndividualUri BTIH;
        [Uri(Vocabularies.At, "bsha1-256")]
        public static readonly IndividualUri BSHA1_256;
        [Uri(Vocabularies.At, "bsha1-512")]
        public static readonly IndividualUri BSHA1_512;
        [Uri(Vocabularies.At, "bsha1-1024")]
        public static readonly IndividualUri BSHA1_1024;
        [Uri(Vocabularies.At, "dhash")]
        public static readonly IndividualUri DHash;
        [Uri(Vocabularies.At, "blake3-256")]
        public static readonly IndividualUri Blake3;
        [Uri(Vocabularies.At)]
        public static readonly IndividualUri Crc32;
        [Uri(Vocabularies.Nfo)]
        public static readonly IndividualUri LosslessCompressionType;
        [Uri(Vocabularies.Nfo)]
        public static readonly IndividualUri LossyCompressionType;
        [Uri(Vocabularies.Nfo)]
        public static readonly IndividualUri DecryptedStatus;
        [Uri(Vocabularies.Nfo)]
        public static readonly IndividualUri EncryptedStatus;

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
