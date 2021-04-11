namespace IS4.MultiArchiver.Vocabulary
{
    public enum Individuals
    {
        [Uri(Vocabularies.XmlDSigMore, "md5")]
        MD5,
        [Uri(Vocabularies.XmlDSig, "sha1")]
        SHA1,
        [Uri(Vocabularies.XmlDSigMore, "sha224")]
        SHA224,
        [Uri(Vocabularies.XmlEnc, "sha256")]
        SHA256,
        [Uri(Vocabularies.XmlDSigMore, "sha384")]
        SHA384,
        [Uri(Vocabularies.XmlDSig, "sha512")]
        SHA512,
        [Uri(Vocabularies.XmlEnc, "ripemd160")]
        RIPEMD160,
        [Uri(Vocabularies.XmlDSigMore2, "sha3-224")]
        SHA3_224,
        [Uri(Vocabularies.XmlDSigMore2, "sha3-256")]
        SHA3_256,
        [Uri(Vocabularies.XmlDSigMore2, "sha3-384")]
        SHA3_384,
        [Uri(Vocabularies.XmlDSigMore2, "sha3-512")]
        SHA3_512,
        [Uri(Vocabularies.XmlDSigMore2)]
        Whirlpool,
        [Uri(Vocabularies.At, "btih")]
        BTIH,
    }
}
