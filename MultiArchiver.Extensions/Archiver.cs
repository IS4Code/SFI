using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
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
        public XmlAnalyzer XmlAnalyzer { get; }
        public BitTorrentHash BitTorrentHash { get; }

        public Archiver()
        {
            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(() => new UdeEncodingDetector()));
            Analyzers.Add(XmlAnalyzer = new XmlAnalyzer());
            Analyzers.Add(new BinaryFormatAnalyzer<object>());
            Analyzers.Add(new XmlFormatAnalyzer<object>());

            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5);
            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA1);
            DataAnalyzer.HashAlgorithms.Add(new PaddedBlockHash(Vocabulary.Individuals.BSHA1_256, "urn:bsha1-256:", 262144));

            FileAnalyzer.HashAlgorithms.Add(BitTorrentHash = new BitTorrentHash());
        }

        public static Archiver CreateDefault()
        {
            var archiver = new Archiver();

            archiver.DataAnalyzer.Formats.Add(new XmlFileFormat());
            archiver.DataAnalyzer.Formats.Add(new ZipFormat());
            archiver.DataAnalyzer.Formats.Add(new RarFormat());
            archiver.DataAnalyzer.Formats.Add(new SevenZipFormat());
            archiver.DataAnalyzer.Formats.Add(new GZipFormat());
            archiver.DataAnalyzer.Formats.Add(new TarFormat());
            archiver.DataAnalyzer.Formats.Add(new ImageMetadataFormat());
            archiver.DataAnalyzer.Formats.Add(new IsoFormat());
            archiver.DataAnalyzer.Formats.Add(new PeFormat());
            archiver.Analyzers.Add(new ArchiveAnalyzer());
            archiver.Analyzers.Add(new ArchiveReaderAnalyzer());
            archiver.Analyzers.Add(new FileSystemAnalyzer());
            archiver.Analyzers.Add(ImageMetadataAnalyzer.CreateDefault());
            archiver.Analyzers.Add(new PeAnalyzer());

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
                FileAnalyzer.Analyze(null, new FileInfo(file), handler);
            }finally{
                graphHandler.EndRdf(true);
            }
            foreach(var voc in handler.Vocabularies)
            {
                graph.NamespaceMap.AddNamespace(voc.Key.ToString().ToLowerInvariant(), new Uri(voc.Value, UriKind.Absolute));
            }
            foreach(var hash in DataAnalyzer.HashAlgorithms)
            {
                if(hash.FormattingMethod != FormattingMethod.Base64)
                {
                    graph.NamespaceMap.AddNamespace(hash.Name, hash.FormatUri(Array.Empty<byte>()));
                }
            }
            foreach(var hash in FileAnalyzer.HashAlgorithms)
            {
                if(hash.FormattingMethod != FormattingMethod.Base64)
                {
                    graph.NamespaceMap.AddNamespace(hash.Name, hash.FormatUri(Array.Empty<byte>()));
                }
            }
            graph.NamespaceMap.AddNamespace("id", new Uri(root + "/"));
            graph.NamespaceMap.AddNamespace("dtxt", new Uri("data:,"));
            graph.NamespaceMap.AddNamespace("dt64", new Uri("data:;base64,"));
            graph.NamespaceMap.AddNamespace("dbin", new Uri("data:application/octet-stream,"));
            graph.NamespaceMap.AddNamespace("db64", new Uri("data:application/octet-stream;base64,"));
            graph.NamespaceMap.AddNamespace("exif", new Uri("http://www.w3.org/2003/12/exif/ns#"));
            var writer = new CompressingTurtleWriter(TurtleSyntax.Original);
            writer.PrettyPrintMode = true;
            writer.DefaultNamespaces.Clear();
            graph.SaveToFile(output, writer);
        }
    }
}
