namespace IS4.MultiArchiver.Vocabulary
{
    public static class Individuals
    {
        [Uri(Vocabularies.Uri.Dsm, "md5")]
        public static readonly IndividualUri MD5;
        [Uri(Vocabularies.Uri.Ds, "sha1")]
        public static readonly IndividualUri SHA1;
        [Uri(Vocabularies.Uri.Dsm, "sha224")]
        public static readonly IndividualUri SHA224;
        [Uri(Vocabularies.Uri.Enc, "sha256")]
        public static readonly IndividualUri SHA256;
        [Uri(Vocabularies.Uri.Dsm, "sha384")]
        public static readonly IndividualUri SHA384;
        [Uri(Vocabularies.Uri.Ds, "sha512")]
        public static readonly IndividualUri SHA512;
        [Uri(Vocabularies.Uri.Enc, "ripemd160")]
        public static readonly IndividualUri RIPEMD160;
        [Uri(Vocabularies.Uri.Dsm2, "sha3-224")]
        public static readonly IndividualUri SHA3_224;
        [Uri(Vocabularies.Uri.Dsm2, "sha3-256")]
        public static readonly IndividualUri SHA3_256;
        [Uri(Vocabularies.Uri.Dsm2, "sha3-384")]
        public static readonly IndividualUri SHA3_384;
        [Uri(Vocabularies.Uri.Dsm2, "sha3-512")]
        public static readonly IndividualUri SHA3_512;
        [Uri(Vocabularies.Uri.Dsm2)]
        public static readonly IndividualUri Whirlpool;
        [Uri(Vocabularies.Uri.At, "btih")]
        public static readonly IndividualUri BTIH;
        [Uri(Vocabularies.Uri.At, "bsha1-256")]
        public static readonly IndividualUri BSHA1_256;
        [Uri(Vocabularies.Uri.At, "bsha1-512")]
        public static readonly IndividualUri BSHA1_512;
        [Uri(Vocabularies.Uri.At, "bsha1-1024")]
        public static readonly IndividualUri BSHA1_1024;
        [Uri(Vocabularies.Uri.At, "dhash")]
        public static readonly IndividualUri DHash;
        [Uri(Vocabularies.Uri.At, "blake3-256")]
        public static readonly IndividualUri Blake3;
        [Uri(Vocabularies.Uri.At)]
        public static readonly IndividualUri Crc32;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly IndividualUri LosslessCompressionType;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly IndividualUri LossyCompressionType;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly IndividualUri DecryptedStatus;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly IndividualUri EncryptedStatus;

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
