using System;
using VDS.RDF;

namespace IS4.SFI
{
    /// <summary>
    /// An RDF handler that asserts triples in a temporary graph
    /// which is regularly cleared.
    /// </summary>
    sealed class TemporaryGraphHandler : IRdfHandler
    {
        readonly IRdfHandler baseHandler;

        readonly Graph graph;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="baseHandler">The base RDF handler to delegate the calls to.</param>
        /// <param name="graph">
        /// The variable which receives the graph for the intermediate results.
        /// This graph is cleared on calls to <see cref="HandleBaseUri(Uri)"/>.
        /// </param>
        public TemporaryGraphHandler(IRdfHandler baseHandler, out Graph graph)
        {
            this.baseHandler = baseHandler;
            this.graph = graph = new Graph(true);
        }

        public bool HandleBaseUri(Uri baseUri)
        {
            graph.Clear();
            return baseHandler.HandleBaseUri(baseUri);
        }

        public bool HandleTriple(Triple t)
        {
            graph.Assert(VDS.RDF.Tools.CopyTriple(t, graph));
            return baseHandler.HandleTriple(t);
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

        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            return baseHandler.HandleNamespace(prefix, namespaceUri);
        }

        public void EndRdf(bool ok)
        {
            baseHandler.EndRdf(ok);
        }

        public string GetNextBlankNodeID()
        {
            return baseHandler.GetNextBlankNodeID();
        }

        public void StartRdf()
        {
            baseHandler.StartRdf();
        }
        #endregion
    }
}
