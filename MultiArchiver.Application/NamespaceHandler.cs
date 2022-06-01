using System;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    sealed class NamespaceHandler : IRdfHandler
    {
        readonly IRdfHandler baseHandler;

        readonly QNameOutputMapper mapper;

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
        #endregion
    }
}
