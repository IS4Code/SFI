using IS4.SFI.Analyzers;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

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
        static readonly TurtleFormatter turtleFormatter = new();
        static readonly GraphDiff graphDiff = new();

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

            const string cachedDir = "Cached";
            const string compareDir = "ExpectedDescriptions";
            var compareFile = Path.Combine(compareDir, id + ".ttl");
            Directory.CreateDirectory(Path.GetDirectoryName(compareFile)!);

            var path = uri.AbsolutePath;
            if(!String.IsNullOrEmpty(host))
            {
                path = host + path;
            }

            TestFile file;
            if(File.Exists(path) || File.Exists(path = Uri.UnescapeDataString(path)))
            {
                file = new TestFile(idUri, path);
            }else{
                var cachedFile = Path.Combine(cachedDir, id);
                if(!File.Exists(cachedFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachedFile)!);
                    using(var stream = await httpClient.GetStreamAsync(uri.AbsoluteUri))
                    {
                        using var fileStream = File.Create(cachedFile);
                        await stream.CopyToAsync(fileStream);
                    }
                }
                file = new TestFile(idUri, cachedFile);
            }

            Assert.IsTrue(File.Exists(compareFile));

            var comparedData = File.ReadAllBytes(compareFile);

            var graph = new Graph(true);
            turtleParser.Load(graph, compareFile);

            var buffer = new MemoryStream();
            await inspector.Inspect(file, buffer, inspectorOptions);
            var data = buffer.GetData();
            buffer = new MemoryStream(data.Array!, data.Offset, data.Count, false);

            var graph2 = new Graph(true);
            turtleParser.Load(graph2, new StreamReader(buffer));

            GraphDiffReport? report = null;
            var thread = new Thread(() => report = graphDiff.Difference(graph, graph2));
            thread.IsBackground = true;
            thread.Start();
            if(!thread.Join(20000))
            {
                thread.Priority = ThreadPriority.Lowest;
                Assert.Inconclusive("The graphs could not be compared within the timeout.");
            }

            if(!report!.AreEqual)
            {
                Console.Error.WriteLine($"File: {compareFile}");
                Console.Error.WriteLine("Added:");
                foreach(var added in report.AddedTriples.Concat(report.AddedMSGs.SelectMany(g => g.Triples)))
                {
                    Console.Error.WriteLine(turtleFormatter.Format(added));
                }
                Console.Error.WriteLine("Removed:");
                foreach(var added in report.AddedTriples.Concat(report.AddedMSGs.SelectMany(g => g.Triples)))
                {
                    Console.Error.WriteLine(turtleFormatter.Format(added));
                }
            }
            Assert.IsTrue(report.AreEqual);
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

                await base.AddDefault();

                DataAnalyzer.MinDataLengthToStore = 0;
                DataAnalyzer.HashAlgorithms.Clear();
                FileAnalyzer.HashAlgorithms.Clear();

                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5!);
                DataAnalyzer.ContentUriFormatter = new NiHashedContentUriFormatter(BuiltInHash.MD5!);

                if(Analyzers.OfType<WaveAnalyzer>().FirstOrDefault() is WaveAnalyzer waveAnalyzer)
                {
                    waveAnalyzer.CreateSpectrum = false;
                }

                if(Analyzers.OfType<DosModuleAnalyzer>().FirstOrDefault() is DosModuleAnalyzer dosAnalyzer)
                {
                    dosAnalyzer.Emulate = false;
                }
            }
        }

        class TestFile : IFileInfo
        {
            readonly Uri uri;
            readonly string location;

            public string? Name => Uri.UnescapeDataString(System.IO.Path.GetFileName(uri.AbsolutePath));
            public string? SubName => null;
            public string? Path => null;
            public int? Revision => null;
            public DateTime? CreationTime => null;
            public DateTime? LastWriteTime => null;
            public DateTime? LastAccessTime => null;
            public FileKind Kind => FileKind.None;
            public FileAttributes Attributes => FileAttributes.Normal;
            public long Length => new FileInfo(location).Length;
            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;
            public object? ReferenceKey => AppDomain.CurrentDomain;
            public object? DataKey => location;

            public TestFile(Uri uri, string location)
            {
                this.uri = uri;
                this.location = location;
            }

            public Stream Open()
            {
                return File.OpenRead(location);
            }

            public override string? ToString()
            {
                return null;
            }
        }
    }
}
