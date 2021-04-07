using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class Class1 : IEntityAnalyzer
    {
        readonly IReadOnlyCollection<object> analyzers;

        public Class1()
        {
            var graph = new Graph();
            analyzers = new ConcurrentBag<object>()
            {
                new FileAnalyzer(graph),
                new DataAnalyzer(graph),
                new FormatObjectAnalyzer(graph)
            };

            var handler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            handler.StartRdf();
            try
            {
                new FileAnalyzer(graph).Analyze(new FileInfo("image.zip"), handler, this);
            } finally
            {
                handler.EndRdf(true);
            }
            graph.SaveToFile("graph.ttl");
        }

        IUriNode IEntityAnalyzer.Analyze<T>(T entity, IRdfHandler handler)
        {
            if(entity == null) return null;
            foreach(var obj in analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    var result = analyzer.Analyze(entity, handler, this);
                    if(result != null) return result;
                }
            }
            return null;
        }
    }
}
