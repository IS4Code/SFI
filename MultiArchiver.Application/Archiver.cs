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
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Provides the support for describing input files
    /// and configuring analyzer components.
    /// </summary>
    public abstract class Archiver : EntityAnalyzerProvider
    {
        /// <summary>
        /// The default file analyzer.
        /// </summary>
        public FileAnalyzer FileAnalyzer { get; }

        /// <summary>
        /// The default data analyzer.
        /// </summary>
        public DataAnalyzer DataAnalyzer { get; }

        /// <summary>
        /// The default <see cref="IDataObject"/> analyzer.
        /// </summary>
        public DataObjectAnalyzer DataObjectAnalyzer { get; }

        /// <summary>
        /// The default XML analyzer.
        /// </summary>
        public XmlAnalyzer XmlAnalyzer { get; }

        /// <summary>
        /// The BitTorrent Info-hash algorithm.
        /// </summary>
        public BitTorrentHash BitTorrentHash { get; }

        /// <summary>
        /// An instance of <see cref="TextWriter"/> to use for logging.
        /// </summary>
        public new TextWriter OutputLog {
            get {
                return base.OutputLog;
            }
            set {
                base.OutputLog = value;
                DataAnalyzer.OutputLog = value;
            }
        }

        /// <summary>
        /// The media type assigned to the output RDF file.
        /// </summary>
        public virtual string OutputMediaType => "text/turtle;charset=utf-8";

        /// <summary>
        /// Provides the collection of data hash algorithms used by the
        /// default image analyzer, if there is any.
        /// </summary>
        public virtual ICollection<IDataHashAlgorithm> ImageDataHashAlgorithms => Array.Empty<IDataHashAlgorithm>();

        /// <summary>
        /// Creates a new instance of the archiver and adds several
        /// default analyzers.
        /// </summary>
        public Archiver()
        {
            Analyzers.Add(FileAnalyzer = new FileAnalyzer());
            Analyzers.Add(DataAnalyzer = new DataAnalyzer(() => new UdeEncodingDetector()));
            Analyzers.Add(DataObjectAnalyzer = new DataObjectAnalyzer());
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
            if(BuiltInHash.SHA256 != null)
            {
                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA256);
            }
            if(BuiltInHash.SHA384 != null)
            {
                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA384);
            }
            if(BuiltInHash.SHA512 != null)
            {
                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.SHA512);
            }
            if(BitTorrentHash.HashAlgorithm != null)
            {
                DataAnalyzer.HashAlgorithms.Add(new PaddedBlockHash(Individuals.BSHA1_256, "urn:bsha1-256:", 262144));
            }
            DataAnalyzer.HashAlgorithms.Add(Crc32Hash.Instance);

            FileAnalyzer.HashAlgorithms.Add(BitTorrentHash = new BitTorrentHash());
        }

        /// <summary>
        /// Adds the default formats and analyzers.
        /// </summary>
        public virtual void AddDefault()
        {
            DataAnalyzer.DataFormats.Add(new XmlFileFormat());
            DataAnalyzer.DataFormats.Add(new X509CertificateFormat());

            XmlAnalyzer.XmlFormats.Add(new RdfXmlFormat());

            Analyzers.Add(new RdfXmlAnalyzer());
        }

        static Archiver()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Not desirable due to EncodedUri and similar; vocabulary URIs are cached anyway
            Options.InternUris = false;
        }

        /// <summary>
        /// Describes a file on the local drive.
        /// </summary>
        /// <param name="file">The input file to describe.</param>
        /// <param name="output">The output file where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Archive(string file, string output, ArchiverOptions options)
        {
            if((File.GetAttributes(file) & FileAttributes.Directory) != 0)
            {
                return Archive(new DirectoryInfo(file), output, options);
            }else{
                return Archive(new FileInfo(file), output, options);
            }
        }

        /// <summary>
        /// Describes an entity.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to describe.</param>
        /// <param name="output">The output file where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Archive<T>(T entity, string output, ArchiverOptions options) where T : class
        {
            return Archive(new[] { entity }, () => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        /// <summary>
        /// Describes an entity.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to describe.</param>
        /// <param name="output">The output stream where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Archive<T>(T entity, Stream output, ArchiverOptions options) where T : class
        {
            return Archive(new[] { entity }, () => output, options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="output">The output file where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Archive<T>(IEnumerable<T> entities, string output, ArchiverOptions options) where T : class
        {
            return Archive(entities, () => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="output">The output stream where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Archive<T>(IEnumerable<T> entities, Stream output, ArchiverOptions options) where T : class
        {
            return Archive(entities, () => output, options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="outputFactory">A function producing the output stream where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public async ValueTask Archive<T>(IEnumerable<T> entities, Func<Stream> outputFactory, ArchiverOptions options) where T : class
        {
            if(options == null) throw new ArgumentNullException(nameof(options));

            // Each custom graph can have a specific handler
            var graphHandlers = new Dictionary<Uri, IRdfHandler>(new UriComparer());

            if(options.HideMetadata)
            {
                graphHandlers[new Uri(Graphs.Metadata.Value)] = new VDS.RDF.Parsing.Handlers.NullHandler();
            }

            graphHandlers[new Uri(Graphs.ShortenedLinks.Value)] = null;

            var queries = GetQueries(options);

            if(options.DirectOutput)
            {
                OutputLog.WriteLine("Writing data...");

                using(var disposable = CreateFileHandler(outputFactory, out var mapper, options, out var handler))
                {
                    Graph queryGraph = null;
                    if(queries.Count > 0)
                    {
                        handler = new TemporaryGraphHandler(handler, out queryGraph);
                    }

                    SetDefaultNamespaces(mapper);

                    NodeQueryTester tester = null;
                    if(queryGraph != null)
                    {
                        tester = new NodeQueryTester(handler, queryGraph, queries);
                    }

                    foreach(var entity in entities)
                    {
                        await AnalyzeEntity(entity, handler, graphHandlers, mapper, tester, options);
                    }
                }
            }else{
                OutputLog.WriteLine("Reading data...");

                var handler = CreateGraphHandler(out var graph);

                SetDefaultNamespaces(graph.NamespaceMap);

                NodeQueryTester tester = null;
                if(queries.Count > 0)
                {
                    tester = new NodeQueryTester(handler, graph, queries);
                }

                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null, tester, options);
                }

                OutputLog.WriteLine("Saving...");

                SaveGraph(graph, outputFactory, options);
            }
        }

        private IReadOnlyCollection<SparqlQuery> GetQueries(ArchiverOptions options)
        {
            SparqlQueryParser queryParser = null;
            var results = new List<SparqlQuery>();
            foreach(var file in options.Queries)
            {
                if(queryParser == null)
                {
                    queryParser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
                    queryParser.DefaultBaseUri = new Uri(options.Root, UriKind.Absolute);
                    queryParser.Warning += OutputLog.WriteLine;
                }
                using(var stream = file.Open())
                {
                    using(var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
                    {
                        var query = queryParser.Parse(reader);
                        switch(query.QueryType)
                        {
                            case SparqlQueryType.Construct:
                            case SparqlQueryType.Select:
                            case SparqlQueryType.SelectAll:
                            case SparqlQueryType.SelectAllDistinct:
                            case SparqlQueryType.SelectDistinct:
                            case SparqlQueryType.SelectReduced:
                                break;
                            default:
                                throw new ApplicationException($"Query in {file.Name} has an unsupported type ({query.QueryType}), only SELECT or CONSTRUCT queries are allowed.");
                        }
                        results.Add(query);
                    }
                }
            }
            return results;
        }

        private async ValueTask<AnalysisResult> AnalyzeEntity<T>(T entity, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, INamespaceMapper mapper, NodeQueryTester queryTester, ArchiverOptions options) where T : class
        {
            var handler = new RdfHandler(new UriTools.PrefixFormatter(options.Root), rdfHandler, graphHandlers, queryTester);
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

        private TextWriter OpenFile(Func<Stream> fileFactory, bool compressed, ArchiverOptions options)
        {
            var stream = fileFactory();
            if(compressed)
            {
                stream = new GZipStream(stream, CompressionLevel.Optimal, false);
            }
            return new StreamWriter(stream)
            {
                NewLine = options.NewLine
            };
        }

        private IDisposable CreateFileHandler(Func<Stream> outputFactory, out INamespaceMapper mapper, ArchiverOptions options, out IRdfHandler handler)
        {
            var writer = OpenFile(outputFactory, options.CompressedOutput, options);
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
            using(var textWriter = OpenFile(outputFactory, options.CompressedOutput, options))
            {
                graph.SaveToStream(textWriter, writer);
            }
        }
    }
}
