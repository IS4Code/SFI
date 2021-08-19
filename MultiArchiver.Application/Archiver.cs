using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.MultiArchiver.Extensions
{
    public class Archiver : EntityAnalyzer
    {
        public FileAnalyzer FileAnalyzer { get; }
        public DataAnalyzer DataAnalyzer { get; }
        public XmlAnalyzer XmlAnalyzer { get; }
        public BitTorrentHash BitTorrentHash { get; }
        public ImageAnalyzer ImageAnalyzer { get; }

        public Archiver()
        {
            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(() => new UdeEncodingDetector()));
            Analyzers.Add(XmlAnalyzer = new XmlAnalyzer());
            Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());
            Analyzers.Add(new BinaryFormatAnalyzer<object>());
            Analyzers.Add(new XmlFormatAnalyzer<object>());

            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5);
            DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA1);
            DataAnalyzer.HashAlgorithms.Add(new PaddedBlockHash(Vocabulary.Individuals.BSHA1_256, "urn:bsha1-256:", 262144));
            DataAnalyzer.HashAlgorithms.Add(Blake3Hash.Instance);
            DataAnalyzer.HashAlgorithms.Add(Crc32Hash.Instance);
            DataAnalyzer.PrimaryHash = Blake3Hash.Instance;

            FileAnalyzer.HashAlgorithms.Add(BitTorrentHash = new BitTorrentHash());

            ImageAnalyzer.LowFrequencyImageHashAlgorithms.Add(Analysis.Images.DHash.Instance);
            ImageAnalyzer.DataHashAlgorithms.Add(BuiltInHash.MD5);
            ImageAnalyzer.DataHashAlgorithms.Add(BuiltInHash.SHA1);
        }

        static Archiver()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Options.InternUris = false;
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
            archiver.DataAnalyzer.Formats.Add(new SzFormat());
            archiver.DataAnalyzer.Formats.Add(new ImageMetadataFormat());
            archiver.DataAnalyzer.Formats.Add(new ImageFormat());
            archiver.DataAnalyzer.Formats.Add(new TagLibFormat());
            archiver.DataAnalyzer.Formats.Add(new IsoFormat());
            archiver.DataAnalyzer.Formats.Add(new DosModuleFormat());
            archiver.DataAnalyzer.Formats.Add(new GenericModuleFormat());
            archiver.DataAnalyzer.Formats.Add(new LinearModuleFormat());
            archiver.DataAnalyzer.Formats.Add(new Win16ModuleFormat());
            //archiver.DataAnalyzer.Formats.Add(new Win32ModuleFormat());
            archiver.DataAnalyzer.Formats.Add(new Win32ModuleFormatManaged());
            archiver.DataAnalyzer.Formats.Add(new WaveFormat());
            //archiver.DataAnalyzer.Formats.Add(new OggFormat());
            archiver.DataAnalyzer.Formats.Add(new WasapiFormat(false));
            archiver.DataAnalyzer.Formats.Add(new WasapiFormat(true));
            archiver.DataAnalyzer.Formats.Add(new DelphiFormFormat());
            archiver.DataAnalyzer.Formats.Add(new CabinetFormat());
            archiver.DataAnalyzer.Formats.Add(new OleStorageFormat());

            archiver.XmlAnalyzer.XmlFormats.Add(new SvgFormat());

            archiver.Analyzers.Add(new ArchiveAnalyzer());
            archiver.Analyzers.Add(new ArchiveReaderAnalyzer());
            archiver.Analyzers.Add(new FileSystemAnalyzer());
            archiver.Analyzers.Add(ImageMetadataAnalyzer.CreateDefault());
            archiver.Analyzers.Add(new TagLibAnalyzer());
            archiver.Analyzers.Add(new DosModuleAnalyzer());
            archiver.Analyzers.Add(new WinModuleAnalyzer());
            archiver.Analyzers.Add(new SvgAnalyzer());
            archiver.Analyzers.Add(new WaveAnalyzer());
            archiver.Analyzers.Add(new DelphiObjectAnalyzer());
            archiver.Analyzers.Add(new CabinetAnalyzer());
            archiver.Analyzers.Add(new OleStorageAnalyzer());

            return archiver;
        }

        public void Archive(string file, string output, bool direct = false, bool compressed = false)
        {
            var graphHandlers = new Dictionary<Uri, IRdfHandler>();

            if(direct)
            {
                Console.Error.WriteLine("Writing data...");

                var handler = CreateFileHandler(output, out var mapper, compressed);

                SetDefaultNamespaces(mapper);

                AnalyzeFile(file, handler, graphHandlers, mapper);
            }else{
                Console.Error.WriteLine("Reading data...");

                var handler = CreateGraphHandler(out var graph);

                SetDefaultNamespaces(graph.NamespaceMap);

                AnalyzeFile(file, handler, graphHandlers, null);

                Console.Error.WriteLine("Saving...");

                SaveGraph(graph, output, compressed);
            }
        }

        const string root = "http://archive.data.is4.site/.well-known/genid";

        private void AnalyzeFile(string file, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, INamespaceMapper mapper)
        {
            var handler = new RdfHandler(new Uri(root), this, rdfHandler, graphHandlers);
            rdfHandler.StartRdf();
            try{
                if(mapper != null)
                {
                    foreach(var prefix in mapper.Prefixes)
                    {
                        rdfHandler.HandleNamespace(prefix, mapper.GetNamespaceUri(prefix));
                    }
                }

                if((File.GetAttributes(file) & FileAttributes.Directory) != 0)
                {
                    FileAnalyzer.Analyze(null, new DirectoryInfo(file), handler);
                }else{
                    FileAnalyzer.Analyze(null, new FileInfo(file), handler);
                }
            }finally{
                rdfHandler.EndRdf(true);
            }
        }

        private TextWriter OpenFile(string file, bool compressed)
        {
            Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            if(compressed)
            {
                stream = new GZipStream(stream, CompressionLevel.Optimal, false);
            }
            return new StreamWriter(stream);
        }

        private IRdfHandler CreateFileHandler(string output, out INamespaceMapper mapper, bool compressed)
        {
            var writer = OpenFile(output, compressed);
            var qnameMapper = new QNameOutputMapper();
            var formatter = new TurtleFormatter(qnameMapper);
            IRdfHandler handler = new VDS.RDF.Parsing.Handlers.WriteThroughHandler(formatter, writer, true);
            handler = new NamespaceHandler(handler, qnameMapper);
            mapper = qnameMapper;
            return handler;
        }

        private IRdfHandler CreateGraphHandler(out Graph graph)
        {
            graph = new Graph();
            return new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
        }

        private void SetDefaultNamespaces(INamespaceMapper mapper)
        {
            foreach(var hash in DataAnalyzer.HashAlgorithms)
            {
                if(hash.FormattingMethod != FormattingMethod.Base64)
                {
                    mapper.AddNamespace(hash.Name, hash[Array.Empty<byte>()]);
                }
            }
            foreach(var hash in FileAnalyzer.HashAlgorithms)
            {
                if(hash.FormattingMethod != FormattingMethod.Base64)
                {
                    mapper.AddNamespace(hash.Name, hash[Array.Empty<byte>()]);
                }
            }
            mapper.AddNamespace("id", new Uri(root + "/"));
            mapper.AddNamespace("dtxt", new Uri("data:,"));
            mapper.AddNamespace("dt64", new Uri("data:;base64,"));
            mapper.AddNamespace("dbin", new Uri("data:application/octet-stream,"));
            mapper.AddNamespace("db64", new Uri("data:application/octet-stream;base64,"));
            mapper.AddNamespace("exif", new Uri("http://www.w3.org/2003/12/exif/ns#"));
        }

        private void SaveGraph(Graph graph, string output, bool compressed)
        {
            var writer = new CompressingTurtleWriter(TurtleSyntax.Original);
            writer.PrettyPrintMode = false;
            writer.DefaultNamespaces.Clear();
            using(var textWriter = OpenFile(output, compressed))
            {
                graph.SaveToStream(textWriter, writer);
            }
        }

        sealed class NamespaceHandler : IRdfHandler
        {
            readonly IRdfHandler baseHandler;

            readonly QNameOutputMapper mapper;

            public NamespaceHandler(IRdfHandler baseHandler, QNameOutputMapper mapper)
            {
                this.baseHandler = baseHandler;
                this.mapper = mapper;
            }

            public bool HandleNamespace(string prefix, Uri namespaceUri)
            {
                mapper.AddNamespace(prefix, namespaceUri);
                return baseHandler.HandleNamespace(prefix, namespaceUri);
            }

            #region Implementation
            public bool AcceptsAll => baseHandler.AcceptsAll;

            public IBlankNode CreateBlankNode()
            {
                return baseHandler.CreateBlankNode();
            }

            public IBlankNode CreateBlankNode(string nodeId)
            {
                return baseHandler.CreateBlankNode(nodeId);
            }

            public IGraphLiteralNode CreateGraphLiteralNode()
            {
                return baseHandler.CreateGraphLiteralNode();
            }

            public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
            {
                return baseHandler.CreateGraphLiteralNode(subgraph);
            }

            public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
            {
                return baseHandler.CreateLiteralNode(literal, datatype);
            }

            public ILiteralNode CreateLiteralNode(string literal)
            {
                return baseHandler.CreateLiteralNode(literal);
            }

            public ILiteralNode CreateLiteralNode(string literal, string langspec)
            {
                return baseHandler.CreateLiteralNode(literal, langspec);
            }

            public IUriNode CreateUriNode(Uri uri)
            {
                return baseHandler.CreateUriNode(uri);
            }

            public IVariableNode CreateVariableNode(string varname)
            {
                return baseHandler.CreateVariableNode(varname);
            }

            public void EndRdf(bool ok)
            {
                baseHandler.EndRdf(ok);
            }

            public string GetNextBlankNodeID()
            {
                return baseHandler.GetNextBlankNodeID();
            }

            public bool HandleBaseUri(Uri baseUri)
            {
                return baseHandler.HandleBaseUri(baseUri);
            }

            public bool HandleTriple(Triple t)
            {
                return baseHandler.HandleTriple(t);
            }

            public void StartRdf()
            {
                baseHandler.StartRdf();
            }
            #endregion
        }
    }
}
