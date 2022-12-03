using IS4.SFI.Analyzers;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// Contains tests for checking the output of the inspection, by comparing
    /// the result graphs.
    /// </summary>
    [TestClass]
    public partial class GraphTests
    {
        static readonly Uri currentDirectory = new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar, UriKind.Absolute);
        static readonly HttpClient httpClient = new HttpClient();

        static readonly Inspector inspector = new TestInspector();
        static readonly InspectorOptions inspectorOptions = new()
        {
            CompressedOutput = false,
            DirectOutput = false,
            Format = "ttl",
            HideMetadata = true,
            PrettyPrint = true,
            SimplifyBlankNodes = true,
        };

        static readonly IRdfReader turtleParser = new Notation3Parser();
        static readonly GraphMatcher graphMatcher = new GraphMatcher();

        async Task TestOutputGraph(string source)
        {
            var idUri = new Uri(source, UriKind.RelativeOrAbsolute);
            var id = UriTools.UuidFromUri(idUri).ToString("N");
            id = id.Substring(0, 2) + Path.DirectorySeparatorChar + id;
            var uri = idUri.IsAbsoluteUri ? idUri : new Uri(currentDirectory, idUri);
            var host = uri.Host;
            if(!String.IsNullOrEmpty(host))
            {
                id = host + Path.DirectorySeparatorChar + id;
            }

            const string cachedDir = "cached";
            const string outputDir = "descriptions";
            var outputFile = Path.Combine(outputDir, id + ".ttl");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);

            var path = uri.AbsolutePath;
            if(!String.IsNullOrEmpty(host))
            {
                path = host + path;
            }

            IStreamFactory streamFactory;
            if(File.Exists(path) || File.Exists(path = Uri.UnescapeDataString(path)))
            {
                streamFactory = new FileInfoWrapper(new FileInfo(path));
            }else{
                var cachedFile = Path.Combine(cachedDir, id);
                if(!File.Exists(cachedFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachedFile)!);
                    using(var stream = await httpClient.GetStreamAsync(uri.AbsoluteUri))
                    {
                        using var file = File.Create(cachedFile);
                        await stream.CopyToAsync(file);
                    }
                }
                streamFactory = new FileInfoWrapper(new FileInfo(cachedFile));
            }

            if(!File.Exists(outputFile))
            {
                await inspector.Inspect(streamFactory, outputFile, inspectorOptions);
            }else{
                var graph = new Graph(true);
                turtleParser.Load(graph, outputFile);

                var buffer = new MemoryStream();
                await inspector.Inspect(streamFactory, buffer, inspectorOptions);
                var data = buffer.GetData();
                buffer = new MemoryStream(data.Array!, data.Offset, data.Count, false);

                var graph2 = new Graph(true);
                turtleParser.Load(graph2, new StreamReader(buffer));

                Assert.IsTrue(graphMatcher.Equals(graph, graph2));
            }
        }
        
        class TestInspector : Inspector
        {
            /// <summary>
            /// The default image analyzer.
            /// </summary>
            public ImageAnalyzer ImageAnalyzer { get; }

            /// <inheritdoc/>
            public TestInspector()
            {
                Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());
                ImageAnalyzer.MakeThumbnail = false;

                _ = AddDefault();
            }

            /// <inheritdoc/>
            public async override ValueTask AddDefault()
            {
                BaseFormats.AddDefault(Analyzers, DataAnalyzer.DataFormats, XmlAnalyzer.XmlFormats, ContainerProviders);
                ExternalFormats.AddDefault(Analyzers, DataAnalyzer.DataFormats, XmlAnalyzer.XmlFormats, ContainerProviders);
                AccessoriesFormats.AddDefault(Analyzers, DataAnalyzer.DataFormats, XmlAnalyzer.XmlFormats, ContainerProviders);
                MediaAnalysisFormats.AddDefault(Analyzers, DataAnalyzer.DataFormats, XmlAnalyzer.XmlFormats, ContainerProviders);
                WindowsFormats.AddDefault(Analyzers, DataAnalyzer.DataFormats, XmlAnalyzer.XmlFormats, ContainerProviders);

                if(Analyzers.OfType<WaveAnalyzer>().FirstOrDefault() is WaveAnalyzer waveAnalyzer)
                {
                    waveAnalyzer.CreateSpectrum = false;
                }

                await base.AddDefault();

                DataAnalyzer.MinDataLengthToStore = 0;
                DataAnalyzer.HashAlgorithms.Clear();
                FileAnalyzer.HashAlgorithms.Clear();
            }
        }
    }
}
