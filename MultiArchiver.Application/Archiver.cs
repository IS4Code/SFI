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
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.MultiArchiver
{
    public abstract class Archiver : EntityAnalyzerProvider
    {
        public FileAnalyzer FileAnalyzer { get; }
        public DataAnalyzer DataAnalyzer { get; }
        public XmlAnalyzer XmlAnalyzer { get; }
        public BitTorrentHash BitTorrentHash { get; }
        
        public new TextWriter OutputLog {
            get {
                return base.OutputLog;
            }
            set {
                base.OutputLog = value;
                DataAnalyzer.OutputLog = value;
            }
        }

        public Archiver()
        {
            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(() => new UdeEncodingDetector()));
            Analyzers.Add(new DataObjectAnalyzer());
            Analyzers.Add(XmlAnalyzer = new XmlAnalyzer());
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
        }

        public virtual void AddDefault()
        {
            DataAnalyzer.DataFormats.Add(new XmlFileFormat());
            DataAnalyzer.DataFormats.Add(new X509CertificateFormat());

            XmlAnalyzer.XmlFormats.Add(new RdfXmlFormat());

            Analyzers.Add(new RdfXmlAnalyzer());
        }

        static Archiver()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Options.InternUris = false;
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

                using(var disposable = CreateFileHandler(outputFactory, out var mapper, options, out var handler))
                {
                    SetDefaultNamespaces(mapper);

                    foreach(var entity in entities)
                    {
                        await AnalyzeEntity(entity, handler, graphHandlers, mapper, options);
                    }
                }
            }else{
                OutputLog.WriteLine("Reading data...");

                var handler = CreateGraphHandler(out var graph);

                SetDefaultNamespaces(graph.NamespaceMap);
                
                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null, options);
                }

                OutputLog.WriteLine("Saving...");

                SaveGraph(graph, outputFactory, options);
            }
        }

        private async ValueTask<AnalysisResult> AnalyzeEntity<T>(T entity, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, INamespaceMapper mapper, ArchiverOptions options) where T : class
        {
            var handler = new RdfHandler(new Uri(options.Root), rdfHandler, graphHandlers);
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

                var node = options.Node != null ? handler.Create(UriFormatter.Instance, options.Node) : null;
                return await this.Analyze(entity, new AnalysisContext(nodeFactory: handler, node: node));
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

        private IDisposable CreateFileHandler(Func<Stream> outputFactory, out INamespaceMapper mapper, ArchiverOptions options, out IRdfHandler handler)
        {
            var writer = OpenFile(outputFactory, options.CompressedOutput);
            var qnameMapper = new QNameOutputMapper();
            var formatter = new TurtleFormatter(qnameMapper);
            if(options.PrettyPrint)
            {
                handler = new TurtleHandler<TurtleFormatter>(writer, formatter, qnameMapper);
            }else{
                handler = new VDS.RDF.Parsing.Handlers.WriteThroughHandler(formatter, writer, true);
                handler = new NamespaceHandler(handler, qnameMapper);
            }
            mapper = qnameMapper;
            return writer;
        }

        private IRdfHandler CreateGraphHandler(out Graph graph)
        {
            graph = new Graph();
            return new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
        }

        private void SetDefaultNamespaces(INamespaceMapper mapper)
        {
            /*foreach(var hash in DataAnalyzer.HashAlgorithms)
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
            mapper.AddNamespace("id", new Uri(Root + "/"));
            mapper.AddNamespace("dtxt", new Uri("data:,"));
            mapper.AddNamespace("dt64", new Uri("data:;base64,"));
            mapper.AddNamespace("dbin", new Uri("data:application/octet-stream,"));
            mapper.AddNamespace("db64", new Uri("data:application/octet-stream;base64,"));*/
        }

        private void SaveGraph(Graph graph, Func<Stream> outputFactory, ArchiverOptions options)
        {
            var writer = new CompressingTurtleWriter(TurtleSyntax.Original);
            writer.PrettyPrintMode = options.PrettyPrint;
            writer.DefaultNamespaces.Clear();
            using(var textWriter = OpenFile(outputFactory, options.CompressedOutput))
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
