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

        public IDictionary<Uri, string> PrefixMap { get; }

        public int MaxUriLength { get; set; } = 4096 - 128;
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
                    AddNamespace(handler, prefix, uri);
                }
                foreach(var query in this.queryTester.Queries)
                {
                    query.NamespaceMap.AddNamespace(prefix, uri);
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

        static readonly ConditionalWeakTable<INode, Uri> longUriCache = new ConditionalWeakTable<INode, Uri>();


        public ILinkedNode Create<T>(IIndividualUriFormatter<T> formatter, T value)
        {
            var uri = formatter[value];
            if(uri == null)
            {
                return null;
            }
            if(uri.OriginalString.Length > MaxUriLength)
            {
                var shortUriLabel = UriTools.ShortenUri(uri, UriPartShortened, "\u00A0(URI\u00A0too\u00A0long)");

                var newUri = UriTools.UriToUuidUri(uri);
                var subject = defaultHandler.CreateUriNode(newUri);
                longUriCache.Add(subject, uri);
                var node = new UriNode(subject, defaultHandler, GetGraphCache(defaultHandler), queryTester);
                node.Set(Properties.AtPrefLabel, shortUriLabel.ToString(), Datatypes.AnyUri);
                return node;
            }
            return new UriNode(defaultHandler.CreateUriNode(uri), defaultHandler, GetGraphCache(defaultHandler), queryTester);
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
            public new IUriNode Subject => (IUriNode)base.Subject;

            readonly NodeQueryTester queryTester;

            public UriNode(INode subject, IRdfHandler handler, Cache cache, NodeQueryTester queryTester) : base(subject, handler, cache)
            {
                if(!(subject is IUriNode)) throw new ArgumentException(null, nameof(subject));
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
                lock(Graph)
                {
                    Graph.HandleBaseUri(Subject.Uri);
                }
            }

            public override bool Match(out IReadOnlyDictionary<string, object> properties)
            {
                return queryTester.Match(Subject.Uri, out properties);
            }

            protected override INode CreateNode(Uri uri)
            {
                return Graph.CreateUriNode(uri);
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
                return longUriCache.TryGetValue(node, out var uri) ? uri : ((IUriNode)node).Uri;
            }

            protected override LinkedNode<INode, IRdfHandler, Cache> CreateNew(INode subject)
            {
                return new UriNode(subject, Graph, Cache, queryTester);
            }

            protected override LinkedNode<INode, IRdfHandler, Cache> CreateInGraph(IRdfHandler graph)
            {
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
