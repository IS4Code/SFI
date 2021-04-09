using IS4.MultiArchiver;
using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using VDS.RDF;

namespace MultiArchiverExtensions
{
    public class Archiver
    {
        readonly EntityAnalyzer analyzer = new EntityAnalyzer();
        readonly FileAnalyzer fileAnalyzer;
        readonly DataAnalyzer dataAnalyzer;

        public Archiver()
        {
            var hash = BuiltInHash.MD5;

            analyzer.Analyzers.Add(fileAnalyzer = new FileAnalyzer());
            analyzer.Analyzers.Add(dataAnalyzer = new DataAnalyzer(hash, () => new UdeEncodingDetector()));
            dataAnalyzer.Formats.Add(new XmlFileFormat());
            analyzer.Analyzers.Add(new FormatObjectAnalyzer());
            analyzer.Analyzers.Add(new XmlAnalyzer());
        }

        public void Archive(string file, string output)
        {
            var graph = new Graph();
            var graphHandler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            var handler = new RdfHandler(new Uri("http://archive.data.is4.site/.well-known/genid"), graphHandler, analyzer);
            graphHandler.StartRdf();
            try{
                new FileAnalyzer().Analyze(new FileInfo(file), handler);
            }finally
            {
                graphHandler.EndRdf(true);
            }
            graph.SaveToFile(output);
        }
    }
}
