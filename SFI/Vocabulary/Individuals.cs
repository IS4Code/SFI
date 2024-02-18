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

        #region xaml
        /// <summary>
        /// <c><see cref="Xaml"/>:Object</c>.
        /// </summary>
        [Uri(Woc, "Object")]
        public static readonly IndividualUri CtsObjectType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Boolean</c>.
        /// </summary>
        [Uri(Woc, "Boolean")]
        public static readonly IndividualUri CtsBooleanType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Char</c>.
        /// </summary>
        [Uri(Woc, "Char")]
        public static readonly IndividualUri CtsCharType;

        /// <summary>
        /// <c><see cref="Xaml"/>:String</c>.
        /// </summary>
        [Uri(Woc, "String")]
        public static readonly IndividualUri CtsStringType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Decimal</c>.
        /// </summary>
        [Uri(Woc, "Decimal")]
        public static readonly IndividualUri CtsDecimalType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Single</c>.
        /// </summary>
        [Uri(Woc, "Single")]
        public static readonly IndividualUri CtsSingleType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Double</c>.
        /// </summary>
        [Uri(Woc, "Double")]
        public static readonly IndividualUri CtsDoubleType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Int16</c>.
        /// </summary>
        [Uri(Woc, "Int16")]
        public static readonly IndividualUri CtsInt16Type;

        /// <summary>
        /// <c><see cref="Xaml"/>:Int32</c>.
        /// </summary>
        [Uri(Woc, "Int32")]
        public static readonly IndividualUri CtsInt32Type;

        /// <summary>
        /// <c><see cref="Xaml"/>:Int64</c>.
        /// </summary>
        [Uri(Woc, "Int64")]
        public static readonly IndividualUri CtsInt64Type;

        /// <summary>
        /// <c><see cref="Xaml"/>:TimeSpan</c>.
        /// </summary>
        [Uri(Woc, "TimeSpan")]
        public static readonly IndividualUri CtsTimeSpanType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Uri</c>.
        /// </summary>
        [Uri(Woc, "Uri")]
        public static readonly IndividualUri CtsUriType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Byte</c>.
        /// </summary>
        [Uri(Woc, "Byte")]
        public static readonly IndividualUri CtsByteType;

        /// <summary>
        /// <c><see cref="Xaml"/>:Array</c>.
        /// </summary>
        [Uri(Woc, "Array")]
        public static readonly IndividualUri CtsArrayType;
        #endregion

        #region err
        /// <summary>
        /// Error codes defined by <c><see cref="Err"/>:</c>.
        /// </summary>
        public static class Errors
        {
            /// <summary>Wrong number of arguments.</summary>
            [Uri(Err, "FOAP0001")]
            public static readonly IndividualUri WrongNumberOfArguments;

            /// <summary>Division by zero.</summary>
            [Uri(Err, "FOAR0001")]
            public static readonly IndividualUri DivisionByZero;

            /// <summary>Numeric operation overflow/underflow.</summary>
            [Uri(Err, "FOAR0002")]
            public static readonly IndividualUri NumericOperationOverflowOrUnderflow;

            /// <summary>Array index out of bounds.</summary>
            [Uri(Err, "FOAY0001")]
            public static readonly IndividualUri ArrayIndexOutOfBounds;

            /// <summary>Negative array length.</summary>
            [Uri(Err, "FOAY0002")]
            public static readonly IndividualUri NegativeArrayLength;

            /// <summary>Input value too large for decimal.</summary>
            [Uri(Err, "FOCA0001")]
            public static readonly IndividualUri InputValueTooLargeForDecimal;

            /// <summary>Invalid lexical value.</summary>
            [Uri(Err, "FOCA0002")]
            public static readonly IndividualUri InvalidLexicalValue;

            /// <summary>Input value too large for integer.</summary>
            [Uri(Err, "FOCA0003")]
            public static readonly IndividualUri InputValueTooLargeForInteger;

            /// <summary>NaN supplied as float/double value.</summary>
            [Uri(Err, "FOCA0005")]
            public static readonly IndividualUri NaNSuppliedAsFloatOrDoubleValue;

            /// <summary>String to be cast to decimal has too many digits of precision.</summary>
            [Uri(Err, "FOCA0006")]
            public static readonly IndividualUri TooManyDigitsOfPrecisionInString;

            /// <summary>Codepoint not valid.</summary>
            [Uri(Err, "FOCH0001")]
            public static readonly IndividualUri CodepointNotValid;

            /// <summary>Unsupported collation.</summary>
            [Uri(Err, "FOCH0002")]
            public static readonly IndividualUri UnsupportedCollation;

            /// <summary>Unsupported normalization form.</summary>
            [Uri(Err, "FOCH0003")]
            public static readonly IndividualUri UnsupportedNormalizationForm;

            /// <summary>Collation does not support collation units.</summary>
            [Uri(Err, "FOCH0004")]
            public static readonly IndividualUri CollationUnitsNotSupported;

            /// <summary>No context document.</summary>
            [Uri(Err, "FODC0001")]
            public static readonly IndividualUri NoContextDocument;

            /// <summary>Error retrieving resource.</summary>
            [Uri(Err, "FODC0002")]
            public static readonly IndividualUri ErrorRetrievingResource;

            /// <summary>Function not defined as deterministic.</summary>
            [Uri(Err, "FODC0003")]
            public static readonly IndividualUri FunctionNotDeterministic;

            /// <summary>Invalid collection URI.</summary>
            [Uri(Err, "FODC0004")]
            public static readonly IndividualUri InvalidCollectionUri;

            /// <summary>Invalid document URI.</summary>
            [Uri(Err, "FODC0005")]
            public static readonly IndividualUri InvalidDocumentUri;

            /// <summary>Invalid XML document.</summary>
            [Uri(Err, "FODC0006")]
            public static readonly IndividualUri InvalidXmlDocument;

            /// <summary>Invalid decimal format name.</summary>
            [Uri(Err, "FODF1280")]
            public static readonly IndividualUri InvalidDecimalFormatName;

            /// <summary>Invalid decimal format picture string.</summary>
            [Uri(Err, "FODF1310")]
            public static readonly IndividualUri InvalidDecimalFormatPictureString;

            /// <summary>Overflow/underflow in date/time operation.</summary>
            [Uri(Err, "FODT0001")]
            public static readonly IndividualUri OverflowOrUnderflowInDateOrTimeOperation;

            /// <summary>Overflow/underflow in duration operation.</summary>
            [Uri(Err, "FODT0002")]
            public static readonly IndividualUri OverflowOrUnderflowInDurationOperation;

            /// <summary>Invalid timezone value.</summary>
            [Uri(Err, "FODT0003")]
            public static readonly IndividualUri InvalidTimezoneValue;

            /// <summary>Unidentified error.</summary>
            [Uri(Err, "FOER0000")]
            public static readonly IndividualUri UnidentifiedError;

            /// <summary>Invalid date/time formatting parameters.</summary>
            [Uri(Err, "FOFD1340")]
            public static readonly IndividualUri InvalidDateOrTimeFormattingParameters;

            /// <summary>Invalid date/time formatting component.</summary>
            [Uri(Err, "FOFD1350")]
            public static readonly IndividualUri InvalidDateOrTimeFormattingComponent;

            /// <summary>Invalid options.</summary>
            [Uri(Err, "FOJS0005")]
            public static readonly IndividualUri InvalidOptions;

            /// <summary>Invalid value for cast/constructor.</summary>
            [Uri(Err, "FORG0001")]
            public static readonly IndividualUri InvalidValueForCastOrConstructor;

            /// <summary>Invalid argument type.</summary>
            [Uri(Err, "FORG0006")]
            public static readonly IndividualUri InvalidArgumentType;

            /// <summary>Invalid date/time.</summary>
            [Uri(Err, "FORG0010")]
            public static readonly IndividualUri InvalidDateOrTime;

            /// <summary>Invalid regular expression flags.</summary>
            [Uri(Err, "FORX0001")]
            public static readonly IndividualUri InvalidRegularExpressionFlags;

            /// <summary>Invalid regular expression.</summary>
            [Uri(Err, "FORX0002")]
            public static readonly IndividualUri InvalidRegularExpression;

            /// <summary>Regular expression matches zero-length string.</summary>
            [Uri(Err, "FORX0003")]
            public static readonly IndividualUri RegularExpressionMatchesEmptyString;

            /// <summary>Invalid replacement string.</summary>
            [Uri(Err, "FORX0004")]
            public static readonly IndividualUri InvalidReplacementString;

            static Errors()
            {
                typeof(Errors).InitializeUris();
            }
        }
        #endregion

        static Individuals()
        {
            typeof(Individuals).InitializeUris();
        }
    }
}
