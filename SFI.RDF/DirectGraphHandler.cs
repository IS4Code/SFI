using VDS.RDF;
using VDS.RDF.Parsing.Handlers;

namespace IS4.SFI.RDF
{
    /// <summary>
    /// Represents an <see cref="IRdfHandler"/> that asserts triples directly
    /// into a graph.
    /// </summary>
    public class DirectGraphHandler : BaseRdfHandler
    {
        readonly IGraph graph;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="graph">The graph to assert to.</param>
        public DirectGraphHandler(IGraph graph) : base(graph)
        {
            this.graph = graph;
        }

        /// <inheritdoc/>
        public override bool AcceptsAll => true;

        /// <inheritdoc/>
        protected override bool HandleTripleInternal(Triple t)
        {
            return graph.Assert(t);
        }

        /// <inheritdoc/>
        protected override bool HandleQuadInternal(Triple t, IRefNode graph)
        {
            if(graph.Equals(this.graph.Name))
            {
                return HandleTripleInternal(t);
            }
            return true;
        }
    }
}
