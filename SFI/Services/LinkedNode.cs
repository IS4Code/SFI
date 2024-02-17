using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Services
{
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
        /// Thrown when <paramref name="subject"/> is <see langword="null"/>.
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTriple(TNode? subj, TNode? pred, TNode? obj, bool inverse)
        {
            if(inverse)
            {
                HandleTriple(obj, pred, subj);
            }else{
                HandleTriple(subj, pred, obj);
            }
        }

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
        public abstract ValueTask DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void Describe(Func<Uri, XmlReader?> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract ValueTask DescribeAsync(Func<Uri, ValueTask<XmlReader?>> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void Describe(Func<Uri, XmlDocument?> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract ValueTask DescribeAsync(Func<Uri, ValueTask<XmlDocument?>> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract bool TryDescribe(object loader, Func<Uri, object?> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract ValueTask<bool> TryDescribeAsync(object loader, Func<Uri, ValueTask<object?>> dataSource, IReadOnlyCollection<Uri>? subjectUris = null);

        /// <inheritdoc/>
        public abstract void SetAsBase();

        /// <inheritdoc/>
        public abstract IEnumerable<INodeMatchProperties> Match();

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
            HandleTriple(Subject, Cache[property], Cache[value], property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value, DatatypeUri datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, Cache[datatype]), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set<TData>(PropertyUri property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, Cache[property], CreateNode(value, datatype), property.IsInverse);
            }
        }

        /// <inheritdoc/>
        public void Set<TValue>(PropertyUri property, TValue value, DatatypeUri datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set<TValue, TData>(PropertyUri property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var datatype = CreateNode(datatypeFormatter, datatypeValue);
            if(datatype != null)
            {
                HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), datatype), property.IsInverse);
            }
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, string value, LanguageCode language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], language.IsEmpty ? CreateNode(value) : CreateNode(value, language.Value), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set<T>(PropertyUri property, IIndividualUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(formatter, value), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(value is not LinkedNode<TNode, TGraphNode, TVocabularyCache> node) throw new ArgumentException($"The object must derive from {typeof(LinkedNode<TNode, TGraphNode, TVocabularyCache>)}.", nameof(value));
            HandleTriple(Subject, Cache[property], node.Subject, property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        /// <inheritdoc/>
        public void Set(PropertyUri property, bool value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(value), property.IsInverse);
        }

        /// <inheritdoc/>
        public void Set<TValue>(PropertyUri property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, Cache[property], CreateNode(value), property.IsInverse);
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
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), language.IsEmpty ? CreateNode(value) : CreateNode(value, language.Value));
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
            if(value is not LinkedNode<TNode, TGraphNode, TVocabularyCache> node) throw new ArgumentException($"The object must derive from {typeof(LinkedNode<TNode, TGraphNode, TVocabularyCache>)}.", nameof(value));
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

        /// <inheritdoc cref="ILinkedNode.this[string]"/>;
        public LinkedNode<TNode, TGraphNode, TVocabularyCache> this[string? subName] {
            get{
                var uri = GetUri(Subject);
                uri = UriTools.MakeSubUri(uri, subName ?? "");
                return CreateNew(CreateNode(uri));
            }
        }

        ILinkedNode ILinkedNode.this[string? subName] => this[subName];

        /// <inheritdoc cref="ILinkedNode.this[IIndividualUriFormatter{Uri}]"/>;
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
