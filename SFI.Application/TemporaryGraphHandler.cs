using System;
using System.Collections.Generic;
using VDS.RDF;

namespace IS4.SFI
{
    /// <summary>
    /// An RDF handler that asserts triples in a temporary graph
    /// which is regularly cleared.
    /// </summary>
    sealed class TemporaryGraphHandler : IRdfHandler
    {
        public const int Capacity = 2;

        readonly IRdfHandler baseHandler;

        readonly Graph mergedGraph;
        readonly Queue<Graph> graphHistory = new(Capacity);
        Graph currentGraph;

        readonly bool isProxy;

        Uri? lastBaseUri;

        static readonly UriComparer comparer = new();

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
            mergedGraph = graph = new Graph(true);
            graphHistory.Enqueue(currentGraph = new Graph(true));
            isProxy = true;
        }

        /// <inheritdoc cref="TemporaryGraphHandler.TemporaryGraphHandler(IRdfHandler, out Graph)"/>
        public TemporaryGraphHandler(out Graph graph)
        {
            mergedGraph = graph = new Graph(true);
            graphHistory.Enqueue(currentGraph = new Graph(true));
            baseHandler = new DirectGraphHandler(graph);
        }

        void ClearTemporary()
        {
            if(graphHistory.Count == Capacity)
            {
                mergedGraph.Clear();
                graphHistory.Dequeue();
                foreach(var graph in graphHistory)
                {
                    mergedGraph.Merge(graph);
                }
            }
            graphHistory.Enqueue(currentGraph = new Graph(true));
        }

        public bool HandleBaseUri(Uri baseUri)
        {
            if(lastBaseUri == null)
            {
                lastBaseUri = baseUri;
            }else if(!comparer.Equals(lastBaseUri, baseUri))
            {
                if(lastBaseUri.OriginalString.StartsWith(baseUri.OriginalString))
                {
                    // do not clear if new base is broader
                    lastBaseUri = baseUri;
                }else if(!baseUri.OriginalString.StartsWith(lastBaseUri.OriginalString))
                {
                    // only clear if unrelated
                    ClearTemporary();
                    lastBaseUri = baseUri;
                }
            }
            return baseHandler.HandleBaseUri(baseUri);
        }

        public bool HandleTriple(Triple t)
        {
            if(isProxy) mergedGraph.Assert(VDS.RDF.Tools.CopyTriple(t, mergedGraph));
            currentGraph.Assert(VDS.RDF.Tools.CopyTriple(t, currentGraph));
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
