using IS4.MultiArchiver;
using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using VDS.RDF;

namespace IS4.MultiArchiver.Extensions
{
    public class Archiver : EntityAnalyzer
    {
        public FileAnalyzer FileAnalyzer { get; }
        public DataAnalyzer DataAnalyzer { get; }

        public Archiver()
        {
            var hash = BuiltInHash.MD5;

            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(hash, () => new UdeEncodingDetector()));
            DataAnalyzer.Formats.Add(new XmlFileFormat());
            Analyzers.Add(new FormatObjectAnalyzer());
            Analyzers.Add(new XmlAnalyzer());
        }

        public void Archive(string file, string output)
        {
            var graph = new Graph();
            var graphHandler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            var handler = new RdfHandler(new Uri("http://archive.data.is4.site/.well-known/genid"), graphHandler, this);
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
