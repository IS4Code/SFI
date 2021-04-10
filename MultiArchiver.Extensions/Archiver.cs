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
        }

        public static Archiver CreateDefault()
        {
            var archiver = new Archiver();

            archiver.DataAnalyzer.Formats.Add(new XmlFileFormat());
            archiver.DataAnalyzer.Formats.Add(new ZipFileFormat());
            archiver.DataAnalyzer.Formats.Add(new ImageMetadataFormat());
            archiver.DataAnalyzer.Formats.Add(new IsoFormat());
            archiver.Analyzers.Add(new FormatObjectAnalyzer());
            archiver.Analyzers.Add(new XmlAnalyzer());
            archiver.Analyzers.Add(new ArchiveAnalyzer());
            archiver.Analyzers.Add(new FileSystemAnalyzer());
            archiver.Analyzers.Add(ImageMetadataAnalyzer.CreateDefault());

            return archiver;
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
