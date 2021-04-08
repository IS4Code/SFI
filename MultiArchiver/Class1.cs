using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class Class1
    {
        readonly IReadOnlyCollection<object> analyzers;

        public Class1()
        {
            var hash = new BuiltInHash(MD5.Create, Individuals.MD5, "urn:md5:");

            var formats = new List<IFileFormat>
            {
                new XmlFileFormat()
            };

            var graph = new Graph();
            analyzers = new ConcurrentBag<object>()
            {
                new FileAnalyzer(),
                new DataAnalyzer(hash, formats),
                new FormatObjectAnalyzer(),
                new XmlAnalyzer()
            };

            //string path = "image.zip";
            string path = "graph.ttl";

            var handler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            handler.StartRdf();
            try{
                new FileAnalyzer().Analyze(new FileInfo(path), new TripleHandler(this, handler));
            }finally{
                handler.EndRdf(true);
            }
            graph.SaveToFile("graph.ttl");
        }

        ILinkedNode Analyze<T>(T entity, TripleHandler handler) where T : class
        {
            if(entity == null) return null;
            foreach(var obj in analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    var result = analyzer.Analyze(entity, handler);
                    if(result != null) return result;
                }
            }
            return null;
        }
    }
}
