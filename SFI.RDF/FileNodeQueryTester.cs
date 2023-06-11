using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VDS.RDF;
using VDS.RDF.Query;

namespace IS4.SFI.RDF
{
    /// <summary>
    /// Tests instances of <see cref="INode"/> against the input SPARQL queries
    /// and selects those which should be extracted.
    /// </summary>
    public class FileNodeQueryTester : NodeQueryTester
    {
        /// <summary>
        /// The name of the variable that must contain the current node in order to be extracted via a SELECT query.
        /// </summary>
        public const string NodeVariableName = "node";

        /// <inheritdoc/>
        public FileNodeQueryTester(IRdfHandler rdfHandler, Graph queryGraph, IReadOnlyCollection<SparqlQuery> queries) : base(rdfHandler, queryGraph, queries)
        {

        }

        /// <summary>
        /// Matches an instance of <see cref="INode"/> against the stored SPARQL queries.
        /// In order for the match to be successful, the variables bound by the query
        /// must contain a <see cref="NodeVariableName"/> variable equal to <paramref name="subject"/>.
        /// </summary>
        /// <param name="subject">The matching node to be identified by the queries.</param>
        /// <returns>A sequence of objects containing additional variables from a successful match.</returns>
        public override IEnumerable<INodeMatchProperties> Match(INode subject)
        {
            foreach(var query in Queries)
            {
                switch(Processor.ProcessQuery(query))
                {
                    case IGraph resultsGraph:
                        foreach(var triple in resultsGraph.Triples)
                        {
                            lock(Handler)
                            {
                                Handler.HandleTriple(triple);
                            }
                        }
                        break;
                    case IEnumerable<ISparqlResult> resultSet:
                        foreach(var result in resultSet)
                        {
                            if(result.TryGetValue(NodeVariableName, out var node) && node.Equals(subject))
                            {
                                var variables = new MatchProperties();
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
                                yield return variables;
                            }
                        }
                        break;
                }
            }
        }
    }
}
