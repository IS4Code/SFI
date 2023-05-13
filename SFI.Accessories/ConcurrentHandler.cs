using System;
using VDS.RDF;

namespace IS4.SFI
{
    /// <summary>
    /// Locks operations on an instance of <see cref="IRdfHandler"/>
    /// so that they can be used concurrently.
    /// </summary>
    public class ConcurrentHandler : IRdfHandler
    {
        readonly IRdfHandler inner;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="inner">The wrapped handler.</param>
        public ConcurrentHandler(IRdfHandler inner)
        {
            this.inner = inner;
        }

        /// <inheritdoc/>
        public bool AcceptsAll {
            get {
                lock(inner) return inner.AcceptsAll;
            }
        }

        /// <inheritdoc/>
        public Uri BaseUri { get => inner.BaseUri; set => inner.BaseUri = value; }

        /// <inheritdoc/>
        public INamespaceMapper NamespaceMap => inner.NamespaceMap;

        /// <inheritdoc/>
        public IUriFactory UriFactory { get => inner.UriFactory; set => inner.UriFactory = value; }

        /// <inheritdoc/>
        public bool NormalizeLiteralValues { get => inner.NormalizeLiteralValues; set => inner.NormalizeLiteralValues = value; }

        /// <inheritdoc/>
        public IBlankNode CreateBlankNode()
        {
            lock(inner) return inner.CreateBlankNode();
        }

        /// <inheritdoc/>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            lock(inner) return inner.CreateBlankNode(nodeId);
        }

        /// <inheritdoc/>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            lock(inner) return inner.CreateGraphLiteralNode();
        }

        /// <inheritdoc/>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            lock(inner) return inner.CreateGraphLiteralNode(subgraph);
        }

        /// <inheritdoc/>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            lock(inner) return inner.CreateLiteralNode(literal, datatype);
        }

        /// <inheritdoc/>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            lock(inner) return inner.CreateLiteralNode(literal);
        }

        /// <inheritdoc/>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            lock(inner) return inner.CreateLiteralNode(literal, langspec);
        }

        /// <inheritdoc/>
        public ITripleNode CreateTripleNode(Triple triple)
        {
            lock(inner) return inner.CreateTripleNode(triple);
        }

        /// <inheritdoc/>
        public IUriNode CreateUriNode(Uri uri)
        {
            lock(inner) return inner.CreateUriNode(uri);
        }

        /// <inheritdoc/>
        public IUriNode CreateUriNode(string qName)
        {
            lock(inner) return inner.CreateUriNode(qName);
        }

        /// <inheritdoc/>
        public IUriNode CreateUriNode()
        {
            lock(inner) return inner.CreateUriNode();
        }

        /// <inheritdoc/>
        public IVariableNode CreateVariableNode(string varname)
        {
            lock(inner) return inner.CreateVariableNode(varname);
        }

        /// <inheritdoc/>
        public void EndRdf(bool ok)
        {
            lock(inner) inner.EndRdf(ok);
        }

        /// <inheritdoc/>
        public string GetNextBlankNodeID()
        {
            lock(inner) return inner.GetNextBlankNodeID();
        }

        /// <inheritdoc/>
        public bool HandleBaseUri(Uri baseUri)
        {
            lock(inner) return inner.HandleBaseUri(baseUri);
        }

        /// <inheritdoc/>
        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            lock(inner) return inner.HandleNamespace(prefix, namespaceUri);
        }

        /// <inheritdoc/>
        public bool HandleQuad(Triple t, IRefNode graph)
        {
            lock(inner) return inner.HandleQuad(t, graph);
        }

        /// <inheritdoc/>
        public bool HandleTriple(Triple t)
        {
            lock(inner) return inner.HandleTriple(t);
        }

        /// <inheritdoc/>
        public Uri ResolveQName(string qName)
        {
            return inner.ResolveQName(qName);
        }

        /// <inheritdoc/>
        public void StartRdf()
        {
            lock(inner) inner.StartRdf();
        }
    }
}
