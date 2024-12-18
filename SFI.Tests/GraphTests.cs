﻿using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
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
        static readonly Uri currentDirectory = new(Environment.CurrentDirectory + Path.DirectorySeparatorChar, UriKind.Absolute);
        static readonly HttpClient httpClient = new();

        static readonly Inspector inspector = new TestInspector();
        static readonly InspectorOptions inspectorOptions = new()
        {
            CompressedOutput = false,
            Buffering = BufferingLevel.Full,
            Format = "ttl",
            HideMetadata = true,
            PrettyPrint = true,
            SimplifyBlankNodes = true,
        };

        static readonly IRdfReader turtleParser = new Notation3Parser();

        static async Task TestOutputGraph(string source)
        {
            var idUri = new Uri(source, UriKind.RelativeOrAbsolute);
            var id = UriTools.UuidFromUri(idUri).ToString("N");
            id = id.Substring(0, 2) + Path.DirectorySeparatorChar + id;
            var uri = idUri.IsAbsoluteUri ? idUri : new Uri(currentDirectory, idUri);
            var host = uri.Host;
            if(!String.IsNullOrEmpty(host))
            {
                id = host + Path.DirectorySeparatorChar + id;
            }else{
                id = "local" + Path.DirectorySeparatorChar + id;
            }

            const string cachedDir = "Cached";
            const string compareDir = "ExpectedDescriptions";
            const string notmatchedDir = "NotMatchedDescriptions";
            const string newDir = "NewDescriptions";
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
                    var downloadUri = uri.AbsoluteUri;
                    for(int i = 0; i < 2; i++)
                    {
                        try{
                            using var stream = await httpClient.GetStreamAsync(downloadUri);
                            using var fileStream = File.Create(cachedFile);
                            await stream.CopyToAsync(fileStream);
                            break;
                        }catch(HttpRequestException) when(i == 0)
                        {
                            downloadUri = "http://web.archive.org/web/2022id_/" + downloadUri;
                        }
                    }
                }
                file = new TestFile(idUri, cachedFile);
            }

            var buffer = new MemoryStream();
            await inspector.Inspect(file, buffer, inspectorOptions);

            if(!File.Exists(compareFile))
            {
                string newFile = Path.Combine(newDir, id + ".ttl");
                Directory.CreateDirectory(Path.GetDirectoryName(newFile)!);
                File.WriteAllBytes(newFile, buffer.ToArray());
                Assert.Inconclusive($"Saved output could not be found. New description was saved to {newFile}.");
            }

            var comparedData = File.ReadAllBytes(compareFile);

            var graph = new Graph(true);
            turtleParser.Load(graph, compareFile);

            var data = buffer.GetData();
            buffer = new MemoryStream(data.Array!, data.Offset, data.Count, false);

            var graph2 = new Graph(true);
            turtleParser.Load(graph2, new StreamReader(buffer));

            var graphDiff = new GraphDiff();
            GraphDiffReport? report = null;
            var thread = new Thread(() => report = graphDiff.Difference(graph, graph2));
            thread.IsBackground = true;
            thread.Start();
            if(!thread.Join(60000))
            {
                thread.Priority = ThreadPriority.Lowest;
                Assert.Inconclusive("The graphs could not be compared within the timeout.");
            }

            string notmatchedFile = Path.Combine(notmatchedDir, id + ".ttl");
            if(!report!.AreEqual)
            {
                var turtleFormatter = new TurtleFormatter(graph2);
                Console.Error.WriteLine($"File: {compareFile}");
                Console.Error.WriteLine("Added:");
                foreach(var added in report.AddedTriples.Concat(report.AddedMSGs.SelectMany(g => g.Triples)))
                {
                    Console.Error.WriteLine(turtleFormatter.Format(added));
                }
                Console.Error.WriteLine("Removed:");
                foreach(var added in report.RemovedTriples.Concat(report.RemovedMSGs.SelectMany(g => g.Triples)))
                {
                    Console.Error.WriteLine(turtleFormatter.Format(added));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(notmatchedFile)!);
                File.WriteAllBytes(notmatchedFile, buffer.ToArray());
            }
            Assert.IsTrue(report.AreEqual, $"The graphs are not equal. New description was saved to {notmatchedFile}.");
        }
        
        class TestInspector : ComponentInspector
        {
            /// <summary>
            /// The default image analyzer.
            /// </summary>
            public ImageAnalyzer? ImageAnalyzer { get; private set; }

            /// <inheritdoc/>
            public TestInspector()
            {
                var valueTask = AddDefault();
                if(valueTask.IsCompletedSuccessfully)
                {
                    return;
                }else if(valueTask.IsFaulted)
                {
                    var task = valueTask.AsTask();
                    if(task.Exception!.InnerExceptions.Count == 1)
                    {
                        ExceptionDispatchInfo.Capture(task.Exception.InnerException!).Throw();
                    }
                    throw task.Exception;
                }else{
                    valueTask.AsTask().Wait();
                }
            }

            /// <inheritdoc/>
            public async override ValueTask AddDefault()
            {
                await LoadAssembly(Formats.All.Provider.Assembly);
                await LoadAssembly(Hashes.All.Provider.Assembly);

                await base.AddDefault();

                DataAnalyzer.MinDataLengthToStore = 0;
                DataAnalyzer.HashAlgorithms.Clear();
                FileAnalyzer.HashAlgorithms.Clear();

                var imageFormat = DataAnalyzer.DataFormats.OfType<ImageFormat>().FirstOrDefault()!;
                imageFormat.PreferImageSharp = true;
                imageFormat.AllowImageSharp = true;
                imageFormat.AllowNativeImage = false;

                ImageAnalyzer = Analyzers.OfType<ImageAnalyzer>().FirstOrDefault()!;

                ImageAnalyzer.MakeThumbnail = false;
                ImageAnalyzer.DataHashAlgorithms.Clear();
                ImageAnalyzer.LowFrequencyImageHashAlgorithms.Clear();

                DataAnalyzer.HashAlgorithms.Add(BuiltInHash.MD5!);
                DataAnalyzer.ContentUriFormatter = new NiHashedContentUriFormatter(BuiltInHash.MD5!);

                if(Analyzers.OfType<ExceptionAnalyzer>().FirstOrDefault() is { } exceptionAnalyzer)
                {
                    exceptionAnalyzer.IgnoreSourcePosition = true;
                }

                if(Analyzers.OfType<WaveAnalyzer>().FirstOrDefault() is { } waveAnalyzer)
                {
                    waveAnalyzer.CreateSpectrum = false;
                }

                if(Analyzers.OfType<DosModuleAnalyzer>().FirstOrDefault() is { } dosAnalyzer)
                {
                    dosAnalyzer.Emulate = false;
                }

                if(Analyzers.OfType<X509CertificateAnalyzer>().FirstOrDefault() is { } x509Analyzer)
                {
                    x509Analyzer.DescribeExtensions = false;
                }

                if(Analyzers.OfType<MediaTypeAnalyzer>().FirstOrDefault() is { } mediaTypeAnalyzer)
                {
                    mediaTypeAnalyzer.AddClasses = false;
                }

                if(Analyzers.OfType<ExtensionObjectAnalyzer>().FirstOrDefault() is { } extensionObjectAnalyzer)
                {
                    extensionObjectAnalyzer.AddClasses = false;
                }

                if(Analyzers.OfType<AssemblyAnalyzer>().FirstOrDefault() is { } assemblyAnalyzer)
                {
                    assemblyAnalyzer.LinkGlobals = true;
                }
            }
        }

        class TestFile : IFileInfo
        {
            readonly Uri uri;
            readonly string location;

            public string? Name => Uri.UnescapeDataString(System.IO.Path.GetFileName(uri.OriginalString));
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
            static readonly Type FileSystemInfoType = typeof(FileSystemInfo);
            public object? ReferenceKey => FileSystemInfoType;
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
