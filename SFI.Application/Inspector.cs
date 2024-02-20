using IS4.SFI.Analyzers;
using IS4.SFI.Application.Tools;
using IS4.SFI.RDF;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Vocabulary;
using Microsoft.Extensions.Logging;
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

namespace IS4.SFI.Application
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

        /// <inheritdoc cref="EntityAnalyzerProvider.OutputLog"/>
        public new ILogger? OutputLog {
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
            Analyzers.Add(new ExceptionAnalyzer());
            Analyzers.Add(new PathObjectAnalyzer());
            Analyzers.Add(new ExtensionObjectAnalyzer());
            Analyzers.Add(new MediaTypeAnalyzer());
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
        }

        /// <summary>
        /// Adds the default formats and analyzers.
        /// </summary>
        public virtual async ValueTask AddDefault()
        {

        }

        static Inspector()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Not desirable due to EncodedUri and similar; vocabulary URIs are cached anyway
            UriFactory.InternUris = false;

            // Custom writer of SPARQL Results as SPARQL query with VALUES
            MimeTypesHelper.RegisterWriter(new SparqlValuesQueryWriter(), new[] { MimeTypesHelper.SparqlQuery }, new[] { MimeTypesHelper.DefaultSparqlQueryExtension, "sparql" }.Distinct());
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
            return Inspect(new[] { entity }, _ => new(new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read)), options);
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
            return Inspect(new[] { entity }, _ => new(output), options);
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
            return Inspect(entities, _ => new(new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read)), options);
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
            return Inspect(entities, _ => new(output), options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="outputFactory">A function producing the output stream where to store the RDF description, with the format MIME type as the argument.</param>
        /// <param name="options">Additional options.</param>
        public ValueTask Inspect<T>(IEnumerable<T> entities, Func<string, Stream> outputFactory, InspectorOptions options) where T : class
        {
            return Inspect(entities, mime => new(outputFactory(mime)), options);
        }

        /// <summary>
        /// Describes a collection of entities.
        /// </summary>
        /// <typeparam name="T">The types of <paramref name="entities"/>.</typeparam>
        /// <param name="entities">The entities to describe.</param>
        /// <param name="outputFactory">A function producing the output stream where to store the RDF description, with the format MIME type as the argument.</param>
        /// <param name="options">Additional options.</param>
        public async ValueTask Inspect<T>(IEnumerable<T> entities, Func<string, ValueTask<Stream>> outputFactory, InspectorOptions options) where T : class
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
            var queries = await GetQueries(options);

            bool sparql = options.OutputIsSparqlResults;

            var formatStr = options.Format ?? (sparql ? MimeTypesHelper.DefaultSparqlXmlExtension : MimeTypesHelper.DefaultTurtleExtension);
            var format = MimeTypesHelper.GetDefinitionsByFileExtension(formatStr).Concat(MimeTypesHelper.GetDefinitions(formatStr)).FirstOrDefault(f => sparql ? f.CanWriteSparqlResults : (f.CanWriteRdf || f.CanWriteRdfDatasets));
            if(format == null)
            {
                throw new ApplicationException($"Format '{options.Format}' is not recognized or could not be used for writing!");
            }
            OutputLog?.LogInformation($"Using {format.SyntaxName + (options.CompressedOutput ? " (compressed)" : "")} for output.");

            Func<ValueTask<Stream>> formatOutputFactory = () => outputFactory(GetOutputMimeType(format));

            if(sparql)
            {
                if(options.DirectOutput)
                {
                    await WriteSparqlToOutput(entities, queries, graphHandlers, formatOutputFactory, format, options);
                }else{
                    await SaveSparqlToOutput(entities, queries, graphHandlers, formatOutputFactory, format, options);
                }
            }else{
                if(options.DirectOutput)
                {
                    await WriteRdfToOutput(entities, queries, graphHandlers, formatOutputFactory, format, options);
                }else{
                    await SaveRdfToOutput(entities, queries, graphHandlers, formatOutputFactory, format, options);
                }
            }

            OutputLog?.LogInformation("Done!");
        }

        /// <summary>
        /// Assigns configured properties to a newly created component.
        /// </summary>
        /// <param name="component">The created component.</param>
        protected virtual void ConfigureNewComponent(object component)
        {

        }

        /// <summary>
        /// Immediately saves the data to output without an intermediate storage.
        /// </summary>
        private async Task WriteRdfToOutput<T>(IEnumerable<T> entities, IReadOnlyCollection<SparqlQuery> queries, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, Func<ValueTask<Stream>> outputFactory, MimeTypeDefinition format, InspectorOptions options) where T : class
        {
            OutputLog?.LogInformation("Writing to output...");

            using var writer = await OpenFile(outputFactory, options);
            CreateRdfFileHandler(writer, out var mapper, options, format, out var handler);
            Graph? queryGraph = null;
            if(options.Buffering == BufferingLevel.Temporary || queries.Count > 0)
            {
                handler = new TemporaryGraphHandler(handler, out queryGraph);
            }

            SetDefaultNamespaces(mapper);

            NodeQueryTester? tester = null;
            if(queries.Count > 0 && queryGraph != null)
            {
                tester = new FileNodeQueryTester(handler, queryGraph, queries);
            }

            foreach(var entity in entities)
            {
                await AnalyzeEntity(entity, handler, graphHandlers, mapper, tester, options);
            }
        }

        /// <summary>
        /// Stores the data in a graph and then saves it.
        /// </summary>
        private async Task SaveRdfToOutput<T>(IEnumerable<T> entities, IReadOnlyCollection<SparqlQuery> queries, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, Func<ValueTask<Stream>> outputFactory, MimeTypeDefinition format, InspectorOptions options) where T : class
        {
            using var textWriter = await OpenFile(outputFactory, options);
            OutputLog?.LogInformation("Creating graph...");

            var handler = CreateGraphHandler(queries.Count > 0, out var graph);

            SetDefaultNamespaces(graph.NamespaceMap);

            NodeQueryTester? tester = null;
            if(queries.Count > 0)
            {
                tester = new FileNodeQueryTester(handler, graph, queries);
            }

            foreach(var entity in entities)
            {
                await AnalyzeEntity(entity, handler, graphHandlers, null, tester, options);
            }

            OutputLog?.LogInformation("Saving...");
            SaveGraph(graph, textWriter, options, format);
        }
        
        /// <summary>
        /// Immediately saves the data to output without an intermediate storage.
        /// </summary>
        private async Task WriteSparqlToOutput<T>(IEnumerable<T> entities, IReadOnlyCollection<SparqlQuery> queries, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, Func<ValueTask<Stream>> outputFactory, MimeTypeDefinition format, InspectorOptions options) where T : class
        {
            using var fileWriter = await OpenFile(outputFactory, options);

            OutputLog?.LogInformation("Searching...");

            IRdfHandler handler = new TemporaryGraphHandler(out var graph);
            handler = new ConcurrentHandler(handler);

            var sparqlWriter = format.GetSparqlResultsWriter();
            ConfigureNewComponent(sparqlWriter);

            SetDefaultNamespaces(graph.NamespaceMap);

            var tester = new SearchNodeQueryTester(handler, graph, queries);

            SparqlResultSet result;
            try{
                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null, tester, options);
                }
                foreach(var _ in tester.Match(graph.CreateBlankNode()));
                result = tester.GetResultSet();
            }catch(SearchNodeQueryTester.SearchEndedException searchEnded)
            {
                result = searchEnded.ResultSet;
            }catch(InternalApplicationException outer) when(outer.InnerException is SearchNodeQueryTester.SearchEndedException searchEnded)
            {
                result = searchEnded.ResultSet;
            }

            OutputLog?.LogInformation("Saving results...");
            sparqlWriter.Save(result, fileWriter);
        }

        /// <summary>
        /// Stores the data in a graph and then saves it.
        /// </summary>
        private async Task SaveSparqlToOutput<T>(IEnumerable<T> entities, IReadOnlyCollection<SparqlQuery> queries, IReadOnlyDictionary<Uri, IRdfHandler?> graphHandlers, Func<ValueTask<Stream>> outputFactory, MimeTypeDefinition format, InspectorOptions options) where T : class
        {
            using var fileWriter = await OpenFile(outputFactory, options);

            OutputLog?.LogInformation("Searching...");

            bool mayExitEarly = queries.Any(q => q.Limit >= 0);

            var handler = CreateGraphHandler(mayExitEarly, out var graph);
            handler = new ConcurrentHandler(handler);

            var sparqlWriter = format.GetSparqlResultsWriter();
            ConfigureNewComponent(sparqlWriter);

            SetDefaultNamespaces(graph.NamespaceMap);

            var tester = mayExitEarly ? new SearchNodeQueryTester(handler, graph, queries) : null;

            SparqlResultSet result;
            try{
                foreach(var entity in entities)
                {
                    await AnalyzeEntity(entity, handler, graphHandlers, null, tester, options);
                }
                tester ??= new SearchNodeQueryTester(handler, graph, queries);
                foreach(var _ in tester.Match(graph.CreateBlankNode()));
                result = tester.GetResultSet();
            }catch(SearchNodeQueryTester.SearchEndedException searchEnded)
            {
                result = searchEnded.ResultSet;
            }catch(InternalApplicationException outer) when(outer.InnerException is SearchNodeQueryTester.SearchEndedException searchEnded)
            {
                result = searchEnded.ResultSet;
            }

            OutputLog?.LogInformation("Saving results...");
            sparqlWriter.Save(result, fileWriter);
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
        private async Task<IReadOnlyCollection<SparqlQuery>> GetQueries(InspectorOptions options)
        {
            SparqlQueryParser? queryParser = null;
            var results = new List<SparqlQuery>();
            foreach(var file in options.Queries)
            {
                if(queryParser == null)
                {
                    queryParser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
                    queryParser.DefaultBaseUri = new Uri(options.Root, UriKind.Absolute);
                    queryParser.Warning += msg => OutputLog?.LogWarning(msg);
                }
                SparqlQuery query;
                try{
                    try{
                        using var stream = file.Open();
                        using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
                        query = queryParser.Parse(reader);
                    }catch(NotSupportedException)
                    {
                        // If synchronous reading is not available
                        using var buffer = new MemoryStream();
                        using(var stream = file.Open())
                        {
                            await stream.CopyToAsync(buffer);
                        }
                        buffer.Position = 0;
                        using var reader = new StreamReader(buffer, Encoding.UTF8, true, 4096, true);
                        query = queryParser.Parse(reader);
                    }
                }catch(Exception e)
                {
                    throw new ApplicationException($"Error while parsing SPARQL query in {file.Name}: {e.Message}", e);
                }
                if(options.OutputIsSparqlResults)
                {
                    switch(query.QueryType)
                    {
                        case SparqlQueryType.Ask:
                        case SparqlQueryType.Select:
                        case SparqlQueryType.SelectAll:
                        case SparqlQueryType.SelectAllDistinct:
                        case SparqlQueryType.SelectDistinct:
                        case SparqlQueryType.SelectReduced:
                            results.Add(query);
                            continue;
                    }
                }else{
                    switch(query.QueryType)
                    {
                        case SparqlQueryType.Construct:
                            results.Add(query);
                            continue;
                        case SparqlQueryType.Select:
                        case SparqlQueryType.SelectAll:
                        case SparqlQueryType.SelectAllDistinct:
                        case SparqlQueryType.SelectDistinct:
                        case SparqlQueryType.SelectReduced:
                            if(!query.Variables.Any(v => FileNodeQueryTester.NodeVariableName.Equals(v.Name)))
                            {
                                throw new ApplicationException($"The SELECT query in {file.Name} does not use the ?{FileNodeQueryTester.NodeVariableName} variable, which is necessary to use to match files for extraction.");
                            }
                            results.Add(query);
                            continue;
                    }
                }
                throw new ApplicationException($"Query in {file.Name} has an unsupported type ({query.QueryType}).");
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
            var handler = new LinkedNodeHandler(new UriTools.PrefixFormatter<string>(options.Root), rdfHandler, graphHandlers, queryTester, options.SimplifyBlankNodes);
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
        /// <param name="options">Additional options.</param>
        /// <returns>Text writer to the output file.</returns>
        private async ValueTask<TextWriter> OpenFile(Func<ValueTask<Stream>> fileFactory, InspectorOptions options)
        {
            var stream = await fileFactory();
            if(options.CompressedOutput)
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
        /// <param name="writer">The writer to save the data to.</param>
        /// <param name="mapper">A variable that receives the instance of <see cref="INamespaceMapper"/> representing the namespaces in use by the handler.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="format">The format to use for writing.</param>
        /// <param name="handler">A variable that receives the RDF handler to use.</param>
        private void CreateRdfFileHandler(TextWriter writer, out INamespaceMapper mapper, InspectorOptions options, MimeTypeDefinition format, out IRdfHandler handler)
        {
            var qnameMapper = new QNameOutputMapper();
            if(format.CanonicalMimeType == "application/ld+json")
            {
                // Use the custom JSON-LD handler
                handler = new JsonLdHandler(writer, qnameMapper)
                {
                    JsonFormatting = options.PrettyPrint ? Newtonsoft.Json.Formatting.Indented : 0
                };
                ConfigureNewComponent(handler);
            }else{
                var rdfWriter = format.CanWriteRdf ? format.GetRdfWriter() as IFormatterBasedWriter : null;
                if(rdfWriter == null)
                {
                    throw new ApplicationException($"Format {format.SyntaxName} requires buffered output!");
                }
                var formatter = CreateFormatter(format.SyntaxName, rdfWriter.TripleFormatterType, qnameMapper);
                ConfigureNewComponent(formatter);
                if(options.PrettyPrint && format.CanonicalMimeType == "text/turtle" && formatter is TurtleFormatter turtleFormatter)
                {
                    // Use the custom Turtle handler
                    handler = new TurtleHandler<TurtleFormatter>(writer, turtleFormatter, qnameMapper);
                    ConfigureNewComponent(handler);
                }else{
                    handler = new WriteThroughHandler(formatter, writer, true);
                    handler = new NamespaceHandler(handler, qnameMapper);
                }
            }
            mapper = qnameMapper;
        }

        class WriteThroughHandler : VDS.RDF.Parsing.Handlers.WriteThroughHandler
        {
            public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer, bool closeOnEnd) : base(formatter, writer, closeOnEnd)
            {
                if(NodeFactory is NodeFactory nodeFactory)
                {
                    nodeFactory.ValidateLanguageSpecifiers = false;
                }
            }
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
        /// <param name="immediate">Whether the triples encountered by the handler have to be immediately added to the graph.</param>
        /// <param name="graph">The variable that receives the created graph.</param>
        /// <returns>The RDF handler to use.</returns>
        private IRdfHandler CreateGraphHandler(bool immediate, out Graph graph)
        {
            graph = new Graph(
                null,
                nodeFactory: new NodeFactory(new NodeFactoryOptions()
                {
                    ValidateLanguageSpecifiers = false
                }),
                emptyNamespaceMap: true
            );
            return immediate ? new DirectGraphHandler(graph) : new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
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
        /// <param name="textWriter">The writer to save the data to.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="format">The format to use for writing.</param>
        private void SaveGraph(Graph graph, TextWriter textWriter, InspectorOptions options, MimeTypeDefinition format)
        {
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
            ConfigureNewComponent(writer);
        }
    }
}
