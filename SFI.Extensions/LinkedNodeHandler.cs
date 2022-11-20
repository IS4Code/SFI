using IS4.SFI.Services;
using IS4.SFI.Tools.Xml;
using IS4.SFI.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;

namespace IS4.SFI.Extensions
{
    /// <summary>
    /// Provides an implementation of <see cref="ILinkedNodeFactory"/>
    /// based on instances of <see cref="IUriNode"/>.
    /// </summary>
    public class LinkedNodeHandler : VocabularyCache<IUriNode>, ILinkedNodeFactory
    {
        readonly IRdfHandler defaultHandler;
        readonly IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers;

        readonly ConcurrentDictionary<GraphUri, IRdfHandler?> graphUriCache = new();
        readonly ConcurrentDictionary<IRdfHandler, VocabularyCache<IUriNode>> graphCaches = new();

        readonly NodeQueryTester? queryTester;

        /// <inheritdoc/>
        public IIndividualUriFormatter<string> Root { get; }

        /// <summary>
        /// The fake URI scheme used for URIs denoting blank nodes.
        /// URIs using this prefix are specially recognized,
        /// creating blank nodes instead of URI nodes in RDF.
        /// </summary>
        public const string BlankUriScheme = "x.blank";

        /// <summary>
        /// A map storing prefixes of used namespaces.
        /// </summary>
        public IDictionary<Uri, string> PrefixMap { get; }

        /// <summary>
        /// The maximum allowed length of generated URIs; longer
        /// URIs will be shortened using <see cref="UriTools.UriToUuidUri(Uri)"/>.
        /// </summary>
        public int MaxUriLength { get; set; } = 1900 - 20; // limit for OpenLink Virtuoso

        /// <summary>
        /// The maximum length of individual URI parts when a string representation of
        /// a shortened URI is stored, for <see cref="UriTools.ShortenUri(Uri, int, string)"/>.
        /// </summary>
        public int UriPartShortened { get; set; } = 64;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="root">The root of the node hierarchy.</param>
        /// <param name="defaultHandler">The default RDF handler to write triples to.</param>
        /// <param name="graphHandlers">The additional handlers for each custom graph.</param>
        /// <param name="queryTester">The instance of <see cref="NodeQueryTester"/> that is used to match nodes.</param>
        public LinkedNodeHandler(IIndividualUriFormatter<string> root, IRdfHandler defaultHandler, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, NodeQueryTester? queryTester)
            : base(defaultHandler.CreateUriNode)
        {
            this.defaultHandler = defaultHandler;
            this.graphHandlers = graphHandlers;
            this.queryTester = queryTester;

            graphCaches[defaultHandler] = this;

            Root = root;

            PrefixMap = new ConcurrentDictionary<Uri, string>(Vocabulary.Vocabularies.Prefixes, new UriComparer());

            VocabularyAdded += (vocabulary) => {
                // When a new vocabulary is added, register it in all RDF handlers
                var uri = new Uri(vocabulary.Value, UriKind.Absolute);
                var prefix = GetNamespace(uri);
                AddNamespace(defaultHandler, prefix, uri);
                foreach(var handler in graphHandlers.Values)
                {
                    if(handler != null)
                    {
                        AddNamespace(handler, prefix, uri);
                    }
                }
            };
        }

        int namespaceCounter;

        /// <summary>
        /// Retrieves the prefix of an added namespace, or creates a new one.
        /// </summary>
        private string GetNamespace(Uri uri)
        {
            return PrefixMap.TryGetValue(uri, out var prefix) ?
                prefix : $"ns{System.Threading.Interlocked.Increment(ref namespaceCounter)}";
        }

        /// <summary>
        /// Handles a new namespace.
        /// </summary>
        private void AddNamespace(IRdfHandler handler, string prefix, Uri uri)
        {
            lock(handler)
            {
                handler.HandleNamespace(prefix, uri);
            }
        }

        /// <summary>
        /// A map of "real" URIs of <see cref="INode"/> instances, in cases
        /// the URI was shortened or denoted a blank node.
        /// </summary>
        static readonly ConditionalWeakTable<INode, Uri> realUriCache = new();

        /// <inheritdoc/>
        public ILinkedNode Create<T>(IIndividualUriFormatter<T> formatter, T value)
        {
            Uri? uri;
            try{
                uri = formatter[value];
            }catch(UriFormatException) when(GlobalOptions.SuppressNonCriticalExceptions)
            {
                uri = null;
            }
            if(uri == null)
            {
                uri = new Uri($"{BlankUriScheme}:{Guid.NewGuid():D}", UriKind.Absolute);
            }
            var node = CreateNode(uri, defaultHandler);
            if(node == null)
            {
                throw new NotSupportedException();
            }
            return new UriNode(node, defaultHandler, GetGraphCache(defaultHandler), queryTester);
        }

        /// <summary>
        /// Creates a new RDF node for the given URI using the specified handler
        /// as the node factory.
        /// </summary>
        private INode CreateNode(Uri uri, IRdfHandler handler)
        {
            if(uri == null)
            {
                return null!;
            }
            if(uri.Scheme == BlankUriScheme)
            {
                return CreateBlankNode(uri, handler);
            }
            if(uri.OriginalString.Length > MaxUriLength)
            {
                // Shorten the URI for displaying
                var shortUriLabel = UriTools.ShortenUri(uri, UriPartShortened, "\u00A0(URI\u00A0too\u00A0long)");

                var newUri = UriTools.UriToUuidUri(uri);
                var subject = handler.CreateUriNode(newUri);
                realUriCache.Add(subject, uri);
                var node = new UriNode(subject, handler, GetGraphCache(handler), queryTester);
                node.Set(Properties.AtPrefLabel, shortUriLabel.ToString(), Datatypes.AnyUri);
                var linked = node.In(Graphs.ShortenedLinks);
                if(linked != null)
                {
                    // Link the individuals in the ShortenedLinks graph
                    linked.Set(Properties.SameAs, UriFormatter.Instance, uri);
                }
                return subject;
            }
            return defaultHandler.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a blank node identified by a URI.
        /// </summary>
        private IBlankNode CreateBlankNode(Uri blankUri, IRdfHandler handler)
        {
            var identifier = $"b{UriTools.UuidFromUri(blankUri):N}";
            var bnode = handler.CreateBlankNode(identifier);
            realUriCache.Add(bnode, blankUri);
            return bnode;
        }

        bool ILinkedNodeFactory.IsSafeString(string str)
        {
            return DataTools.IsSafeString(str);
        }
        
        /// <summary>
        /// Returns the <see cref="VocabularyCache{TNode}"/> for a given RDF handler.
        /// </summary>
        Cache GetGraphCache(IRdfHandler handler)
        {
            var cache = graphCaches.GetOrAdd(handler, h => new VocabularyCache<IUriNode>(h.CreateUriNode));
            return new Cache(this, cache);
        }

        /// <summary>
        /// Returns the RDF handler for a given named graph, or the default handler
        /// if the named graph is not defined.
        /// </summary>
        IRdfHandler? GetGraphHandler(GraphUri name)
        {
            if(graphUriCache.TryGetValue(name, out var handler))
            {
                return handler;
            }
            var uri = new Uri(name.Value, UriKind.Absolute);
            return graphUriCache[name] = GetGraphHandler(uri);
        }

        /// <summary>
        /// Returns the RDF handler for a given named graph, or the default handler
        /// if the named graph is not defined.
        /// </summary>
        IRdfHandler? GetGraphHandler(Uri uri)
        {
            if(graphHandlers.TryGetValue(uri, out var handler))
            {
                return handler;
            }
            return defaultHandler;
        }

        /// <summary>
        /// Implements <see cref="IVocabularyCache{TTerm, TNode}"/> for all RDF terms,
        /// based on a shared instance of <see cref="VocabularyCache{TNode}"/>.
        /// </summary>
        struct Cache :
            IVocabularyCache<ClassUri, IUriNode>, IVocabularyCache<PropertyUri, IUriNode>,
            IVocabularyCache<IndividualUri, IUriNode>, IVocabularyCache<DatatypeUri, IUriNode>,
            IVocabularyCache<GraphUri, IRdfHandler?>
        {
            public LinkedNodeHandler Parent { get; }
            public VocabularyCache<IUriNode> Inner { get; }

            public Cache(LinkedNodeHandler handler, VocabularyCache<IUriNode> cache)
            {
                Parent = handler;
                Inner = cache;
            }

            public IUriNode this[ClassUri name] => Inner[name];
            public IUriNode this[PropertyUri name] => Inner[name];
            public IUriNode this[IndividualUri name] => Inner[name];
            public IUriNode this[DatatypeUri name] => Inner[name];
            public IRdfHandler? this[GraphUri name] => Parent.GetGraphHandler(name);
            public ICollection<VocabularyUri> Vocabularies => Inner.Vocabularies;
        }

        /// <summary>
        /// An implementation of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}"/>
        /// for <see cref="INode"/>.
        /// </summary>
        class UriNode : LinkedNode<INode, IRdfHandler, Cache>
        {
            readonly NodeQueryTester? queryTester;

            /// <param name="queryTester">The instance of <see cref="NodeQueryTester"/> for <see cref="Match(out INodeMatchProperties)"/>.</param>
            /// <inheritdoc cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/>
            /// <param name="subject"><inheritdoc path="/param[@name='subject']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            /// <param name="graph"><inheritdoc path="/param[@name='graph']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            /// <param name="cache"><inheritdoc path="/param[@name='cache']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            public UriNode(INode subject, IRdfHandler graph, Cache cache, NodeQueryTester? queryTester) : base(subject, graph, cache)
            {
                if(!(subject is IUriNode || subject is IBlankNode)) throw new ArgumentException(null, nameof(subject));
                this.queryTester = queryTester;
            }

            protected override void HandleTriple(INode? subj, INode? pred, INode? obj)
            {
                if(subj == null || pred == null || obj == null)
                {
                    return;
                }
                var date = DateTime.UtcNow;
                lock(Graph)
                {
                    Graph.HandleTriple(new Triple(subj, pred, obj));
                }
                var meta = In(Graphs.Metadata);
                if(meta != null && !Equals(meta))
                {
                    // Add at:visited to the metadata graph (if this is not the metadata graph)
                    var dateString = XmlConvert.ToString(date, "yyyy-MM-dd\\THH:mm:ssK");
                    meta.Set(Properties.Visited, dateString, Datatypes.DateTime);
                }
            }

            /// <summary>
            /// Called when a triple from an external graph describing the current node
            /// is encountered.
            /// </summary>
            private bool HandleExternalTriple(INode subj, INode pred, INode obj)
            {
                if(Subject.Equals(subj) && !(obj is IBlankNode))
                {
                    // The triple is accepted only if it fully described the current node
                    HandleTriple(subj, pred, obj);
                    return true;
                }
                return false;
            }

            public override void SetAsBase()
            {
                if(Subject is IUriNode subject)
                {
                    lock(Graph)
                    {
                        Graph.HandleBaseUri(subject.Uri);
                    }
                }
            }

            public override bool Match(out INodeMatchProperties properties)
            {
                if(queryTester == null)
                {
                    properties = null!;
                    return false;
                }
                return queryTester.Match(Subject, out properties);
            }

            protected override INode CreateNode(Uri uri)
            {
                return Cache.Parent.CreateNode(uri, Graph);
            }

            protected override INode CreateNode(string value)
            {
                if(DataTools.IsSafeString(value)) return Graph.CreateLiteralNode(value);
                return Graph.CreateLiteralNode(DataTools.CreateLiteralJsonLd(value), GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(string value, INode datatype)
            {
                var dt = GetUri(datatype);
                if(DataTools.IsSafeString(value)) return Graph.CreateLiteralNode(value, dt);
                return Graph.CreateLiteralNode(DataTools.CreateLiteralJsonLd(value, dt), GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(string value, string language)
            {
                if(DataTools.IsSafeString(value)) return Graph.CreateLiteralNode(value, language);
                return Graph.CreateLiteralNode(DataTools.CreateLiteralJsonLd(value, language), GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(bool value)
            {
                return LiteralExtensions.ToLiteral(value, Graph);
            }

            protected override INode CreateNode<T>(T value)
            {
                try{
                    return LiteralExtensions.ToLiteral((dynamic)value, Graph);
                }catch(RuntimeBinderException e)
                {
                    throw new ArgumentException(null, nameof(value), e);
                }
            }

            protected override Uri GetUri(INode node)
            {
                return realUriCache.TryGetValue(node, out var uri) ? uri : ((IUriNode)node).Uri;
            }

            protected override LinkedNode<INode, IRdfHandler, Cache> CreateNew(INode subject)
            {
                return new UriNode(subject, Graph, Cache, queryTester);
            }

            protected override LinkedNode<INode, IRdfHandler, Cache>? CreateInGraph(IRdfHandler? graph)
            {
                if(graph == null)
                {
                    return null;
                }
                return new UriNode(VDS.RDF.Tools.CopyNode(Subject, graph), graph, Cache.Parent.GetGraphCache(graph), queryTester);
            }

            protected override IRdfHandler? CreateGraphNode(Uri uri)
            {
                return Cache.Parent.GetGraphHandler(uri);
            }

            /// <summary>
            /// Ensures that the argument stores an rdf:RDF element, and returns a document
            /// with the base set to the current URI.
            /// </summary>
            private XmlDocument PrepareXmlDocument(XmlReader rdfXmlReader)
            {
                if(rdfXmlReader.NodeType != XmlNodeType.Element || rdfXmlReader.NamespaceURI != "http://www.w3.org/1999/02/22-rdf-syntax-ns#" || rdfXmlReader.LocalName != "RDF")
                {
                    throw new ArgumentException("The XML reader must be positioned on a rdf:RDF element.", nameof(rdfXmlReader));
                }
                return new BaseXmlDocument(GetUri(Subject).AbsoluteUri, rdfXmlReader.NameTable);
            }

            public override void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var doc = PrepareXmlDocument(rdfXmlReader);
                doc.Load(rdfXmlReader);
                Describe(doc, subjectUris);
            }

            public async override Task DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var doc = PrepareXmlDocument(rdfXmlReader);
                await doc.LoadAsync(rdfXmlReader);
                Describe(doc, subjectUris);
            }

            public override void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                if(rdfXmlDocument is BaseXmlDocument baseXmlDocument && baseXmlDocument.BaseURI == null)
                {
                    baseXmlDocument.SetBaseURI(GetUri(Subject).AbsoluteUri);
                }
                var graph = new DataGraph(this, subjectUris);
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                parser.Load(graph, rdfXmlDocument);
            }

            /// <summary>
            /// The graph holding the external description, used by <see cref="Describe(XmlDocument, IReadOnlyCollection{Uri}?)"/>.
            /// </summary>
            class DataGraph : Graph
            {
                readonly UriNode describingNode;
                readonly IReadOnlyCollection<Uri> subjectUris;

                static readonly UriComparer comparer = new();

                public DataGraph(UriNode describingNode, IReadOnlyCollection<Uri>? subjectUris) : base(true)
                {
                    this.describingNode = describingNode;
                    this.subjectUris = subjectUris ?? Array.Empty<Uri>();
                }

                public override bool Assert(Triple t)
                {
                    return describingNode.HandleExternalTriple(ReplaceNode(t.Subject), t.Predicate, ReplaceNode(t.Object));
                }

                private INode ReplaceNode(INode node)
                {
                    if(node is IUriNode uriNode && subjectUris.Any(uri => comparer.Equals(uriNode.Uri, uri)))
                    {
                        return describingNode.Subject;
                    }
                    return node;
                }
            }
        }
    }
}
