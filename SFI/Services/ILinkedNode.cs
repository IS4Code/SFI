using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// The scheme of the URi identifying this node.
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// Describes the node using the RDF/XML description provided
        /// through <paramref name="rdfXmlReader"/>.
        /// </summary>
        /// <param name="rdfXmlReader">
        /// An XML reader for a valid RDF/XML document. The document
        /// shall describe the node by using a blank relative URI,
        /// i.e. &lt;rdf:Description rdf:about=""&gt;
        /// </param>
        /// <param name="subjectUris">
        /// Stores a collection of URIs that represent this node.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="rdfXmlReader"/> is not positioned on an
        /// {http://www.w3.org/1999/02/22-rdf-syntax-ns#}RDF element.
        /// </exception>
        void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc cref="Describe(XmlReader, IReadOnlyCollection{Uri})"/>
        Task DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Describes the node using the RDF/XML description provided
        /// in <paramref name="rdfXmlDocument"/>.
        /// </summary>
        /// <param name="rdfXmlDocument">
        /// A valid RDF/XML document. The document
        /// shall describe the node by using a blank relative URI,
        /// i.e. &lt;rdf:Description rdf:about=""&gt;
        /// </param>
        /// <param name="subjectUris">
        /// Stores a collection of URIs that represent this node.
        /// </param>
        void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <summary>
        /// Attempts to describe the node in an implementation-specific
        /// way, using a <paramref name="loader"/> which, if recognized,
        /// operates on the result of <paramref name="dataSource"/>.
        /// </summary>
        /// <param name="loader">An instance of an implementation-specific RDF loader class.</param>
        /// <param name="dataSource">
        /// A function that provides the argument to the loader.
        /// The parameter is a URI that should be used as the base.</param>
        /// <param name="subjectUris"><inheritdoc cref="Describe(XmlDocument, IReadOnlyCollection{Uri}?)" path="/param[@name='subjectUris']"/></param>
        /// <returns>
        /// True if <paramref name="loader"/> was correctly recognized
        /// and executed, false otherwise.
        /// </returns>
        bool TryDescribe(object loader, Func<Uri, object> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

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
        /// for a larger hierarchy, in which this case it might use its URI
        /// as a base URI when producing RDF output.
        /// </summary>
        void SetAsBase();

        /// <summary>
        /// Asks the implementation whether this resource is matched by an
        /// externally configured query. This may be used in combination with
        /// <see cref="IHasFileOutput"/> and <see cref="OutputFileDelegate"/>.
        /// </summary>
        /// <param name="properties">An object that provides custom match properties.</param>
        /// <returns>True if the resource is matched, false otherwise.</returns>
        bool Match([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out INodeMatchProperties properties);

        /// <summary>
        /// Returns a version of this node that writes output to a graph identified
        /// by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The graph to use for storing the description of the resource.</param>
        /// <returns>A new version of the node, or null if the graph is disabled.</returns>
        ILinkedNode? In(GraphUri graph);

        /// <summary>
        /// Returns a version of this node that writes output to a graph identified
        /// by <paramref name="graphFormatter"/> and <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TGraph">The type of <paramref name="value"/>.</typeparam>
        /// <param name="graphFormatter">The formatter to use for storing the description of the resource.</param>
        /// <param name="value">The value to format using <paramref name="graphFormatter"/>.</param>
        /// <returns>A new version of the node, or null if the graph is disabled.</returns>
        ILinkedNode? In<TGraph>(IGraphUriFormatter<TGraph> graphFormatter, TGraph value);
    }

    /// <summary>
    /// Provides a base implementation of <see cref="ILinkedNode"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of the underlying nodes.</typeparam>
    /// <typeparam name="TGraphNode">The type of the underlying graph.</typeparam>
    /// <typeparam name="TVocabularyCache">The type of the used vocabulary cache.</typeparam>
    public abstract class LinkedNode<TNode, TGraphNode, TVocabularyCache> :
        ILinkedNode, IEquatable<LinkedNode<TNode, TGraphNode, TVocabularyCache>>
        where TNode : class, IEquatable<TNode> where TGraphNode : class
        where TVocabularyCache : IVocabularyCache<PropertyUri, TNode>, IVocabularyCache<ClassUri, TNode>,
        IVocabularyCache<IndividualUri, TNode>, IVocabularyCache<DatatypeUri, TNode>,
        IVocabularyCache<GraphUri, TGraphNode?>
    {
        /// <summary>
        /// The subject node wrapped by this instance.
        /// </summary>
        protected TNode Subject { get; }

        /// <summary>
        /// The graph to use for describing the node, logically containing <see cref="Subject"/>.
        /// </summary>
        protected TGraphNode Graph { get; }
        
        /// <summary>
        /// The vocabulary cache to use when creating nodes from vocabulary items.
        /// </summary>
        protected TVocabularyCache Cache { get; }

        /// <summary>
        /// Creates a new instance of the linked node.
        /// </summary>
        /// <param name="subject">The value of <see cref="Subject"/>.</param>
        /// <param name="graph">The value of <see cref="Graph"/>.</param>
        /// <param name="cache">The value of <see cref="Cache"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subject"/> is null.
        /// </exception>
        public LinkedNode(TNode subject, TGraphNode graph, TVocabularyCache cache)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Graph = graph;
            Cache = cache;
        }

        /// <summary>
        /// Asserts a new triple in the current graph.
        /// </summary>
        /// <param name="subj">The subject node of the triple.</param>
        /// <param name="pred">The predicate node of the triple.</param>
        /// <param name="obj">The object node of the triple.</param>
        protected abstract void HandleTriple(TNode? subj, TNode? pred, TNode? obj);

        /// <summary>
        /// Creates a new URI node in the current graph.
        /// </summary>
        /// <param name="uri">The URI identifying the node.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        protected abstract TNode CreateNode(Uri uri);

        /// <summary>
        /// Creates a new literal node in the current graph.
        /// </summary>
        /// <param name="value">The value of the plain literal.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        protected abstract TNode CreateNode(string value);

        /// <summary>
        /// Creates a new literal node in the current graph.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        /// <param name="datatype">The node representing the datatype of the literal.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        protected abstract TNode CreateNode(string value, TNode datatype);

        /// <summary>
        /// Creates a new literal node in the current graph.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        /// <param name="language">The language of the literal.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        protected abstract TNode CreateNode(string value, string language);

        /// <summary>
        /// Creates a new literal node in the current graph.
        /// </summary>
        /// <param name="value">The boolean value of the literal.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        protected abstract TNode CreateNode(bool value);

        /// <summary>
        /// Creates a new literal node in the current graph.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="value">The value of the literal.</param>
        /// <returns>An instance of <typeparamref name="TNode"/> representing the node.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> doesn't have a recognized datatype.
        /// </exception>
        protected abstract TNode CreateNode<T>(T value) where T : struct, IEquatable<T>, IFormattable;

        /// <summary>
        /// Creates a new graph node from a URI.
        /// </summary>
        /// <param name="uri">The URI of the graph.</param>
        /// <returns>An instance of <typeparamref name="TGraphNode"/> for the graph.</returns>
        protected abstract TGraphNode? CreateGraphNode(Uri uri);

        /// <summary>
        /// Obtains the URI from an instance of <typeparamref name="TNode"/>.
        /// </summary>
        /// <param name="node">The node to obtain the URI from.</param>
        /// <returns>The URI of the node.</returns>
        protected abstract Uri GetUri(TNode node);

        /// <summary>
        /// Creates a new instance of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}"/>
        /// with a different subject.
        /// </summary>
        /// <param name="subject">The new subject to use.</param>
        /// <returns>A new instance of the node.</returns>
        protected abstract LinkedNode<TNode, TGraphNode, TVocabularyCache> CreateNew(TNode subject);

        /// <summary>
        /// Creates a new instance of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}"/>
        /// in a different graph.
        /// </summary>
        /// <param name="graph">The new graph to use.</param>
        /// <returns>A new instance of the node.</returns>
        protected abstract LinkedNode<TNode, TGraphNode, TVocabularyCache>? CreateInGraph(TGraphNode? graph);

        private TNode? CreateNode<T>(IUriFormatter<T> formatter, T value)
        {
            try{
                var uri = formatter[value];
                return uri != null ? CreateNode(uri) : null;
            }catch(UriFormatException) when(GlobalOptions.SuppressNonCriticalExceptions)
            {
                return null;
            }
        }

        private TGraphNode? CreateGraphNode<T>(IGraphUriFormatter<T> formatter, T value)
        {
            try{
                var uri = formatter[value];
                return uri != null ? CreateGraphNode(uri) : null;
            }catch(UriFormatException) when(GlobalOptions.SuppressNonCriticalExceptions)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public string Scheme => GetUri(Subject).Scheme;

        /// <inheritdoc/>
        public abstract void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract Task DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract bool TryDescribe(object loader, Func<Uri, object> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void SetAsBase();

        /// <inheritdoc/>
        public abstract bool Match(out INodeMatchProperties properties);

        /// <inheritdoc/>
        public void SetClass(ClassUri @class)
        {
            HandleTriple(Subject, Cache[Properties.Type], Cache[@class]);
        }

        /// <inheritdoc/>
        public void SetClass<T>(IClassUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[Properties.Type], CreateNode(formatter, value));
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, IndividualUri value)
        {
            HandleTriple(Subject, Cache[property], Cache[value]);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value, DatatypeUri datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, Cache[datatype]));
        }

        /// <inheritdoc/>
        public void Set<TData>(PropertyUri property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, Cache[property], CreateNode(value, datatype));
            }
        }

        /// <inheritdoc/>
        public void Set<TValue>(PropertyUri property, TValue value, DatatypeUri datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]));
        }

        /// <inheritdoc/>
        public void Set<TValue, TData>(PropertyUri property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), datatype));
            }
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value, LanguageCode language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language.Value == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, Cache[property], CreateNode(value, language.Value));
        }

        /// <inheritdoc/>
        public void Set<T>(PropertyUri property, IIndividualUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(formatter, value));
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(value is not LinkedNode<TNode, TGraphNode, TVocabularyCache> node) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, Cache[property], node.Subject);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, bool value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        /// <inheritdoc/>
        public void Set<TValue>(PropertyUri property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IndividualUri value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), Cache[value]);
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, DatatypeUri datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, Cache[datatype]));
        }

        /// <inheritdoc/>
        public void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, datatype));
            }
        }

        /// <inheritdoc/>
        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, DatatypeUri datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]));
        }

        /// <inheritdoc/>
        public void Set<TProp, TValue, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value.ToString(null, CultureInfo.InvariantCulture), datatype));
            }
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, LanguageCode language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language.Value == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, language.Value));
        }

        /// <inheritdoc/>
        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(formatter, value));
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(value is not LinkedNode<TNode, TGraphNode, TVocabularyCache> node) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), node.Subject);
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value)
        {
            Set(propertyFormatter, propertyValue, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        /// <inheritdoc/>
        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, bool value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        /// <inheritdoc/>
        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }
        
        /// <summary>
        /// Produces a subnode that is logically aggregated under the current node,
        /// from its name in <paramref name="subName"/>.
        /// </summary>
        /// <param name="subName">A name that is appended to the URI of this node.</param>
        /// <returns>A new node with the specific subname.</returns>
        public LinkedNode<TNode, TGraphNode, TVocabularyCache> this[string? subName] {
            get{
                subName ??= "";
                var uri = GetUri(Subject);
                string prefix;
                if(subName.StartsWith("#"))
                {
                    prefix = "";
                }else if(String.IsNullOrEmpty(uri.Fragment) && (String.IsNullOrEmpty(uri.Authority) || !String.IsNullOrEmpty(uri.Query)))
                {
                    prefix = subName.StartsWith("/") ? "#" : "#/";
                }else{
                    prefix = subName.StartsWith("/") ? "" : "/";
                }
                uri = new EncodedUri(uri.AbsoluteUri + prefix + subName);
                return CreateNew(CreateNode(uri));
            }
        }

        ILinkedNode ILinkedNode.this[string? subName] => this[subName];

        /// <summary>
        /// Produces a subnode that is logically aggregated under the current node,
        /// from a formatter that transforms the current URI.
        /// </summary>
        /// <param name="subFormatter">The formatter that transforms the current URI.</param>
        /// <returns>A new node with the transformed URI.</returns>
        public LinkedNode<TNode, TGraphNode, TVocabularyCache>? this[IIndividualUriFormatter<Uri> subFormatter] {
            get{
                if(subFormatter == null) throw new ArgumentNullException(nameof(subFormatter));
                var node = CreateNode(subFormatter, GetUri(Subject));
                return node != null ? CreateNew(node) : null;
            }
        }

        ILinkedNode? ILinkedNode.this[IIndividualUriFormatter<Uri> subFormatter] => this[subFormatter];

        /// <inheritdoc/>
        public ILinkedNode? In(GraphUri graph)
        {
            return CreateInGraph(Cache[graph]);
        }

        /// <inheritdoc/>
        public ILinkedNode? In<TGraph>(IGraphUriFormatter<TGraph> graphFormatter, TGraph value)
        {
            if(graphFormatter == null) throw new ArgumentNullException(nameof(graphFormatter));
            var node = CreateGraphNode(graphFormatter, value);
            return CreateInGraph(node);
        }

        /// <inheritdoc/>
        public bool Equals(LinkedNode<TNode, TGraphNode, TVocabularyCache> node)
        {
            return Subject.Equals(node.Subject) && Object.Equals(Graph, node.Graph);
        }

        /// <inheritdoc/>
        public bool Equals(ILinkedNode other)
        {
            return other is LinkedNode<TNode, TGraphNode, TVocabularyCache> node && Equals(node);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is LinkedNode<TNode, TGraphNode, TVocabularyCache> node && Equals(node);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Subject.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Subject.ToString();
        }
    }
}
