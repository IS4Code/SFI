namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF datatypes, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Datatypes
    {
        #region rdf
        /// <summary>
        /// <c><see cref="Rdf"/>:XMLLiteral</c>.
        /// </summary>
        [Uri(Rdf, "XMLLiteral")]
        public static readonly DatatypeUri XmlLiteral;

        /// <summary>
        /// <c><see cref="Rdf"/>:HTML</c>.
        /// </summary>
        [Uri(Rdf, "HTML")]
        public static readonly DatatypeUri Html;

        /// <summary>
        /// <c><see cref="Rdf"/>:JSON</c>.
        /// </summary>
        [Uri(Rdf, "JSON")]
        public static readonly DatatypeUri Json;
        #endregion

        #region xsd
        /// <summary>
        /// <c><see cref="Xsd"/>:anyURI</c>.
        /// </summary>
        [Uri(Xsd, "anyURI")]
        public static readonly DatatypeUri AnyUri;

        /// <summary>
        /// <c><see cref="Xsd"/>:hexBinary</c>.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri HexBinary;

        /// <summary>
        /// <c><see cref="Xsd"/>:base64Binary</c>.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri Base64Binary;

        /// <summary>
        /// <c><see cref="Xsd"/>:integer</c>.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri Integer;

        /// <summary>
        /// <c><see cref="Xsd"/>:string</c>.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri String;

        /// <summary>
        /// <c><see cref="Xsd"/>:dateTime</c>.
        /// </summary>
        [Uri(Xsd)]
        public static readonly DatatypeUri DateTime;
        #endregion

        /// <summary>
        /// <c><see cref="Owl"/>:rational</c>.
        /// </summary>
        [Uri(Owl)]
        public static readonly DatatypeUri Rational;

        #region dt
        /// <summary>
        /// <c><see cref="Dt"/>:byte</c>.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri Byte;

        /// <summary>
        /// <c><see cref="Dt"/>:hertz</c>.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri Hertz;

        /// <summary>
        /// <c><see cref="Dt"/>:bitPerSecond</c>.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri BitPerSecond;

        /// <summary>
        /// <c><see cref="Dt"/>:kilobitPerSecond</c>.
        /// </summary>
        [Uri(Dt)]
        public static readonly DatatypeUri KilobitPerSecond;
        #endregion

        static Datatypes()
        {
            typeof(Datatypes).InitializeUris();
        }
    }
}
