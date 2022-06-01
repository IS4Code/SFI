using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query;

namespace IS4.MultiArchiver
{
    public class NodeQueryTester
    {
        public Graph QueryGraph { get; set; }
        public IReadOnlyCollection<SparqlQuery> Queries { get; set; }

        public NodeQueryTester(Graph queryGraph, IReadOnlyCollection<SparqlQuery> queries)
        {
            QueryGraph = queryGraph;
            Queries = queries;
        }

        public bool Match(Uri subject, out IReadOnlyDictionary<string, object> properties)
        {
            properties = null;
            return false;
        }
    }
}
