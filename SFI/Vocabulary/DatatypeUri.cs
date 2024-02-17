using System;
using System.Xml;
using System.Xml.Schema;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Represents an RDF datatype term in a vocabulary.
    /// </summary>
    public struct DatatypeUri : ITermUri<DatatypeUri>
    {
        /// <inheritdoc/>
        public VocabularyUri Vocabulary { get; }

        /// <inheritdoc/>
        public string Term { get; }

        /// <inheritdoc/>
        public string Value => Vocabulary.Value + Term;

        /// <summary>
        /// Creates a new instance of the term.
        /// </summary>
        /// <param name="vocabulary">The value of <see cref="Vocabulary"/>.</param>
        /// <param name="term">The value of <see cref="Term"/>.</param>
        public DatatypeUri(VocabularyUri vocabulary, string term)
        {
            Vocabulary = vocabulary;
            Term = term;
        }

        /// <summary>
        /// Creates a new instance of the term from a field.
        /// </summary>
        /// <param name="uriAttribute">The attribute identifying the vocabulary and local name of the term.</param>
        /// <param name="fieldName">
        /// The name of the field, used as a fallback for <see cref="Term"/>,
        /// converted via <see cref="Extensions.ToCamelCase(string)"/>.
        /// </param>
        public DatatypeUri(UriAttribute uriAttribute, string fieldName)
            : this(uriAttribute.Vocabulary, uriAttribute.LocalName ?? fieldName.ToCamelCase())
        {

        }

        /// <summary>
        /// Creates a new instance from an XML type code.
        /// </summary>
        /// <param name="typeCode">The XML Schema type code identifying the datatype.</param>
        public DatatypeUri(XmlTypeCode typeCode) : this(XmlSchemaType.GetBuiltInSimpleType(typeCode) ?? throw new ArgumentOutOfRangeException(nameof(typeCode), "The argument is not a valid XML Schema simple type code."))
        {

        }

        /// <summary>
        /// Creates a new instance from an XML type.
        /// </summary>
        /// <param name="schemaType">The XML Schema type identifying the datatype.</param>
        public DatatypeUri(XmlSchemaType schemaType) : this(schemaType.QualifiedName)
        {

        }

        /// <summary>
        /// Creates a new instance from an XML qualified name.
        /// </summary>
        /// <param name="qualifiedName">The XML qualified name identifying the datatype.</param>
        public DatatypeUri(XmlQualifiedName qualifiedName) : this(GetXmlnsVocabulary(qualifiedName.Namespace), qualifiedName.Name)
        {

        }

        static VocabularyUri GetXmlnsVocabulary(string ns)
        {
            // For #-ending namespace just assume it is the whole prefix
            var uri = ns.EndsWith("#", StringComparison.Ordinal)
                ? ns // otherwise form the prefix naturally
                : UriTools.MakeSubUri(new Uri(ns, UriKind.Absolute), "#").AbsoluteUri;
            return new VocabularyUri(String.Intern(uri));
        }

        /// <summary>
        /// Returns <see cref="Value"/> formatted as a URI node.
        /// </summary>
        /// <returns>The formatted value of the instance.</returns>
        public override string ToString()
        {
            return $"<{Value}>";
        }

        /// <inheritdoc/>
        public bool Equals(DatatypeUri other)
        {
            return Vocabulary == other.Vocabulary && Term == other.Term;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is DatatypeUri uri && Equals(uri);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Vocabulary.Value, Term);
        }

        /// <summary>
        /// Compares two instances of <see cref="DatatypeUri"/> for equality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The result of <see cref="Equals(DatatypeUri)"/>.</returns>
        public static bool operator ==(DatatypeUri a, DatatypeUri b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two instances of <see cref="DatatypeUri"/> for inequality.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns>The negated result of <see cref="Equals(DatatypeUri)"/>.</returns>
        public static bool operator !=(DatatypeUri a, DatatypeUri b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Converts an XML type code to a <see cref="DatatypeUri"/> instance.
        /// </summary>
        /// <param name="typeCode">The XML Schema type code identifying the datatype.</param>
        public static implicit operator DatatypeUri(XmlTypeCode typeCode)
        {
            return new(typeCode);
        }

        /// <summary>
        /// Converts an XML type to a <see cref="DatatypeUri"/> instance.
        /// </summary>
        /// <param name="schemaType">The XML Schema type identifying the datatype.</param>
        public static implicit operator DatatypeUri(XmlSchemaType schemaType)
        {
            return new(schemaType);
        }

        /// <summary>
        /// Converts a <see cref="DatatypeUri"/> instance to an XML type code, or <see langword="null"/> if the conversion is not possible.
        /// </summary>
        /// <param name="datatype">The datatype to convert.</param>
        public static explicit operator XmlTypeCode?(DatatypeUri datatype)
        {
            if(datatype.Vocabulary.Value != Vocabularies.Uri.Xsd)
            {
                return null;
            }
            var qname = new XmlQualifiedName(datatype.Term, XmlSchema.Namespace);
            var type = XmlSchemaType.GetBuiltInSimpleType(qname);
            if(type is not { TypeCode: var typeCode and not XmlTypeCode.None })
            {
                return null;
            }
            return typeCode;
        }

        /// <summary>
        /// Converts a <see cref="DatatypeUri"/> instance to an XML type code.
        /// </summary>
        /// <param name="datatype">The datatype to convert.</param>
        /// <exception cref="ArgumentException"><paramref name="datatype"/> does not identify a built-in XML schema datatype.</exception>
        public static explicit operator XmlTypeCode(DatatypeUri datatype)
        {
            return (XmlTypeCode?)datatype ?? throw new ArgumentException("Argument does not identify a built-in XML schema datatype.", nameof(datatype));
        }
    }
}
