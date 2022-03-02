using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Extensions;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.MultiArchiver
{
    public class Archiver : EntityAnalyzerProvider
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
            Analyzers.Add(new DataObjectAnalyzer());
            Analyzers.Add(XmlAnalyzer = new XmlAnalyzer());
            Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());
            Analyzers.Add(new X509CertificateAnalyzer());
            Analyzers.Add(new FormatObjectAnalyzer());

            if(BuiltInHash.MD5 != null)
            {
                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5);
            }
            if(BuiltInHash.SHA1 != null)
            {
                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA1);
            }
            if(BitTorrentHash.HashAlgorithm != null)
            {
                DataAnalyzer.HashAlgorithms.Add(new PaddedBlockHash(Vocabulary.Individuals.BSHA1_256, "urn:bsha1-256:", 262144));
            }
            DataAnalyzer.HashAlgorithms.Add(Blake3Hash.Instance);
            DataAnalyzer.HashAlgorithms.Add(Crc32Hash.Instance);
            DataAnalyzer.ContentUriFormatter = new AdHashedContentUriFormatter(Blake3Hash.Instance);

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

        public static Archiver CreateDefault(bool analysis = true)
        {
            var archiver = new Archiver();

            archiver.DataAnalyzer.DataFormats.Add(new XmlFileFormat());
            archiver.DataAnalyzer.DataFormats.Add(new ZipFormat());
            archiver.DataAnalyzer.DataFormats.Add(new RarFormat());
            archiver.DataAnalyzer.DataFormats.Add(new SevenZipFormat());
            archiver.DataAnalyzer.DataFormats.Add(new GZipFormat());
            archiver.DataAnalyzer.DataFormats.Add(new TarFormat());
            archiver.DataAnalyzer.DataFormats.Add(new SzFormat());
            archiver.DataAnalyzer.DataFormats.Add(new ImageMetadataFormat());
            archiver.DataAnalyzer.DataFormats.Add(new ImageFormat());
            archiver.DataAnalyzer.DataFormats.Add(new TagLibFormat());
            archiver.DataAnalyzer.DataFormats.Add(new IsoFormat());
            archiver.DataAnalyzer.DataFormats.Add(new DosModuleFormat());
            archiver.DataAnalyzer.DataFormats.Add(new GenericModuleFormat());
            archiver.DataAnalyzer.DataFormats.Add(new LinearModuleFormat());
            archiver.DataAnalyzer.DataFormats.Add(new Win16ModuleFormat());
            //archiver.DataAnalyzer.Formats.Add(new Win32ModuleFormat());
            archiver.DataAnalyzer.DataFormats.Add(new Win32ModuleFormatManaged());
            archiver.DataAnalyzer.DataFormats.Add(new WaveFormat());
            //archiver.DataAnalyzer.Formats.Add(new OggFormat());
            archiver.DataAnalyzer.DataFormats.Add(new WasapiFormat(false));
            archiver.DataAnalyzer.DataFormats.Add(new WasapiFormat(true));
            archiver.DataAnalyzer.DataFormats.Add(new DelphiFormFormat());
            archiver.DataAnalyzer.DataFormats.Add(new CabinetFormat());
            archiver.DataAnalyzer.DataFormats.Add(new OleStorageFormat());
            archiver.DataAnalyzer.DataFormats.Add(new X509CertificateFormat());

            archiver.ContainerProviders.Add(new OpenPackageFormat());
            archiver.ContainerProviders.Add(new PackageDescriptionFormat());
            archiver.ContainerProviders.Add(new ExcelXmlDocumentFormat());
            archiver.ContainerProviders.Add(new ExcelDocumentFormat());
            archiver.ContainerProviders.Add(new WordXmlDocumentFormat());

            archiver.XmlAnalyzer.XmlFormats.Add(new SvgFormat());
            archiver.XmlAnalyzer.XmlFormats.Add(new RdfXmlFormat());

            archiver.Analyzers.Add(new ArchiveAnalyzer());
            archiver.Analyzers.Add(new ArchiveReaderAnalyzer());
            archiver.Analyzers.Add(new FileSystemAnalyzer());
            archiver.Analyzers.Add(ImageMetadataAnalyzer.CreateDefault());
            archiver.Analyzers.Add(new TagLibAnalyzer());
            archiver.Analyzers.Add(new DosModuleAnalyzer());
            archiver.Analyzers.Add(new WinModuleAnalyzer());
            //archiver.Analyzers.Add(new WinVersionAnalyzer());
            archiver.Analyzers.Add(new WinVersionAnalyzerManaged());
            archiver.Analyzers.Add(new SvgAnalyzer());
            archiver.Analyzers.Add(new WaveAnalyzer());
            archiver.Analyzers.Add(new DelphiObjectAnalyzer());
            archiver.Analyzers.Add(new CabinetAnalyzer());
            archiver.Analyzers.Add(new OleStorageAnalyzer());
            archiver.Analyzers.Add(new PackageDescriptionAnalyzer());
            archiver.Analyzers.Add(new RdfXmlAnalyzer());

            return archiver;
        }

        public ValueTask Archive(string file, string output, ArchiverOptions options)
        {
            if((File.GetAttributes(file) & FileAttributes.Directory) != 0)
            {
                return Archive(new DirectoryInfo(file), output, options);
            }else{
                return Archive(new FileInfo(file), output, options);
            }
        }

        public ValueTask Archive<T>(T entity, string output, ArchiverOptions options) where T : class
        {
            return Archive(new[] { entity }, () => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        public ValueTask Archive<T>(T entity, Stream output, ArchiverOptions options) where T : class
        {
            return Archive(new[] { entity }, () => output, options);
        }

        public ValueTask Archive<T>(IEnumerable<T> entities, string output, ArchiverOptions options) where T : class
        {
            return Archive(entities, () => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        public ValueTask Archive<T>(IEnumerable<T> entities, Stream output, ArchiverOptions options) where T : class
        {
            return Archive(entities, () => output, options);
        }

        public async ValueTask Archive<T>(IEnumerable<T> entities, Func<Stream> outputFactory, ArchiverOptions options) where T : class
        {
            if(options == null) throw new ArgumentNullException(nameof(options));

            var graphHandlers = new Dictionary<Uri, IRdfHandler>(new UriComparer());

            if(options.HideMetadata)
            {
                graphHandlers[new Uri(Graphs.Metadata.Value)] = new VDS.RDF.Parsing.Handlers.NullHandler();
            }

            if(options.DirectOutput)
            {
                OutputLog.WriteLine("Writing data...");

                var handler = CreateFileHandler(outputFactory, out var mapper, options.CompressedOutput);

                SetDefaultNamespaces(mapper);

                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, mapper);
                }
            }else{
                OutputLog.WriteLine("Reading data...");

                var handler = CreateGraphHandler(out var graph);

                SetDefaultNamespaces(graph.NamespaceMap);
                
                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null);
                }

                OutputLog.WriteLine("Saving...");

                SaveGraph(graph, outputFactory, options.CompressedOutput);
            }
        }

        const string root = "http://archive.data.is4.site/.well-known/genid";

        private async ValueTask<AnalysisResult> AnalyzeEntity<T>(T entity, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, INamespaceMapper mapper) where T : class
        {
            var handler = new RdfHandler(new Uri(root), rdfHandler, graphHandlers);
            rdfHandler.StartRdf();
            foreach(var graphHandler in graphHandlers)
            {
                graphHandler.Value?.StartRdf();
            }
            try{
                if(mapper != null)
                {
                    foreach(var prefix in mapper.Prefixes)
                    {
                        rdfHandler.HandleNamespace(prefix, mapper.GetNamespaceUri(prefix));
                    }
                }

                return await this.Analyze(entity, new AnalysisContext(nodeFactory: handler));
            }finally{
                rdfHandler.EndRdf(true);
                foreach(var graphHandler in graphHandlers)
                {
                    graphHandler.Value?.EndRdf(true);
                }
            }
        }

        private TextWriter OpenFile(Func<Stream> fileFactory, bool compressed)
        {
            var stream = fileFactory();
            if(compressed)
            {
                stream = new GZipStream(stream, CompressionLevel.Optimal, false);
            }
            return new StreamWriter(stream);
        }

        private IRdfHandler CreateFileHandler(Func<Stream> outputFactory, out INamespaceMapper mapper, bool compressed)
        {
            var writer = OpenFile(outputFactory, compressed);
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
                    mapper.AddNamespace(hash.Name, hash[new ArraySegment<byte>(Array.Empty<byte>())]);
                }
            }
            foreach(var hash in FileAnalyzer.HashAlgorithms)
            {
                if(hash.FormattingMethod != FormattingMethod.Base64)
                {
                    mapper.AddNamespace(hash.Name, hash[new ArraySegment<byte>(Array.Empty<byte>())]);
                }
            }
            mapper.AddNamespace("id", new Uri(root + "/"));
            mapper.AddNamespace("dtxt", new Uri("data:,"));
            mapper.AddNamespace("dt64", new Uri("data:;base64,"));
            mapper.AddNamespace("dbin", new Uri("data:application/octet-stream,"));
            mapper.AddNamespace("db64", new Uri("data:application/octet-stream;base64,"));
            mapper.AddNamespace("exif", new Uri("http://www.w3.org/2003/12/exif/ns#"));
        }

        private void SaveGraph(Graph graph, Func<Stream> outputFactory, bool compressed)
        {
            var writer = new CompressingTurtleWriter(TurtleSyntax.Original);
            writer.PrettyPrintMode = false;
            writer.DefaultNamespaces.Clear();
            using(var textWriter = OpenFile(outputFactory, compressed))
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
