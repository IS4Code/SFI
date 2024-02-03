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

        #region dsm, dsm2, ds, enc
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
        #endregion

        /// <summary>
        /// <c><see cref="At"/>:dhash</c>.
        /// </summary>
        [Uri(At, "dhash")]
        public static readonly IndividualUri DHash;

        #region nfo
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
        #endregion

        #region woc
        /// <summary>
        /// <c><see cref="Woc"/>:Abstract</c>.
        /// </summary>
        [Uri(Woc, "Abstract")]
        public static readonly IndividualUri CodeAbstractModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Boolean</c>.
        /// </summary>
        [Uri(Woc, "Boolean")]
        public static readonly IndividualUri JavaBooleanType;

        /// <summary>
        /// <c><see cref="Woc"/>:Byte</c>.
        /// </summary>
        [Uri(Woc, "Byte")]
        public static readonly IndividualUri JavaByteType;

        /// <summary>
        /// <c><see cref="Woc"/>:Char</c>.
        /// </summary>
        [Uri(Woc, "Char")]
        public static readonly IndividualUri JavaCharType;

        /// <summary>
        /// <c><see cref="Woc"/>:Default</c>.
        /// </summary>
        [Uri(Woc, "Default")]
        public static readonly IndividualUri CodeDefaultModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Double</c>.
        /// </summary>
        [Uri(Woc, "Double")]
        public static readonly IndividualUri JavaDoubleType;

        /// <summary>
        /// <c><see cref="Woc"/>:Final</c>.
        /// </summary>
        [Uri(Woc, "Final")]
        public static readonly IndividualUri CodeFinalModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Float</c>.
        /// </summary>
        [Uri(Woc, "Float")]
        public static readonly IndividualUri JavaFloatType;

        /// <summary>
        /// <c><see cref="Woc"/>:Int</c>.
        /// </summary>
        [Uri(Woc, "Int")]
        public static readonly IndividualUri JavaIntType;

        /// <summary>
        /// <c><see cref="Woc"/>:Long</c>.
        /// </summary>
        [Uri(Woc, "Long")]
        public static readonly IndividualUri JavaLongType;

        /// <summary>
        /// <c><see cref="Woc"/>:Private</c>.
        /// </summary>
        [Uri(Woc, "Private")]
        public static readonly IndividualUri CodePrivateModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Protected</c>.
        /// </summary>
        [Uri(Woc, "Protected")]
        public static readonly IndividualUri CodeProtectedModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Public</c>.
        /// </summary>
        [Uri(Woc, "Public")]
        public static readonly IndividualUri CodePublicModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Short</c>.
        /// </summary>
        [Uri(Woc, "Short")]
        public static readonly IndividualUri JavaShortType;

        /// <summary>
        /// <c><see cref="Woc"/>:Static</c>.
        /// </summary>
        [Uri(Woc, "Static")]
        public static readonly IndividualUri CodeStaticModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Synchronized</c>.
        /// </summary>
        [Uri(Woc, "Synchronized")]
        public static readonly IndividualUri CodeSynchronizedModifier;

        /// <summary>
        /// <c><see cref="Woc"/>:Volatile</c>.
        /// </summary>
        [Uri(Woc, "Volatile")]
        public static readonly IndividualUri CodeVolatileModifier;
        #endregion

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
