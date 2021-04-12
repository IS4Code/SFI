namespace IS4.MultiArchiver.Vocabulary
{
    public enum Individuals
    {
        [Uri(Vocabularies.Dsm, "md5")]
        MD5,
        [Uri(Vocabularies.Ds, "sha1")]
        SHA1,
        [Uri(Vocabularies.Dsm, "sha224")]
        SHA224,
        [Uri(Vocabularies.Enc, "sha256")]
        SHA256,
        [Uri(Vocabularies.Dsm, "sha384")]
        SHA384,
        [Uri(Vocabularies.Ds, "sha512")]
        SHA512,
        [Uri(Vocabularies.Enc, "ripemd160")]
        RIPEMD160,
        [Uri(Vocabularies.Dsm2, "sha3-224")]
        SHA3_224,
        [Uri(Vocabularies.Dsm2, "sha3-256")]
        SHA3_256,
        [Uri(Vocabularies.Dsm2, "sha3-384")]
        SHA3_384,
        [Uri(Vocabularies.Dsm2, "sha3-512")]
        SHA3_512,
        [Uri(Vocabularies.Dsm2)]
        Whirlpool,
        [Uri(Vocabularies.At, "btih")]
        BTIH,
        [Uri(Vocabularies.At, "bsha1-256")]
        BSHA1_256,
        [Uri(Vocabularies.At, "bsha1-512")]
        BSHA1_512,
        [Uri(Vocabularies.At, "bsha1-1024")]
        BSHA1_1024,
    }
}
