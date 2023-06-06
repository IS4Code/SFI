using System;
using VDS.RDF;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// An implementation of <see cref="IRdfHandler"/> that wraps
    /// another <see cref="IRdfHandler"/> and also registers each namespace
    /// in an instance of <see cref="QNameOutputMapper"/>.
    /// </summary>
    sealed class NamespaceHandler : IRdfHandler
    {
        readonly IRdfHandler baseHandler;

        readonly QNameOutputMapper mapper;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="baseHandler">The base RDF handler to delegate the calls to.</param>
        /// <param name="mapper">A namespace mapper for registering namespaces from <see cref="HandleNamespace(string, Uri)"/>.</param>
        public NamespaceHandler(IRdfHandler baseHandler, QNameOutputMapper mapper)
        {
            this.baseHandler = baseHandler;
            this.mapper = mapper;
        }

        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            mapper.AddNamespace(prefix, namespaceUri);
            return baseHandler.HandleNamespace(prefix, namespaceUri);
        }

        #region Implementation
        public bool AcceptsAll => baseHandler.AcceptsAll;

        public Uri BaseUri { get => baseHandler.BaseUri; set => baseHandler.BaseUri = value; }

        public INamespaceMapper NamespaceMap => baseHandler.NamespaceMap;

        public IUriFactory UriFactory { get => baseHandler.UriFactory; set => baseHandler.UriFactory = value; }
        public bool NormalizeLiteralValues { get => baseHandler.NormalizeLiteralValues; set => baseHandler.NormalizeLiteralValues = value; }

        public IBlankNode CreateBlankNode()
        {
            return baseHandler.CreateBlankNode();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return baseHandler.CreateBlankNode(nodeId);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return baseHandler.CreateGraphLiteralNode();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return baseHandler.CreateGraphLiteralNode(subgraph);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return baseHandler.CreateLiteralNode(literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return baseHandler.CreateLiteralNode(literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return baseHandler.CreateLiteralNode(literal, langspec);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return baseHandler.CreateUriNode(uri);
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return baseHandler.CreateVariableNode(varname);
        }

        public void EndRdf(bool ok)
        {
            baseHandler.EndRdf(ok);
        }

        public string GetNextBlankNodeID()
        {
            return baseHandler.GetNextBlankNodeID();
        }

        public bool HandleBaseUri(Uri baseUri)
        {
            return baseHandler.HandleBaseUri(baseUri);
        }

        public bool HandleTriple(Triple t)
        {
            return baseHandler.HandleTriple(t);
        }

        public void StartRdf()
        {
            baseHandler.StartRdf();
        }

        public bool HandleQuad(Triple t, IRefNode graph)
        {
            return baseHandler.HandleQuad(t, graph);
        }

        public IUriNode CreateUriNode(string qName)
        {
            return baseHandler.CreateUriNode(qName);
        }

        public IUriNode CreateUriNode()
        {
            return baseHandler.CreateUriNode();
        }

        public ITripleNode CreateTripleNode(Triple triple)
        {
            return baseHandler.CreateTripleNode(triple);
        }

        public Uri ResolveQName(string qName)
        {
            return baseHandler.ResolveQName(qName);
        }
        #endregion
    }
}
