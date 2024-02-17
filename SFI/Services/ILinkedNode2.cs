using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Services
{
    /// <summary>
    /// Represents a write-only abstraction of an RDF resource,
    /// allowing setting its classes as <see cref="ClassUri"/>
    /// and properties as <see cref="PropertyUri"/>.
    /// The node is usually internally backed by a URI.
    /// </summary>
    public interface ILinkedNode : IEquatable<ILinkedNode>
    {
        /// <summary>
        /// The scheme of the URI identifying this node.
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// Describes the node using the RDF/XML description provided
        /// through <paramref name="rdfXmlReader"/>.
        /// </summary>
        /// <param name="rdfXmlReader">
        /// An XML reader for a valid RDF/XML document. The document
        /// shall describe the node by using a blank relative URI,
        /// i.e. <c>&lt;rdf:Description rdf:about=""&gt;</c>
        /// </param>
        /// <param name="subjectUris">
        /// Stores a collection of URIs that represent this node.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="rdfXmlReader"/> is not positioned on an
        /// <c>{http://www.w3.org/1999/02/22-rdf-syntax-ns#}RDF</c> element.
        /// </exception>
        void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri})"/>
        ValueTask DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Describes the node using the RDF/XML description provided
        /// through <paramref name="rdfXmlReaderFactory"/>.
        /// </summary>
        /// <param name="rdfXmlReaderFactory">
        /// A function returning an XML reader, like for <see cref="Describe(XmlReader, IReadOnlyCollection{Uri}?)"/>,
        /// but the argument is an URI that should be used as the base URI of the graph.
        /// </param>
        /// <param name="subjectUris"><inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri}?)" path="/param[@name='subjectUris']"/></param>
        /// <exception cref="ArgumentException">
        /// The reader returned by <paramref name="rdfXmlReaderFactory"/>
        /// is not positioned on an
        /// <c>{http://www.w3.org/1999/02/22-rdf-syntax-ns#}RDF</c> element.
        /// </exception>
        void Describe(Func<Uri, XmlReader?> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc cref="Describe(Func{Uri, XmlReader}, IReadOnlyCollection{Uri}?)"/>
        ValueTask DescribeAsync(Func<Uri, ValueTask<XmlReader?>> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Describes the node using the RDF/XML description provided
        /// in <paramref name="rdfXmlDocument"/>.
        /// </summary>
        /// <param name="rdfXmlDocument">
        /// A valid RDF/XML document. The document
        /// shall describe the node by using a blank relative URI,
        /// i.e. <c>&lt;rdf:Description rdf:about=""&gt;</c>
        /// </param>
        /// <param name="subjectUris"><inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri}?)" path="/param[@name='subjectUris']"/></param>
        void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Describes the node using the RDF/XML description
        /// obtained when calling <paramref name="rdfXmlDocumentFactory"/>.
        /// </summary>
        /// <param name="rdfXmlDocumentFactory">
        /// A function returning an XML document, like for <see cref="Describe(XmlDocument, IReadOnlyCollection{Uri}?)"/>,
        /// but the argument is an URI that should be used as the base URI of the graph.
        /// </param>
        /// <param name="subjectUris"><inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri}?)" path="/param[@name='subjectUris']"/></param>
        void Describe(Func<Uri, XmlDocument?> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc cref="Describe(Func{Uri, XmlDocument?}, IReadOnlyCollection{Uri}?)"/>
        ValueTask DescribeAsync(Func<Uri, ValueTask<XmlDocument?>> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Attempts to describe the node in an implementation-specific
        /// way, using a <paramref name="loader"/> which, if recognized,
        /// operates on the result of <paramref name="dataSource"/>.
        /// </summary>
        /// <param name="loader">An instance of an implementation-specific RDF loader class.</param>
        /// <param name="dataSource">
        /// A function that provides the argument to the loader.
        /// The parameter is a URI that should be used as the base.</param>
        /// <param name="subjectUris"><inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri}?)" path="/param[@name='subjectUris']"/></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="loader"/> was correctly recognized
        /// and executed, <see langword="false"/> otherwise.
        /// </returns>
        bool TryDescribe(object loader, Func<Uri, object?> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc cref="TryDescribe(object, Func{Uri, object}, IReadOnlyCollection{Uri}?)"/>
        ValueTask<bool> TryDescribeAsync(object loader, Func<Uri, ValueTask<object?>> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Sets one of the classes of the resource to be <paramref name="class"/>.
        /// </summary>
        /// <param name="class">The class to add to the list of classes of this resource.</param>
        void SetClass(ClassUri @class);

        /// <summary>
        /// Sets one of the classes of the resource to be the result of
        /// formatting <paramref name="value"/> with <paramref name="formatter"/>.
        /// </summary>
        /// <typeparam name="TClass">The type of the formatted value.</typeparam>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="value">The value to use with <paramref name="formatter"/>.</param>
        void SetClass<TClass>(IClassUriFormatter<TClass> formatter, TClass value);

        /// <summary>
        /// Sets one of the properties to an individual resource.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The individual value to assign.</param>
        void Set(PropertyUri property, IndividualUri value);

        /// <summary>
        /// Sets one of the properties to a plain literal.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The literal value to assign.</param>
        void Set(PropertyUri property, string value);

        /// <summary>
        /// Sets one of the properties to a literal with a particular datatype.
        /// </summary>
        /// <param name="datatype">The datatype of the literal.</param>
        /// <param name="property"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='property']"/></param>
        /// <param name="value"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='value']"/></param>
        void Set(PropertyUri property, string value, DatatypeUri datatype);

        /// <summary>
        /// Sets one of the properties to a literal with a datatype produced from a formatter.
        /// </summary>
        /// <typeparam name="TData">The type supported by <paramref name="datatypeFormatter"/>.</typeparam>
        /// <param name="datatypeFormatter">The formatter to use for the datatype.</param>
        /// <param name="datatypeValue">The value to format for the datatype.</param>
        /// <param name="property"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='property']"/></param>
        /// <param name="value"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='value']"/></param>
        void Set<TData>(PropertyUri property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);

        /// <summary>
        /// Sets one of the properties to the string value of a literal with a particular datatype.
        /// </summary>
        /// <typeparam name="TValue">The type of the literal.</typeparam>
        /// <inheritdoc cref="Set(PropertyUri, string, DatatypeUri)"/>
        void Set<TValue>(PropertyUri property, TValue value, DatatypeUri datatype) where TValue : IFormattable;

        /// <summary>
        /// Sets one of the properties to the string value of a literal with a datatype produced from a formatter.
        /// </summary>
        /// <typeparam name="TData">The type supported by <paramref name="datatypeFormatter"/>.</typeparam>
        /// <param name="datatypeFormatter">The formatter to use for the datatype.</param>
        /// <param name="datatypeValue">The value to format for the datatype.</param>
        /// <param name="property"><inheritdoc cref="Set{TValue}(PropertyUri, TValue, DatatypeUri)" path="/param[@name='property']"/></param>
        /// <param name="value"><inheritdoc cref="Set{TValue}(PropertyUri, TValue, DatatypeUri)" path="/param[@name='value']"/></param>
        /// <typeparam name="TValue"><inheritdoc cref="Set{TValue}(PropertyUri, TValue, DatatypeUri)" path="/typeparam[@name='TValue']"/></typeparam>
        void Set<TValue, TData>(PropertyUri property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;

        /// <summary>
        /// Sets one of the properties to a literal with a particular language.
        /// </summary>
        /// <param name="language">The language of the literal.</param>
        /// <param name="property"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='property']"/></param>
        /// <param name="value"><inheritdoc cref="Set(PropertyUri, string)" path="/param[@name='value']"/></param>
        void Set(PropertyUri property, string value, LanguageCode language);

        /// <summary>
        /// Sets one of the properties to an individual produced from a formatter.
        /// </summary>
        /// <typeparam name="TValue">The type of <paramref name="value"/>.</typeparam>
        /// <param name="property">The property to set.</param>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="value">The value to format.</param>
        void Set<TValue>(PropertyUri property, IIndividualUriFormatter<TValue> formatter, TValue value);

        /// <summary>
        /// Sets one of the properties to a URI literal.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The URI value to assign.</param>
        void Set(PropertyUri property, Uri value);

        /// <summary>
        /// Sets one of the properties to a resource identified by another <see cref="ILinkedNode"/>.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The node to assign.</param>
        void Set(PropertyUri property, ILinkedNode value);

        /// <summary>
        /// Sets one of the properties to a boolean value.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The boolean value to assign.</param>
        void Set(PropertyUri property, bool value);

        /// <summary>
        /// Sets one of the properties to a literal value with an automatically recognized type.
        /// </summary>
        /// <typeparam name="TValue">The type of the literal.</typeparam>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value to assign.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> doesn't have a recognized datatype.
        /// </exception>
        void Set<TValue>(PropertyUri property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        /// <typeparam name="TProp">The type of <paramref name="propertyValue"/>.</typeparam>
        /// <param name="propertyFormatter">The formatter to use for the property.</param>
        /// <param name="propertyValue">The value formatted with <paramref name="propertyFormatter"/>.</param>
        /// <inheritdoc cref="Set(PropertyUri, IndividualUri)"/>
        /// <param name="value"><inheritdoc cref="Set(PropertyUri, IndividualUri)" path="/param[@name='value']"/></param>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IndividualUri value);

        /// <inheritdoc cref="Set(PropertyUri, string)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value);

        /// <inheritdoc cref="Set(PropertyUri, string, DatatypeUri)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, DatatypeUri datatype);

        /// <inheritdoc cref="Set{TValue, TData}(PropertyUri, TValue, IDatatypeUriFormatter{TData}, TData)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);

        /// <inheritdoc cref="Set{TValue}(PropertyUri, TValue, DatatypeUri)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, DatatypeUri datatype) where TValue : IFormattable;

        /// <inheritdoc cref="Set{TValue, TData}(PropertyUri, TValue, IDatatypeUriFormatter{TData}, TData)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp, TValue, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;

        /// <inheritdoc cref="Set(PropertyUri, string, LanguageCode)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, LanguageCode language);

        /// <inheritdoc cref="Set{TValue}(PropertyUri, IIndividualUriFormatter{TValue}, TValue)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value);

        /// <inheritdoc cref="Set(PropertyUri, Uri)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value);

        /// <inheritdoc cref="Set(PropertyUri, ILinkedNode)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value);

        /// <inheritdoc cref="Set(PropertyUri, bool)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, bool value);

        /// <inheritdoc cref="Set{TValue}(PropertyUri, TValue)"/>
        /// <inheritdoc cref="Set{TProp}(IPropertyUriFormatter{TProp}, TProp, IndividualUri)"/>
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        /// <summary>
        /// Produces a subnode that is logically aggregated under the current node,
        /// from its name in <paramref name="subName"/>.
        /// </summary>
        /// <param name="subName">A name that is appended to the URI of this node.</param>
        /// <returns>A new node with the specific subname.</returns>
        ILinkedNode this[string? subName] { get; }

        /// <summary>
        /// Produces a subnode that is logically aggregated under the current node,
        /// from a formatter that transforms the current URI.
        /// </summary>
        /// <param name="subFormatter">The formatter that transforms the current URI.</param>
        /// <returns>A new node with the transformed URI.</returns>
        ILinkedNode? this[IIndividualUriFormatter<Uri> subFormatter] { get; }

        /// <summary>
        /// Informs the implementation that this resource is used as a base
        /// for a larger hierarchy, in which case it might use its URI
        /// as a base URI when producing RDF output.
        /// </summary>
        void SetAsBase();

        /// <summary>
        /// Asks the implementation whether this resource is matched by an
        /// externally configured query. This may be used in combination with
        /// <see cref="IHasFileOutput"/> and <see cref="OutputFileDelegate"/>.
        /// </summary>
        /// <returns>A sequence of objects produced by the queries that matched the resource.</returns>
        IEnumerable<INodeMatchProperties> Match();

        /// <summary>
        /// Returns a version of this node that writes output to a graph identified
        /// by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The graph to use for storing the description of the resource.</param>
        /// <returns>A new version of the node, or <see langword="null"/> if the graph is disabled.</returns>
        ILinkedNode? In(GraphUri graph);

        /// <summary>
        /// Returns a version of this node that writes output to a graph identified
        /// by <paramref name="graphFormatter"/> and <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TGraph">The type of <paramref name="value"/>.</typeparam>
        /// <param name="graphFormatter">The formatter to use for storing the description of the resource.</param>
        /// <param name="value">The value to format using <paramref name="graphFormatter"/>.</param>
        /// <returns>A new version of the node, or <see langword="null"/> if the graph is disabled.</returns>
        ILinkedNode? In<TGraph>(IGraphUriFormatter<TGraph> graphFormatter, TGraph value);
    }
}
