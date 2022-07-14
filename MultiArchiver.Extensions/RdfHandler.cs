using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools.Xml;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;

namespace IS4.MultiArchiver.Extensions
{
    public class RdfHandler : VocabularyCache<IUriNode>, ILinkedNodeFactory
    {
        readonly IRdfHandler defaultHandler;
        readonly IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers;

        readonly ConcurrentDictionary<GraphUri, IRdfHandler> graphUriCache = new ConcurrentDictionary<GraphUri, IRdfHandler>();
        readonly ConcurrentDictionary<IRdfHandler, VocabularyCache<IUriNode>> graphCaches = new ConcurrentDictionary<IRdfHandler, VocabularyCache<IUriNode>>();

        readonly NodeQueryTester queryTester;

        public IIndividualUriFormatter<string> Root { get; }

        public const string BlankUriScheme = "x.blank";

        public IDictionary<Uri, string> PrefixMap { get; }

        public int MaxUriLength { get; set; } = 1900 - 20; // limit for OpenLink Virtuoso
        public int UriPartShortened { get; set; } = 64;

        public RdfHandler(IIndividualUriFormatter<string> root, IRdfHandler defaultHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, NodeQueryTester queryTester)
            : base(defaultHandler.CreateUriNode)
        {
            this.defaultHandler = defaultHandler;
            this.graphHandlers = graphHandlers;
            this.queryTester = queryTester;

            graphCaches[defaultHandler] = this;

            Root = root;

            PrefixMap = new ConcurrentDictionary<Uri, string>(Vocabulary.Vocabularies.Prefixes, new UriComparer());

            VocabularyAdded += (vocabulary) => {
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

        private string GetNamespace(Uri uri)
        {
            return PrefixMap.TryGetValue(uri, out var prefix) ?
                prefix : $"ns{System.Threading.Interlocked.Increment(ref namespaceCounter)}";
        }

        private void AddNamespace(IRdfHandler handler, string prefix, Uri uri)
        {
            lock(handler)
            {
                handler.HandleNamespace(prefix, uri);
            }
        }

        static readonly ConditionalWeakTable<INode, Uri> realUriCache = new ConditionalWeakTable<INode, Uri>();

        public ILinkedNode Create<T>(IIndividualUriFormatter<T> formatter, T value)
        {
            Uri uri;
            try{
                uri = formatter[value];
            }catch(UriFormatException) when(GlobalOptions.SuppressNonCriticalExceptions)
            {
                uri = new Uri($"{BlankUriScheme}:{Guid.NewGuid():D}", UriKind.Absolute);
            }
            var node = CreateNode(uri, defaultHandler);
            if(node == null)
            {
                return null;
            }
            return new UriNode(node, defaultHandler, GetGraphCache(defaultHandler), queryTester);
        }

        private INode CreateNode(Uri uri, IRdfHandler handler)
        {
            if(uri == null)
            {
                return null;
            }
            if(uri.Scheme == BlankUriScheme)
            {
                return CreateBlankNode(uri, handler);
            }
            if(uri.OriginalString.Length > MaxUriLength)
            {
                var shortUriLabel = UriTools.ShortenUri(uri, UriPartShortened, "\u00A0(URI\u00A0too\u00A0long)");

                var newUri = UriTools.UriToUuidUri(uri);
                var subject = handler.CreateUriNode(newUri);
                realUriCache.Add(subject, uri);
                var node = new UriNode(subject, handler, GetGraphCache(handler), queryTester);
                node.Set(Properties.AtPrefLabel, shortUriLabel.ToString(), Datatypes.AnyUri);
                var linked = node.In(Graphs.ShortenedLinks);
                if(linked != null)
                {
                    linked.Set(Properties.SameAs, UriFormatter.Instance, uri);
                }
                return subject;
            }
            return defaultHandler.CreateUriNode(uri);
        }

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
        
        Cache GetGraphCache(IRdfHandler handler)
        {
            var cache = graphCaches.GetOrAdd(handler, h => new VocabularyCache<IUriNode>(h.CreateUriNode));
            return new Cache(this, cache);
        }

        IRdfHandler GetGraphHandler(GraphUri name)
        {
            if(graphUriCache.TryGetValue(name, out var handler))
            {
                return handler;
            }
            var uri = new Uri(name.Value, UriKind.Absolute);
            return graphUriCache[name] = GetGraphHandler(uri);
        }

        IRdfHandler GetGraphHandler(Uri uri)
        {
            if(graphHandlers.TryGetValue(uri, out var handler))
            {
                return handler;
            }
            return defaultHandler;
        }

        struct Cache :
            IVocabularyCache<ClassUri, IUriNode>, IVocabularyCache<PropertyUri, IUriNode>,
            IVocabularyCache<IndividualUri, IUriNode>, IVocabularyCache<DatatypeUri, IUriNode>,
            IVocabularyCache<GraphUri, IRdfHandler>
        {
            public RdfHandler Parent { get; }
            public VocabularyCache<IUriNode> Inner { get; }

            public Cache(RdfHandler handler, VocabularyCache<IUriNode> cache)
            {
                Parent = handler;
                Inner = cache;
            }

            public IUriNode this[ClassUri name] => Inner[name];
            public IUriNode this[PropertyUri name] => Inner[name];
            public IUriNode this[IndividualUri name] => Inner[name];
            public IUriNode this[DatatypeUri name] => Inner[name];
            public IRdfHandler this[GraphUri name] => Parent.GetGraphHandler(name);
            public IReadOnlyCollection<VocabularyUri> Vocabularies => Inner.Vocabularies;
        }

        class UriNode : LinkedNode<INode, IRdfHandler, Cache>
        {
            readonly NodeQueryTester queryTester;

            public UriNode(INode subject, IRdfHandler handler, Cache cache, NodeQueryTester queryTester) : base(subject, handler, cache)
            {
                if(!(subject is IUriNode || subject is IBlankNode)) throw new ArgumentException(null, nameof(subject));
                this.queryTester = queryTester;
            }

            protected override void HandleTriple(INode subj, INode pred, INode obj)
            {
                var date = DateTime.UtcNow;
                lock(Graph)
                {
                    Graph.HandleTriple(new Triple(subj, pred, obj));
                }
                var meta = In(Graphs.Metadata);
                if(meta != null && !Equals(meta))
                {
                    var dateString = XmlConvert.ToString(date, "yyyy-MM-dd\\THH:mm:ssK");
                    meta.Set(Properties.Visited, dateString, Datatypes.DateTime);
                }
            }

            private bool HandleExternalTriple(INode subj, INode pred, INode obj)
            {
                if(Subject.Equals(subj) && !(obj is IBlankNode))
                {
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

            public override bool Match(out IReadOnlyDictionary<string, object> properties)
            {
                if(queryTester == null)
                {
                    properties = null;
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

            protected override LinkedNode<INode, IRdfHandler, Cache> CreateInGraph(IRdfHandler graph)
            {
                if(graph == null)
                {
                    return null;
                }
                return new UriNode(VDS.RDF.Tools.CopyNode(Subject, graph), graph, Cache.Parent.GetGraphCache(graph), queryTester);
            }

            protected override IRdfHandler CreateGraphNode(Uri uri)
            {
                return Cache.Parent.GetGraphHandler(uri);
            }

            private XmlDocument PrepareXmlDocument(XmlReader rdfXmlReader)
            {
                if(rdfXmlReader.NodeType != XmlNodeType.Element || rdfXmlReader.NamespaceURI != "http://www.w3.org/1999/02/22-rdf-syntax-ns#" || rdfXmlReader.LocalName != "RDF")
                {
                    throw new ArgumentException("The XML reader must be positioned on a rdf:RDF element.", nameof(rdfXmlReader));
                }
                return new BaseXmlDocument(GetUri(Subject).AbsoluteUri, rdfXmlReader.NameTable);
            }

            public override void Describe(XmlReader rdfXmlReader)
            {
                var doc = PrepareXmlDocument(rdfXmlReader);
                doc.Load(rdfXmlReader);
                Describe(doc);
            }

            public override async Task DescribeAsync(XmlReader rdfXmlReader)
            {
                var doc = PrepareXmlDocument(rdfXmlReader);
                await doc.LoadAsync(rdfXmlReader);
                Describe(doc);
            }

            public override void Describe(XmlDocument rdfXmlDocument)
            {
                if(rdfXmlDocument is BaseXmlDocument baseXmlDocument && baseXmlDocument.BaseURI == null)
                {
                    baseXmlDocument.SetBaseURI(GetUri(Subject).AbsoluteUri);
                }
                var graph = new DataGraph(this);
                var parser = new VDS.RDF.Parsing.RdfXmlParser();
                parser.Load(graph, rdfXmlDocument);
            }

            class DataGraph : Graph
            {
                readonly UriNode describingNode;

                public DataGraph(UriNode describingNode) : base(true)
                {
                    this.describingNode = describingNode;
                }

                public override bool Assert(Triple t)
                {
                    return describingNode.HandleExternalTriple(t.Subject, t.Predicate, t.Object);
                }
            }
        }
    }
}
