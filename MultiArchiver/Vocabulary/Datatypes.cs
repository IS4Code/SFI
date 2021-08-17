namespace IS4.MultiArchiver.Vocabulary
{
    public static class Datatypes
    {
        [Uri(Vocabularies.Rdf, "XMLLiteral")]
        public static readonly DatatypeUri XmlLiteral;
        [Uri(Vocabularies.Rdf, "HTML")]
        public static readonly DatatypeUri Html;
        [Uri(Vocabularies.Rdf, "JSON")]
        public static readonly DatatypeUri Json;

        [Uri(Vocabularies.Xsd, "anyURI")]
        public static readonly DatatypeUri AnyUri;
        [Uri(Vocabularies.Xsd)]
        public static readonly DatatypeUri HexBinary;
        [Uri(Vocabularies.Xsd)]
        public static readonly DatatypeUri Base64Binary;
        [Uri(Vocabularies.Xsd)]
        public static readonly DatatypeUri Integer;
        [Uri(Vocabularies.Xsd)]
        public static readonly DatatypeUri String;

        [Uri(Vocabularies.Owl)]
        public static readonly DatatypeUri Rational;

        [Uri(Vocabularies.Dt)]
        public static readonly DatatypeUri Byte;
        [Uri(Vocabularies.Dt)]
        public static readonly DatatypeUri Hertz;
        [Uri(Vocabularies.Dt)]
        public static readonly DatatypeUri BitPerSecond;
        [Uri(Vocabularies.Dt)]
        public static readonly DatatypeUri KilobitPerSecond;

        static Datatypes()
        {
            typeof(Datatypes).InitializeUris();
        }
    }
}
