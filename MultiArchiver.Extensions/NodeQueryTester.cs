using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace IS4.MultiArchiver
{
    public class NodeQueryTester
    {
        public Graph QueryGraph { get; set; }
        public IReadOnlyCollection<SparqlQuery> Queries { get; set; }

        readonly IRdfHandler rdfHandler;
        readonly LeviathanQueryProcessor processor;

        static readonly UriComparer uriComparer = new UriComparer();

        public NodeQueryTester(IRdfHandler rdfHandler, Graph queryGraph, IReadOnlyCollection<SparqlQuery> queries)
        {
            QueryGraph = queryGraph;
            Queries = queries;

            this.rdfHandler = rdfHandler;
            var dataset = new InMemoryDataset(queryGraph);
            processor = new LeviathanQueryProcessor(dataset);
        }

        public bool Match(Uri subject, out IReadOnlyDictionary<string, object> properties)
        {
            Dictionary<string, object> variables = null;
            var success = false;
            foreach(var query in Queries)
            {
                switch(processor.ProcessQuery(query))
                {
                    case IGraph resultsGraph:
                        foreach(var triple in resultsGraph.Triples)
                        {
                            rdfHandler.HandleTriple(triple);
                        }
                        break;
                    case IEnumerable<SparqlResult> resultSet:
                        foreach(var result in resultSet)
                        {
                            if(result["node"] is IUriNode uriNode && uriComparer.Equals(uriNode.Uri, subject))
                            {
                                success = true;
                                if(variables == null)
                                {
                                    variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                }
                                foreach(var pair in result)
                                {
                                    switch(pair.Value)
                                    {
                                        case IUriNode uriValue:
                                            variables[pair.Key] = uriValue.Uri;
                                            break;
                                        case ILiteralNode literalValue:
                                            variables[pair.Key] = literalValue.Value;
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            properties = variables;
            return success;
        }
    }
}
