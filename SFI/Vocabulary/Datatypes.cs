namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF datatypes, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Datatypes
    {
        /// <summary>
        /// <see cref="Rdf"/>:XMLLiteral.
        /// </summary>
        [Uri(Rdf, "XMLLiteral")]
        public static readonly DatatypeUri XmlLiteral;

        /// <summary>
        /// <see cref="Rdf"/>:HTML.
        /// </summary>
        [Uri(Rdf, "HTML")]
        public static readonly DatatypeUri Html;

        /// <summary>
        /// <see cref="Rdf"/>:JSON.
        /// </summary>
        [Uri(Rdf, "JSON")]
        public static readonly DatatypeUri Json;

        /// <summary>
        /// <see cref="Xsd"/>:anyURI.
        /// </summary>
        [Uri(Xsd, "anyURI")]
        public static readonly DatatypeUri AnyUri;

        /// <summary>
        /// <see cref="Xsd"/>:hexBinary.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri HexBinary;

        /// <summary>
        /// <see cref="Xsd"/>:base64Binary.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri Base64Binary;

        /// <summary>
        /// <see cref="Xsd"/>:integer.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri Integer;

        /// <summary>
        /// <see cref="Xsd"/>:string.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri String;

        /// <summary>
        /// <see cref="Xsd"/>:dateTime.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri DateTime;

        /// <summary>
        /// <see cref="Owl"/>:rational.
        /// </summary>
        [Uri(Owl)]
        public static readonly DatatypeUri Rational;

        /// <summary>
        /// <see cref="Dt"/>:byte.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri Byte;

        /// <summary>
        /// <see cref="Dt"/>:hertz.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri Hertz;

        /// <summary>
        /// <see cref="Dt"/>:bitPerSecond.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri BitPerSecond;

        /// <summary>
        /// <see cref="Dt"/>:kilobitPerSecond.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri KilobitPerSecond;

        static Datatypes()
        {
            typeof(Datatypes).InitializeUris();
        }
    }
}
