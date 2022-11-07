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

        /// <inheritdoc cref="EntityAnalyzerProvider.OutputLog"/>
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
        /// Provides the collection of image hash algorithms used by the
        /// default image analyzer, if there is any.
        /// </summary>
        public virtual ICollection<IHashAlgorithm> ImageHashAlgorithms => Array.Empty<IHashAlgorithm>();

        /// <summary>
        /// Creates a new instance of the archiver and initializes several
        /// core analyzers.
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
        public async ValueTask Archive(string file, string output, ArchiverOptions options)
        {
            if((File.GetAttributes(file) & FileAttributes.Directory) != 0)
            {
                await Archive(new DirectoryInfo(file), output, options);
            }else{
                await Archive(new FileInfo(file), output, options);
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
                graphHandlers[new Uri(Graphs.Metadata.Value)] = null;
            }

            graphHandlers[new Uri(Graphs.ShortenedLinks.Value)] = null;

            // Loads SPARQL queries to evaluate on the RDF
            var queries = GetQueries(options);

            if(options.DirectOutput)
            {
                // The data is immediately saved to output without an intermediate storage
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
                // The data is first stored in a graph and then saved
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

            OutputLog.WriteLine("Done!");
        }

        /// <summary>
        /// Loads SPARQL queries from options.
        /// </summary>
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
        /// <summary>
        /// Analyzes a single entity.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="rdfHandler">The RDF handler to receive the description of the entity.</param>
        /// <param name="graphHandlers">A collection of RDF handlers for other graphs.</param>
        /// <param name="mapper">The mapper storing default namespaces to add to <paramref name="rdfHandler"/>..</param>
        /// <param name="queryTester">An instance of <see cref="NodeQueryTester"/> to match nodes using user-provided queries.</param>
        /// <param name="options">Additional options.</param>
        /// <returns>The result of the analysis.</returns>
        private async ValueTask<AnalysisResult> AnalyzeEntity<T>(T entity, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler> graphHandlers, INamespaceMapper mapper, NodeQueryTester queryTester, ArchiverOptions options) where T : class
        {
            // The node factory/handler
            var handler = new LinkedNodeHandler(new UriTools.PrefixFormatter(options.Root), rdfHandler, graphHandlers, queryTester);
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

        /// <summary>
        /// Opens an output file as text.
        /// </summary>
        /// <param name="fileFactory">The function to provide the output stream.</param>
        /// <param name="compressed">Whether to compress the output with gzip.</param>
        /// <param name="options">Additional options.</param>
        /// <returns>Text writer to the output file.</returns>
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

        /// <summary>
        /// Creates an RDF handler for writing directly to the output file.
        /// </summary>
        /// <param name="outputFactory">The function to provide the output stream.</param>
        /// <param name="mapper">A variable that receives the instance of <see cref="INamespaceMapper"/> representing the namespaces in use by the handler.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="handler">A variable that receives the RDF handler to use.</param>
        /// <returns>An instance of <see cref="IDisposable"/> representing the open file.</returns>
        private IDisposable CreateFileHandler(Func<Stream> outputFactory, out INamespaceMapper mapper, ArchiverOptions options, out IRdfHandler handler)
        {
            var writer = OpenFile(outputFactory, options.CompressedOutput, options);
            var qnameMapper = new QNameOutputMapper();
            //TODO: Support for other output formats
            var formatter = new TurtleFormatter(qnameMapper);
            if(options.PrettyPrint && formatter is TurtleFormatter turtleFormatter)
            {
                // Use the custom Turtle handler with @base support
                handler = new TurtleHandler<TurtleFormatter>(writer, turtleFormatter, qnameMapper);
            }else{
                handler = new VDS.RDF.Parsing.Handlers.WriteThroughHandler(formatter, writer, true);
                handler = new NamespaceHandler(handler, qnameMapper);
            }
            mapper = qnameMapper;
            return writer;
        }

        /// <summary>
        /// Creates an RDF handler for asserting triples into a graph.
        /// </summary>
        /// <param name="graph">The variable that receives the created graph.</param>
        /// <returns>The RDF handler to use.</returns>
        private IRdfHandler CreateGraphHandler(out Graph graph)
        {
            graph = new Graph();
            return new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
        }

        /// <summary>
        /// Registers default namespaces that are always in use.
        /// </summary>
        /// <param name="mapper">The mapper that receives the namespaces.</param>
        protected virtual void SetDefaultNamespaces(INamespaceMapper mapper)
        {
            /*
            // These namespaces are better unused since some formatters don't escape invalid characters correctly
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
            mapper.AddNamespace("id", new Uri(Root + "/"));
            mapper.AddNamespace("dtxt", new Uri("data:,"));
            mapper.AddNamespace("dt64", new Uri("data:;base64,"));
            mapper.AddNamespace("dbin", new Uri("data:application/octet-stream,"));
            mapper.AddNamespace("db64", new Uri("data:application/octet-stream;base64,"));
            */
        }

        /// <summary>
        /// Saves a graph to the output.
        /// </summary>
        /// <param name="graph">The graph to save.</param>
        /// <param name="outputFactory">The function to provide the output stream.</param>
        /// <param name="options">Additional options.</param>
        private void SaveGraph(Graph graph, Func<Stream> outputFactory, ArchiverOptions options)
        {
            //TODO: Support for other output formats
            var writer = new CompressingTurtleWriter(TurtleSyntax.Original);
            writer.PrettyPrintMode = options.PrettyPrint;
            writer.DefaultNamespaces.Clear();
            using(var textWriter = OpenFile(outputFactory, options.CompressedOutput, options))
            {
                graph.SaveToStream(textWriter, writer);
            }
        }

        /// <summary>
        /// A wrapper collection used to implement members such as <see cref="ImageHashAlgorithms"/>
        /// using a general type while operating on a collection with a more specific type.
        /// </summary>
        /// <typeparam name="TGeneral">
        /// The general element type, affecting the concrete type of <see cref="ICollection{T}"/>
        /// implemented by this type.
        /// </typeparam>
        /// <typeparam name="TSpecific">
        /// The specific element type, determining the type of the underlying instance of <see cref="ICollection{T}"/>.
        /// </typeparam>
        protected class ConcreteCollectionWrapper<TGeneral, TSpecific>
            : ICollection<TGeneral>, IPersistentKey
            where TSpecific : TGeneral
        {
            readonly ICollection<TSpecific> inner;

            /// <summary>
            /// Creates a new instance of the wrapper.
            /// </summary>
            /// <param name="inner">The underlying collection.</param>
            public ConcreteCollectionWrapper(ICollection<TSpecific> inner)
            {
                this.inner = inner;
            }

            public int Count => inner.Count;

            public bool IsReadOnly => inner.IsReadOnly;

            object IPersistentKey.ReferenceKey => inner;

            object IPersistentKey.DataKey => null;

            public void Add(TGeneral item)
            {
                inner.Add((TSpecific)item);
            }

            public void Clear()
            {
                inner.Clear();
            }

            public bool Contains(TGeneral item)
            {
                return item is TSpecific item2 && inner.Contains(item2);
            }

            public void CopyTo(TGeneral[] array, int arrayIndex)
            {
                foreach(var specific in inner)
                {
                    array[arrayIndex++] = specific;
                }
            }

            public IEnumerator<TGeneral> GetEnumerator()
            {
                foreach(var specific in inner) yield return specific;
            }

            public bool Remove(TGeneral item)
            {
                return item is TSpecific item2 && inner.Remove(item2);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return inner.GetEnumerator();
            }
        }
    }
}
