using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace IS4.MultiArchiver.Extensions
{
    public class Archiver : EntityAnalyzer
    {
        public FileAnalyzer FileAnalyzer { get; }
        public DataAnalyzer DataAnalyzer { get; }
        public BitTorrentHash BitTorrentHash { get; }

        public Archiver()
        {
            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(() => new UdeEncodingDetector()));

            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5);
            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA1);

            FileAnalyzer.HashAlgorithms.Add(BitTorrentHash = new BitTorrentHash());
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
            var root = "http://archive.data.is4.site/.well-known/genid";
            var handler = new RdfHandler(new Uri(root), graphHandler, this);
            graphHandler.StartRdf();
            try{
                new FileAnalyzer().Analyze(new FileInfo(file), handler);
            }finally
            {
                graphHandler.EndRdf(true);
            }
            foreach(var voc in handler.Vocabularies)
            {
                graph.NamespaceMap.AddNamespace(voc.Key.ToString().ToLowerInvariant(), new Uri(voc.Value, UriKind.Absolute));
            }
            foreach(var hash in DataAnalyzer.HashAlgorithms)
            {
                graph.NamespaceMap.AddNamespace(hash.Name, hash.FormatUri(Array.Empty<byte>()));
            }
            foreach(var hash in FileAnalyzer.HashAlgorithms)
            {
                graph.NamespaceMap.AddNamespace(hash.Name, hash.FormatUri(Array.Empty<byte>()));
            }
            graph.NamespaceMap.AddNamespace("id", new Uri(root + "/"));
            var writer = new CompressingTurtleWriter(TurtleSyntax.W3C);
            writer.PrettyPrintMode = true;
            writer.DefaultNamespaces.Clear();
            graph.SaveToFile(output, writer);
        }
    }
}
