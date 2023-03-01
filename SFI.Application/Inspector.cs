using IS4.SFI.Analyzers;
using IS4.SFI.Extensions;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace IS4.SFI
{
    /// <summary>
    /// Provides the support for describing input files
    /// and configuring analyzer components.
    /// </summary>
    public abstract class Inspector : EntityAnalyzerProvider
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
        /// Creates a new instance of the inspector and initializes several
        /// core analyzers.
        /// </summary>
        public Inspector()
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
            DataAnalyzer.HashAlgorithms.Add(BuiltInNonCryptographicHash.CRC32);
            DataAnalyzer.HashAlgorithms.Add(BuiltInNonCryptographicHash.CRC64);
            DataAnalyzer.HashAlgorithms.Add(BuiltInNonCryptographicHash.XXH32);
            DataAnalyzer.HashAlgorithms.Add(BuiltInNonCryptographicHash.XXH64);

            FileAnalyzer.HashAlgorithms.Add(BitTorrentHash = new BitTorrentHash());
        }

        /// <summary>
        /// Adds the default formats and analyzers.
        /// </summary>
        public virtual async ValueTask AddDefault()
        {
            DataAnalyzer.DataFormats.Add(new XmlFileFormat());
            DataAnalyzer.DataFormats.Add(new X509CertificateFormat());
        }

        static Inspector()
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
        public async ValueTask Inspect(string file, string output, InspectorOptions options)
        {
            if((File.GetAttributes(file) & FileAttributes.Directory) != 0)
            {
                await Inspect(new DirectoryInfo(file), output, options);
            }else{
                await Inspect(new FileInfo(file), output, options);
            }
        }

        /// <summary>
        /// Describes an entity.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to describe.</param>
        /// <param name="output">The output file where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Inspect<T>(T entity, string output, InspectorOptions options) where T : class
        {
            return Inspect(new[] { entity }, _ => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        /// <summary>
        /// Describes an entity.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to describe.</param>
        /// <param name="output">The output stream where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Inspect<T>(T entity, Stream output, InspectorOptions options) where T : class
        {
            return Inspect(new[] { entity }, _ => output, options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="output">The output file where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Inspect<T>(IEnumerable<T> entities, string output, InspectorOptions options) where T : class
        {
            return Inspect(entities, _ => new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read), options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="output">The output stream where to store the RDF description.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Inspect<T>(IEnumerable<T> entities, Stream output, InspectorOptions options) where T : class
        {
            return Inspect(entities, _ => output, options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="outputFactory">A function producing the output stream where to store the RDF description, with the format MIME type as the argument.</param>
        /// <param name="options">Additional options.</param>
        public async ValueTask Inspect<T>(IEnumerable<T> entities, Func<string, Stream> outputFactory, InspectorOptions options) where T : class
        {
            if(options == null) throw new ArgumentNullException(nameof(options));

            // Each custom graph can have a specific handler
            var graphHandlers = new Dictionary<Uri, IRdfHandler?>(new UriComparer());

            if(options.HideMetadata)
            {
                graphHandlers[new Uri(Graphs.Metadata.Value)] = null;
            }

            graphHandlers[new Uri(Graphs.ShortenedLinks.Value)] = null;

            // Loads SPARQL queries to evaluate on the RDF
            var queries = GetQueries(options);

            var formatStr = options.Format ?? MimeTypesHelper.DefaultTurtleExtension;
            var format = MimeTypesHelper.GetDefinitionsByFileExtension(formatStr).Concat(MimeTypesHelper.GetDefinitions(formatStr)).FirstOrDefault(f => f.CanWriteRdf || f.CanWriteRdfDatasets);
            if(format == null)
            {
                throw new ApplicationException($"Format '{options.Format}' is not recognized or could not be used for writing!");
            }
            OutputLog.WriteLine($"Using {format.SyntaxName + (options.CompressedOutput ? " (compressed)" : "")} for output.");

            if(options.DirectOutput)
            {
                // The data is immediately saved to output without an intermediate storage
                OutputLog.WriteLine("Writing data...");

                using var disposable = CreateFileHandler(() => outputFactory(GetOutputMimeType(format)), out var mapper, options, format, out var handler);
                Graph? queryGraph = null;
                if(queries.Count > 0)
                {
                    handler = new TemporaryGraphHandler(handler, out queryGraph);
                }

                SetDefaultNamespaces(mapper);

                NodeQueryTester? tester = null;
                if(queryGraph != null)
                {
                    tester = new NodeQueryTester(handler, queryGraph, queries);
                }

                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, mapper, tester, options);
                }
            }else{
                // The data is first stored in a graph and then saved
                OutputLog.WriteLine("Reading data...");

                var handler = CreateGraphHandler(out var graph);

                SetDefaultNamespaces(graph.NamespaceMap);

                NodeQueryTester? tester = null;
                if(queries.Count > 0)
                {
                    tester = new NodeQueryTester(handler, graph, queries);
                }

                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null, tester, options);
                }

                OutputLog.WriteLine("Saving...");

                SaveGraph(graph, () => outputFactory(GetOutputMimeType(format)), options, format);
            }

            OutputLog.WriteLine("Done!");
        }

        private string GetOutputMimeType(MimeTypeDefinition mime)
        {
            var type = mime.CanonicalMimeType;
            if(type.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                type += ";charset=utf-8";
            }
            return type;
        }

        /// <summary>
        /// Loads SPARQL queries from options.
        /// </summary>
        private IReadOnlyCollection<SparqlQuery> GetQueries(InspectorOptions options)
        {
            SparqlQueryParser? queryParser = null;
            var results = new List<SparqlQuery>();
            foreach(var file in options.Queries)
            {
                if(queryParser == null)
                {
                    queryParser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
                    queryParser.DefaultBaseUri = new Uri(options.Root, UriKind.Absolute);
                    queryParser.Warning += OutputLog.WriteLine;
                }
                SparqlQuery query;
                try{
                    using var stream = file.Open();
                    using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
                    query = queryParser.Parse(reader);
                }catch(Exception e)
                {
                    throw new ApplicationException($"Error while parsing SPARQL query in {file.Name}: {e.Message}", e);
                }
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
        private async ValueTask<AnalysisResult> AnalyzeEntity<T>(T entity, IRdfHandler rdfHandler, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, INamespaceMapper? mapper, NodeQueryTester? queryTester, InspectorOptions options) where T : class
        {
            // The node factory/handler
            var handler = new LinkedNodeHandler(new UriTools.PrefixFormatter(options.Root), rdfHandler, graphHandlers, queryTester, options.SimplifyBlankNodes);
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
                return await this.Analyze(entity, AnalysisContext.Create(node, handler));
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
        private TextWriter OpenFile(Func<Stream> fileFactory, bool compressed, InspectorOptions options)
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
        /// <param name="format">The format to use for writing.</param>
        /// <param name="handler">A variable that receives the RDF handler to use.</param>
        /// <returns>An instance of <see cref="IDisposable"/> representing the open file.</returns>
        private IDisposable CreateFileHandler(Func<Stream> outputFactory, out INamespaceMapper mapper, InspectorOptions options, MimeTypeDefinition format, out IRdfHandler handler)
        {
            var writer = OpenFile(outputFactory, options.CompressedOutput, options);
            var qnameMapper = new QNameOutputMapper();
            var rdfWriter = format.CanWriteRdf ? format.GetRdfWriter() as IFormatterBasedWriter : null;
            if(rdfWriter == null)
            {
                throw new ApplicationException($"Format {format.SyntaxName} does not support direct output!");
            }
            var formatter = CreateFormatter(format.SyntaxName, rdfWriter.TripleFormatterType, qnameMapper);
            if(options.PrettyPrint && format.CanonicalMimeType == "text/turtle" && formatter is TurtleFormatter turtleFormatter)
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
        /// Creates a new instance of <see cref="ITripleFormatter"/> from the corresponding
        /// arguments.
        /// </summary>
        /// <param name="name">The name of the format, for diagnostics.</param>
        /// <param name="formatterType">The type of the formatter.</param>
        /// <param name="mapper">The namespace mapper to provide to the formatter.</param>
        /// <returns>A new instance of the formatter.</returns>
        private ITripleFormatter CreateFormatter(string name, Type formatterType, QNameOutputMapper mapper)
        {
            try{
                return (ITripleFormatter)Activator.CreateInstance(formatterType, mapper);
            }catch(MissingMethodException)
            {
                try{
                    return (ITripleFormatter)Activator.CreateInstance(formatterType);
                }catch(MissingMethodException e)
                {
                    throw new ApplicationException($"Formatter for {name} could not be constructed!", e);
                }catch(TargetInvocationException e)
                {
                    ExceptionDispatchInfo.Capture(e).Throw();
                    throw;
                }
            }catch(TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        /// <summary>
        /// Creates an RDF handler for asserting triples into a graph.
        /// </summary>
        /// <param name="graph">The variable that receives the created graph.</param>
        /// <returns>The RDF handler to use.</returns>
        private IRdfHandler CreateGraphHandler(out Graph graph)
        {
            graph = new Graph(true);
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
        /// <param name="format">The format to use for writing.</param>
        private void SaveGraph(Graph graph, Func<Stream> outputFactory, InspectorOptions options, MimeTypeDefinition format)
        {
            using var textWriter = OpenFile(outputFactory, options.CompressedOutput, options);
            if(format.CanWriteRdf)
            {
                var writer = format.GetRdfWriter();
                ConfigureWriter(writer, options);
                graph.SaveToStream(textWriter, writer);
            }else if(format.CanWriteRdfDatasets)
            {
                var writer = format.GetRdfDatasetWriter();
                ConfigureWriter(writer, options);
                graph.SaveToStream(textWriter, writer);
            }else{
                throw new ApplicationException($"The format {format.SyntaxName} cannot be used for writing!");
            }
        }

        /// <summary>
        /// Configures an arbitrary writer instance based on the supplied options.
        /// </summary>
        /// <param name="writer">The RDF writer.</param>
        /// <param name="options">The options.</param>
        private void ConfigureWriter(object writer, InspectorOptions options)
        {
            if(writer is IPrettyPrintingWriter prettyWriter)
            {
                prettyWriter.PrettyPrintMode = options.PrettyPrint;
            }
            if(writer is INamespaceWriter namespaceWriter)
            {
                namespaceWriter.DefaultNamespaces.Clear();
            }
        }
    }
}
