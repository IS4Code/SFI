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
        public IUriNode CreateUriNode(Uri uri)
        {
            lock(inner) return inner.CreateUriNode(uri);
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
        public bool HandleTriple(Triple t)
        {
            lock(inner) return inner.HandleTriple(t);
        }

        /// <inheritdoc/>
        public void StartRdf()
        {
            lock(inner) inner.StartRdf();
        }
    }
}
