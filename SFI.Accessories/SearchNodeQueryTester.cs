using IS4.SFI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VDS.RDF;
using VDS.RDF.Query;

namespace IS4.SFI
{
    /// <summary>
    /// Tests instances of <see cref="INode"/> against the input SPARQL queries
    /// and stops when results are found. The result is outputted using
    /// an instance of <see cref="SearchEndedException"/>.
    /// </summary>
    public class SearchNodeQueryTester : NodeQueryTester
    {
        readonly ConcurrentQueue<SparqlResult> results = new();

        /// <inheritdoc/>
        public SearchNodeQueryTester(IRdfHandler rdfHandler, Graph queryGraph, IReadOnlyCollection<SparqlQuery> queries) : base(rdfHandler, queryGraph, queries)
        {

        }

        /// <summary>
        /// Returns the result of the whole search.
        /// </summary>
        /// <returns>The total result set.</returns>
        public SparqlResultSet GetResultSet()
        {
            if(results.Count > 0)
            {
                return new SparqlResultSet(results);
            }
            if(Queries.Any(q => q.QueryType == SparqlQueryType.Ask))
            {
                return new SparqlResultSet(false);
            }
            return new SparqlResultSet(Array.Empty<SparqlResult>());
        }

        /// <inheritdoc/>
        public override bool Match(INode subject, out INodeMatchProperties properties)
        {
            try{
                var resultsHandler = new SparqlResultsHandler(Handler, results);
                var continuing = false;
                foreach(var query in Queries)
                {
                    if(query.Limit == 0)
                    {
                        continue;
                    }

                    lock(query)
                    {
                        if(query.Limit == 0)
                        {
                            continue;
                        }

                        var offset = query.Offset;
                        var limit = query.Limit;
                        try{
                            // Ignore offset so that we know the number of skipped results
                            query.Offset = 0;
                            if(limit >= 0)
                            {
                                query.Limit = limit + offset;
                            }
                            lock(Handler)
                            {
                                Processor.ProcessQuery(Handler, resultsHandler, query);
                            }
                        }finally{
                            query.Limit = limit;
                            query.Offset = offset;
                        }

                        if(resultsHandler.Result)
                        {
                            throw new SearchEndedException(new SparqlResultSet(true));
                        }

                        int count = resultsHandler.Count;
                        if(count > 0)
                        {
                            if(offset > 0)
                            {
                                query.Offset = Math.Max(0, offset - count);
                            }
                            if(limit >= 0)
                            {
                                query.Limit = limit = Math.Max(0, limit - count);
                                if(limit == 0)
                                {
                                    continue;
                                }
                            }
                        }
                        // This query was not satisfied
                        continuing = true;
                    }
                }
                if(!continuing)
                {
                    // All queries were satisfied
                    throw new SearchEndedException(new SparqlResultSet(results));
                }
            }catch(SearchEndedException e)
            {
                throw new InternalApplicationException(e);
            }
            properties = MatchProperties.Default;
            return false;
        }

        class SparqlResultsHandler : ISparqlResultsHandler
        {
            readonly INodeFactory nodeFactory;
            readonly IProducerConsumerCollection<SparqlResult> results;

            public bool Result { get; private set; }

            public int Count { get; private set; }

            public bool Success { get; private set; }

            public SparqlResultsHandler(INodeFactory nodeFactory, IProducerConsumerCollection<SparqlResult> results)
            {
                this.nodeFactory = nodeFactory;
                this.results = results;
            }

            public void StartResults()
            {
                Result = false;
                Count = 0;
            }

            public bool HandleVariable(string var)
            {
                return true;
            }

            public bool HandleResult(SparqlResult result)
            {
                ++Count;
                return results.TryAdd(result);
            }

            public void HandleBooleanResult(bool result)
            {
                Result = result;
            }

            public void EndResults(bool ok)
            {
                Success = ok;
            }

            #region INodeFactory implementation
            public IBlankNode CreateBlankNode()
            {
                return nodeFactory.CreateBlankNode();
            }

            public IBlankNode CreateBlankNode(string nodeId)
            {
                return nodeFactory.CreateBlankNode(nodeId);
            }

            public IGraphLiteralNode CreateGraphLiteralNode()
            {
                return nodeFactory.CreateGraphLiteralNode();
            }

            public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
            {
                return nodeFactory.CreateGraphLiteralNode(subgraph);
            }

            public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
            {
                return nodeFactory.CreateLiteralNode(literal, datatype);
            }

            public ILiteralNode CreateLiteralNode(string literal)
            {
                return nodeFactory.CreateLiteralNode(literal);
            }

            public ILiteralNode CreateLiteralNode(string literal, string langspec)
            {
                return nodeFactory.CreateLiteralNode(literal, langspec);
            }

            public IUriNode CreateUriNode(Uri uri)
            {
                return nodeFactory.CreateUriNode(uri);
            }

            public IVariableNode CreateVariableNode(string varname)
            {
                return nodeFactory.CreateVariableNode(varname);
            }

            public string GetNextBlankNodeID()
            {
                return nodeFactory.GetNextBlankNodeID();
            }
            #endregion
        }

        /// <summary>
        /// Thrown when all SPARQL queries were satisfied and there is nothing more to search.
        /// </summary>
        public class SearchEndedException : ApplicationException
        {
            /// <summary>
            /// The collective SPARQL results.
            /// </summary>
            public SparqlResultSet ResultSet { get; } = null!;

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            /// <param name="resultSet">The value of <see cref="ResultSet"/>.</param>
            public SearchEndedException(SparqlResultSet resultSet) : base("The SPARQL search has ended.")
            {
                ResultSet = resultSet;
            }

            /// <inheritdoc/>
            protected SearchEndedException(SerializationInfo info, StreamingContext context) : base(info, context)
            {

            }
        }
    }
}
