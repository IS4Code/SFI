using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace IS4.SFI
{
    /// <summary>
    /// Tests instances of <see cref="INode"/> whether they match
    /// any of the input SPARQL queries.
    /// </summary>
    public class NodeQueryTester
    {
        readonly IReadOnlyCollection<SparqlQuery> queries;
        readonly IRdfHandler rdfHandler;
        readonly LeviathanQueryProcessor processor;

        /// <summary>
        /// The name of the variable that must contain the current node in order to be extracted via a SELECT query.
        /// </summary>
        public const string NodeVariableName = "node";

        /// <summary>
        /// Creates a new instance of the tester.
        /// </summary>
        /// <param name="rdfHandler">The RDF handler to use to store the result of CONSTRUCT queries.</param>
        /// <param name="queryGraph">The graph to process with the queries.</param>
        /// <param name="queries">The collection of SPARQL queries to process.</param>
        public NodeQueryTester(IRdfHandler rdfHandler, Graph queryGraph, IReadOnlyCollection<SparqlQuery> queries)
        {
            this.queries = queries;
            this.rdfHandler = rdfHandler;
            var dataset = new InMemoryDataset(queryGraph);
            processor = new LeviathanQueryProcessor(dataset);
        }

        /// <summary>
        /// Matches an instance of <see cref="INode"/> against the stored SPARQL queries.
        /// In order for the match to be successful, the variables bound by the query
        /// must contain a <see cref="NodeVariableName"/> variable equal to <paramref name="subject"/>.
        /// </summary>
        /// <param name="subject">The matching node to be identified by the queries.</param>
        /// <param name="properties">Additional variables from a successful match, as instances of <see cref="Uri"/> or <see cref="String"/>.</param>
        /// <returns><see langword="true"/> if any of the queries successfully matched the node.</returns>
        public bool Match(INode subject, out INodeMatchProperties properties)
        {
            MatchProperties? variables = null;
            var success = false;
            foreach(var query in queries)
            {
                switch(processor.ProcessQuery(query))
                {
                    case IGraph resultsGraph:
                        foreach(var triple in resultsGraph.Triples)
                        {
                            lock(rdfHandler)
                            {
                                var copy = new Triple(
                                    VDS.RDF.Tools.CopyNode(triple.Subject, rdfHandler),
                                    VDS.RDF.Tools.CopyNode(triple.Predicate, rdfHandler),
                                    VDS.RDF.Tools.CopyNode(triple.Object, rdfHandler)
                                );
                                rdfHandler.HandleTriple(copy);
                            }
                        }
                        break;
                    case IEnumerable<SparqlResult> resultSet:
                        foreach(var result in resultSet)
                        {
                            if(result.TryGetValue(NodeVariableName, out var node) && node.Equals(subject))
                            {
                                success = true;
                                if(variables == null)
                                {
                                    variables = new MatchProperties();
                                }
                                foreach(var pair in result)
                                {
                                    if(variables.Properties.TryGetValue(pair.Key, out var prop))
                                    {
                                        var conv = TypeDescriptor.GetConverter(prop.PropertyType);
                                        switch(pair.Value)
                                        {
                                            case IUriNode uriValue:
                                                prop.SetValue(variables, conv.ConvertFrom(uriValue.Uri));
                                                break;
                                            case ILiteralNode literalValue:
                                                prop.SetValue(variables, conv.ConvertFromInvariantString(literalValue.Value));
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            properties = variables ?? MatchProperties.Default;
            return success;
        }

        class MatchProperties : INodeMatchProperties
        {
            public static readonly MatchProperties Default = new MatchProperties();

            /// <inheritdoc/>
            public string? Extension { get; set; }

            /// <inheritdoc/>
            public string? MediaType { get; set; }

            /// <inheritdoc/>
            public long? Size { get; set; }

            /// <inheritdoc/>
            public string? Name { get; set; }

            /// <inheritdoc/>
            public string? PathFormat { get; set; }

            Dictionary<string, PropertyDescriptor>? properties;

            public Dictionary<string, PropertyDescriptor> Properties => properties ??= this.GetProperties().ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
