﻿using IS4.SFI.Services;
using IS4.SFI.Tools.Xml;
using IS4.SFI.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace IS4.SFI.RDF
{
    /// <summary>
    /// Provides an implementation of <see cref="ILinkedNodeFactory"/>
    /// based on instances of <see cref="IUriNode"/>.
    /// </summary>
    public class LinkedNodeHandler : VocabularyCache<IUriNode>, ILinkedNodeFactory
    {
        readonly IRdfHandler defaultHandler;
        readonly IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers;
        readonly ConcurrentDictionary<Uri, IBlankNode>? blankNodes;

        int blankNodeId;

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
        /// Whether to emit additional identifying triples to help identify container membership properties.
        /// </summary>
        public bool DescribeMembershipProperties { get; set; } = true;

        SparseBitTable memberMetadataEmitted;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="root">The root of the node hierarchy.</param>
        /// <param name="defaultHandler">The default RDF handler to write triples to.</param>
        /// <param name="graphHandlers">The additional handlers for each custom graph.</param>
        /// <param name="queryTester">The instance of <see cref="NodeQueryTester"/> that is used to match nodes.</param>
        /// <param name="simplifyBlankNodes">Whether to produce shorter blank node IDs.</param>
        public LinkedNodeHandler(IIndividualUriFormatter<string> root, IRdfHandler defaultHandler, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, NodeQueryTester? queryTester, bool simplifyBlankNodes)
            : base(defaultHandler.CreateUriNode)
        {
            this.defaultHandler = defaultHandler;
            this.graphHandlers = graphHandlers;
            this.queryTester = queryTester;

            var uriComparer = new UriComparer();

            if(simplifyBlankNodes)
            {
                blankNodes = new(uriComparer);
            }

            graphCaches[defaultHandler] = this;

            Root = root;

            PrefixMap = new ConcurrentDictionary<Uri, string>(Vocabulary.Vocabularies.Prefixes, uriComparer);

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
            return new UriNode(node, defaultHandler, GetGraphCache(defaultHandler));
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
                var node = new UriNode(subject, handler, GetGraphCache(handler));
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
            IBlankNode bnode;
            if(blankNodes != null)
            {
                bnode = blankNodes.GetOrAdd(blankUri, _ => handler.CreateBlankNode("n" + Interlocked.Increment(ref blankNodeId)));
            }else{
                var uuid = UriTools.UuidFromUri(blankUri);
                bnode = handler.CreateBlankNode($"b{uuid:N}");
            }
            // May be already added
            realUriCache.GetValue(bnode, _ => blankUri);
            return bnode;
        }

        bool ILinkedNodeFactory.IsSafeLiteral(string str)
        {
            return DataTools.IsSafeString(str);
        }

        bool ILinkedNodeFactory.IsSafePredicate(Uri uri)
        {
            return IsSafePredicate(uri);
        }

        static readonly VDS.RDF.Writing.Formatting.RdfXmlFormatter rdfXmlFormatter = new();

        IBlankNode? testBlankNode;

        bool IsSafePredicate(Uri uri)
        {
            var blank = testBlankNode;
            if(blank == null)
            {
                lock(defaultHandler)
                {
                    if(testBlankNode == null)
                    {
                        testBlankNode = defaultHandler.CreateBlankNode();
                    }
                    blank = testBlankNode;
                }
            }
            var triple = new Triple(blank, defaultHandler.CreateUriNode(uri), blank);
            try{
                rdfXmlFormatter.Format(triple);
                return true;
            }catch(VDS.RDF.Writing.RdfOutputException)
            {
                return false;
            }
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
        readonly struct Cache :
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
            LinkedNodeHandler handler => Cache.Parent;

            /// <inheritdoc cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/>
            /// <param name="subject"><inheritdoc path="/param[@name='subject']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            /// <param name="graph"><inheritdoc path="/param[@name='graph']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            /// <param name="cache"><inheritdoc path="/param[@name='cache']" cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.LinkedNode(TNode, TGraphNode, TVocabularyCache)"/></param>
            public UriNode(INode subject, IRdfHandler graph, Cache cache) : base(subject, graph, cache)
            {
                if(!(subject is IUriNode || subject is IBlankNode)) throw new ArgumentException($"The object must derive from {typeof(IUriNode)} or {typeof(IBlankNode)}.", nameof(subject));
            }

            protected override void HandleTriple(INode? subj, INode? pred, INode? obj)
            {
                if(subj == null || pred == null || obj == null)
                {
                    return;
                }
                var date = DateTime.UtcNow;
                var oldPred = pred;
                while(pred is IUriNode uriPred && !handler.IsSafePredicate(uriPred.Uri))
                {
                    // Create v5 UUID URI to encode the unsafe predicate URI
                    pred = Graph.CreateUriNode(UriTools.UriToUuidUri(uriPred.Uri));
                }
                lock(Graph)
                {
                    Graph.HandleTriple(new Triple(subj, pred, obj));
                }
                if(pred != oldPred)
                {
                    var predNode = CreateNew(pred);
                    var oldPredNode = CreateNew(oldPred);
                    predNode.Set(Properties.SameAs, oldPredNode);
                    predNode.Set(Properties.EquivalentProperty, oldPredNode);
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
                if(Subject.Equals(subj) && obj is not IBlankNode)
                {
                    // The triple is accepted only if it fully described the current node
                    HandleTriple(Subject, pred, obj);
                    return true;
                }
                return false;
            }

            /// <inheritdoc/>
            protected override INode? CreateNode<T>(IUriFormatter<T> formatter, T value)
            {
                var node = base.CreateNode(formatter, value);
                if(handler.DescribeMembershipProperties && formatter == Properties.MemberAt)
                {
                    int index = (int)(object)value!;
                    if(handler.memberMetadataEmitted.TryAdd(index))
                    {
                        // Add metadata to help querying the membership property
                        HandleTriple(node, Cache[Properties.Type], Cache[Classes.ContainerMembershipProperty]);
                        HandleTriple(node, Cache[Properties.SubPropertyOf], Cache[Properties.Member]);
                    }
                }
                return node;
            }

            public override void SetAsBase()
            {
                switch(Subject)
                {
                    case IUriNode uriSubject:
                        lock(Graph)
                        {
                            Graph.HandleBaseUri(uriSubject.Uri);
                        }
                        break;
                    case IBlankNode bnodeSubject:
                        if(realUriCache.TryGetValue(bnodeSubject, out var uri) && uri.Scheme != BlankUriScheme)
                        {
                            lock(Graph)
                            {
                                Graph.HandleBaseUri(uri);
                            }
                        }
                        break;
                }
            }

            public override IEnumerable<INodeMatchProperties> Match()
            {
                var tester = handler.queryTester;
                if(tester == null)
                {
                    return Array.Empty<INodeMatchProperties>();
                }
                return tester.Match(Subject).Distinct(NodeMatchPropertiesComparer.Instance).ToArray();
            }

            protected override INode CreateNode(Uri uri)
            {
                return handler.CreateNode(uri, Graph);
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
                if(DataTools.IsSafeString(value) && IsSafeLanguage(language)) return Graph.CreateLiteralNode(value, language);
                return Graph.CreateLiteralNode(DataTools.CreateLiteralJsonLd(value, language), GetUri(Cache[Datatypes.Json]));
            }

            private static bool IsSafeLanguage(string language)
            {
                if(language.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
                {
                    // Fix privateuse appearing at start
                    return LanguageTag.IsWellFormed("en-" + language);
                }
                return LanguageTag.IsWellFormed(language);
            }

            protected override INode CreateNode(bool value)
            {
                return LiteralExtensions.ToLiteral(value, Graph);
            }

            [DynamicDependency(nameof(LiteralExtensions.ToLiteral), typeof(LiteralExtensions))]
            protected override INode CreateNode<T>(T value)
            {
                try{
                    return LiteralExtensions.ToLiteral((dynamic)value, Graph);
                }catch(RuntimeBinderException e)
                {
                    throw new ArgumentException("The object could not be converted to a literal node.", nameof(value), e);
                }
            }

            protected override Uri GetUri(INode node)
            {
                return realUriCache.TryGetValue(node, out var uri) ? uri : ((IUriNode)node).Uri;
            }

            protected override LinkedNode<INode, IRdfHandler, Cache> CreateNew(INode subject)
            {
                return new UriNode(subject, Graph, Cache);
            }

            protected override LinkedNode<INode, IRdfHandler, Cache>? CreateInGraph(IRdfHandler? graph)
            {
                if(graph == null)
                {
                    return null;
                }
                return new UriNode(Subject, graph, handler.GetGraphCache(graph));
            }

            protected override IRdfHandler? CreateGraphNode(Uri uri)
            {
                return handler.GetGraphHandler(uri);
            }

            /// <summary>
            /// Ensures that the argument stores an <c>rdf:RDF</c> element, and returns an
            /// instance of <see cref="BaseXmlDocument"/>.
            /// </summary>
            private static BaseXmlDocument PrepareXmlDocument(XmlReader rdfXmlReader)
            {
                if(rdfXmlReader.NodeType != XmlNodeType.Element || rdfXmlReader.NamespaceURI != "http://www.w3.org/1999/02/22-rdf-syntax-ns#" || rdfXmlReader.LocalName != "RDF")
                {
                    throw new ArgumentException("The XML reader must be positioned on a rdf:RDF element.", nameof(rdfXmlReader));
                }
                return new BaseXmlDocument(rdfXmlReader.NameTable);
            }

            public override void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!TryDescribe(parser, uri => {
                    var doc = PrepareXmlDocument(rdfXmlReader);
                    doc.SetBaseURI(uri.AbsoluteUri);
                    doc.Load(rdfXmlReader);
                    return doc;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public async override ValueTask DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!await TryDescribeAsync(parser, async uri => {
                    var doc = PrepareXmlDocument(rdfXmlReader);
                    doc.SetBaseURI(uri.AbsoluteUri);
                    await doc.LoadAsync(rdfXmlReader);
                    return doc;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public override void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!TryDescribe(parser, uri => {
                    if(rdfXmlDocument is BaseXmlDocument baseXmlDocument)
                    {
                        baseXmlDocument.SetBaseURI(uri.AbsoluteUri);
                    }
                    return rdfXmlDocument;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public override void Describe(Func<Uri, XmlReader?> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!TryDescribe(parser, uri => {
                    var rdfXmlReader = rdfXmlReaderFactory(uri);
                    if(rdfXmlReader == null) return null;
                    var doc = PrepareXmlDocument(rdfXmlReader);
                    doc.SetBaseURI(uri.AbsoluteUri);
                    doc.Load(rdfXmlReader);
                    return doc;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public override async ValueTask DescribeAsync(Func<Uri, ValueTask<XmlReader?>> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!await TryDescribeAsync(parser, async uri => {
                    var rdfXmlReader = await rdfXmlReaderFactory(uri);
                    if(rdfXmlReader == null) return null;
                    var doc = PrepareXmlDocument(rdfXmlReader);
                    doc.SetBaseURI(uri.AbsoluteUri);
                    await doc.LoadAsync(rdfXmlReader);
                    return doc;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public override void Describe(Func<Uri, XmlDocument?> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!TryDescribe(parser, uri => {
                    var rdfXmlDocument = rdfXmlDocumentFactory(uri);
                    if(rdfXmlDocument == null) return null;
                    if(rdfXmlDocument is BaseXmlDocument baseXmlDocument)
                    {
                        baseXmlDocument.SetBaseURI(uri.AbsoluteUri);
                    }
                    return rdfXmlDocument;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public async override ValueTask DescribeAsync(Func<Uri, ValueTask<XmlDocument?>> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                if(!await TryDescribeAsync(parser, async uri => {
                    var rdfXmlDocument = await rdfXmlDocumentFactory(uri);
                    if(rdfXmlDocument == null) return null;
                    if(rdfXmlDocument is BaseXmlDocument baseXmlDocument)
                    {
                        baseXmlDocument.SetBaseURI(uri.AbsoluteUri);
                    }
                    return rdfXmlDocument;
                }, subjectUris))
                {
                    throw new NotSupportedException();
                }
            }

            public override bool TryDescribe(object loader, Func<Uri, object?> dataSource, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                if(loader is IRdfReader reader)
                {
                    var graph = new DataGraph(this, subjectUris);
                    var data = dataSource(graph.BaseUri);
                    if(data == null) return false;
                    return LoadData(graph, reader, data);
                }
                return false;
            }

            public async override ValueTask<bool> TryDescribeAsync(object loader, Func<Uri, ValueTask<object?>> dataSource, IReadOnlyCollection<Uri>? subjectUris = null)
            {
                if(loader is IRdfReader reader)
                {
                    var graph = new DataGraph(this, subjectUris);
                    var data = await dataSource(graph.BaseUri);
                    if(data == null) return false;
                    return LoadData(graph, reader, data);
                }
                return false;
            }

            private bool LoadData(DataGraph graph, IRdfReader reader, object data)
            {
                switch(data)
                {
                    case string filename:
                        reader.Load(graph, filename);
                        return true;
                    case StreamReader inputStream:
                        reader.Load(graph, inputStream);
                        return true;
                    case TextReader inputText:
                        reader.Load(graph, inputText);
                        return true;
                    case XmlDocument xmlDocument when reader is VDS.RDF.Parsing.RdfXmlParser xmlParser:
                        xmlParser.Load(graph, xmlDocument);
                        return true;
                }
                return false;
            }

            /// <summary>
            /// The graph holding the external description, used by <see cref="Describe(XmlDocument, IReadOnlyCollection{Uri}?)"/>
            /// and <see cref="TryDescribe(object, Func{Uri, object}, IReadOnlyCollection{Uri}?)"/>.
            /// </summary>
            class DataGraph : Graph
            {
                readonly UriNode describingNode;
                readonly IReadOnlyCollection<Uri> subjectUris;

                static readonly UriComparer comparer = new();

                public DataGraph(UriNode describingNode, IReadOnlyCollection<Uri>? subjectUris)
                    : base(
                          null,
                          nodeFactory: new FixedBaseNodeFactory(new Uri($"x.{Guid.NewGuid():N}:", UriKind.Absolute)),
                          emptyNamespaceMap: true
                          )
                {
                    this.describingNode = describingNode;
                    this.subjectUris = subjectUris ?? Array.Empty<Uri>();
                }

                public override bool Assert(Triple t)
                {
                    var subj = ReplaceNode(t.Subject);
                    var obj = ReplaceNode(t.Object);
                    if(subj == null || obj == null) return false;
                    return describingNode.HandleExternalTriple(subj, t.Predicate, obj);
                }

                private INode? ReplaceNode(INode node)
                {
                    if(node is IUriNode uriNode)
                    {
                        var nodeUri = uriNode.Uri;
                        if(subjectUris.Any(uri => comparer.Equals(nodeUri, uri)))
                        {
                            return describingNode.Subject;
                        }
                        if(nodeUri.IsAbsoluteUri && nodeUri.Scheme == BaseUri.Scheme)
                        {
                            var idUri = BaseUri.MakeRelativeUri(nodeUri);
                            if(!idUri.IsAbsoluteUri)
                            {
                                var id = idUri.OriginalString;
                                if(id == "")
                                {
                                    return describingNode.Subject;
                                }
                                return (describingNode[id] as UriNode)?.Subject;
                            }
                        }
                    }
                    return node;
                }

                class FixedBaseNodeFactory : INodeFactory
                {
                    readonly INodeFactory inner;

                    public FixedBaseNodeFactory(Uri baseUri)
                    {
                        inner = new NodeFactory(new NodeFactoryOptions
                        {
                            BaseUri = baseUri,
                            ValidateLanguageSpecifiers = false
                        });
                    }

                    public Uri BaseUri {
                        get => inner.BaseUri;
                        set {

                        }
                    }

                    #region INodeFactory implementation
                    public INamespaceMapper NamespaceMap => inner.NamespaceMap;

                    public IUriFactory UriFactory { get => inner.UriFactory; set => inner.UriFactory = value; }
                    public bool NormalizeLiteralValues { get => inner.NormalizeLiteralValues; set => inner.NormalizeLiteralValues = value; }

                    public IBlankNode CreateBlankNode()
                    {
                        return inner.CreateBlankNode();
                    }

                    public IBlankNode CreateBlankNode(string nodeId)
                    {
                        return inner.CreateBlankNode(nodeId);
                    }

                    public IGraphLiteralNode CreateGraphLiteralNode()
                    {
                        return inner.CreateGraphLiteralNode();
                    }

                    public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
                    {
                        return inner.CreateGraphLiteralNode(subgraph);
                    }

                    public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
                    {
                        return inner.CreateLiteralNode(literal, datatype);
                    }

                    public ILiteralNode CreateLiteralNode(string literal)
                    {
                        return inner.CreateLiteralNode(literal);
                    }

                    public ILiteralNode CreateLiteralNode(string literal, string langSpec)
                    {
                        return inner.CreateLiteralNode(literal, langSpec);
                    }

                    public ITripleNode CreateTripleNode(Triple triple)
                    {
                        return inner.CreateTripleNode(triple);
                    }

                    public IUriNode CreateUriNode(Uri uri)
                    {
                        return inner.CreateUriNode(uri);
                    }

                    public IUriNode CreateUriNode(string qName)
                    {
                        return inner.CreateUriNode(qName);
                    }

                    public IUriNode CreateUriNode()
                    {
                        return inner.CreateUriNode();
                    }

                    public IVariableNode CreateVariableNode(string varName)
                    {
                        return inner.CreateVariableNode(varName);
                    }

                    public string GetNextBlankNodeID()
                    {
                        return inner.GetNextBlankNodeID();
                    }

                    public Uri ResolveQName(string qName)
                    {
                        return inner.ResolveQName(qName);
                    }
                    #endregion
                }
            }
        }
    }
}
