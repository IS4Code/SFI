using IS4.MultiArchiver;
using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;

namespace MultiArchiverExtensions
{
    public class Archiver
    {
        readonly EntityAnalyzer analyzer = new EntityAnalyzer();

        public Archiver()
        {
            var hash = BuiltInHash.MD5;

            var formats = new List<IFileFormat>
            {
                new XmlFileFormat()
            };

            analyzer.Analyzers.Add(new FileAnalyzer());
            analyzer.Analyzers.Add(new DataAnalyzer(hash, () => new UdeEncodingDetector(), formats));
            analyzer.Analyzers.Add(new FormatObjectAnalyzer());
            analyzer.Analyzers.Add(new XmlAnalyzer());
        }

        public void Archive(string file, string output)
        {
            var graph = new Graph();
            var graphHandler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            var handler = new RdfHandler(graphHandler, analyzer);
            graphHandler.StartRdf();
            try
            {
                new FileAnalyzer().Analyze(new FileInfo(file), handler);
            }finally
            {
                graphHandler.EndRdf(true);
            }
            graph.SaveToFile(output);
        }
    }
}
