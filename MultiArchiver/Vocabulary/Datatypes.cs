namespace IS4.MultiArchiver.Vocabulary
{
    public static class Datatypes
    {
        [Uri(Vocabularies.Uri.Rdf, "XMLLiteral")]
        public static readonly DatatypeUri XmlLiteral;
        [Uri(Vocabularies.Uri.Rdf, "HTML")]
        public static readonly DatatypeUri Html;
        [Uri(Vocabularies.Uri.Rdf, "JSON")]
        public static readonly DatatypeUri Json;

        [Uri(Vocabularies.Uri.Xsd, "anyURI")]
        public static readonly DatatypeUri AnyUri;
        [Uri(Vocabularies.Uri.Xsd)]
        public static readonly DatatypeUri HexBinary;
        [Uri(Vocabularies.Uri.Xsd)]
        public static readonly DatatypeUri Base64Binary;
        [Uri(Vocabularies.Uri.Xsd)]
        public static readonly DatatypeUri Integer;
        [Uri(Vocabularies.Uri.Xsd)]
        public static readonly DatatypeUri String;
        [Uri(Vocabularies.Uri.Xsd)]
        public static readonly DatatypeUri DateTime;

        [Uri(Vocabularies.Uri.Owl)]
        public static readonly DatatypeUri Rational;

        [Uri(Vocabularies.Uri.Dt)]
        public static readonly DatatypeUri Byte;
        [Uri(Vocabularies.Uri.Dt)]
        public static readonly DatatypeUri Hertz;
        [Uri(Vocabularies.Uri.Dt)]
        public static readonly DatatypeUri BitPerSecond;
        [Uri(Vocabularies.Uri.Dt)]
        public static readonly DatatypeUri KilobitPerSecond;

        static Datatypes()
        {
            typeof(Datatypes).InitializeUris();
        }
    }
}
