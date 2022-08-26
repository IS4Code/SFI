using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace IS4.MultiArchiver.ConsoleApp
{
    /// <summary>
    /// The specific implementation of <see cref="Archiver"/> for the console application.
    /// </summary>
    class ConsoleArchiver : Archiver
    {
        /// <summary>
        /// The default image analyzer.
        /// </summary>
        public ImageAnalyzer ImageAnalyzer { get; }

        public override ICollection<IDataHashAlgorithm> ImageDataHashAlgorithms => ImageAnalyzer.DataHashAlgorithms;

        /// <inheritdoc/>
        public ConsoleArchiver()
        {
            Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());

            ImageAnalyzer.LowFrequencyImageHashAlgorithms.Add(Analysis.Images.DHash.Instance);

            var algorithms = ImageAnalyzer.DataHashAlgorithms;
            DataAnalyzer.HashAlgorithms.Add(Blake3Hash.Instance);
            foreach(var algorithm in DataAnalyzer.HashAlgorithms)
            {
                algorithms.Add(algorithm);
            }
        }

        public IDictionary<AssemblyName, AssemblyLoadContext> LoadedPlugins { get; } = new Dictionary<AssemblyName, AssemblyLoadContext>();

        void AddPlugins()
        {
            var services = new ServiceCollection();
            services.AddSingleton<Archiver>(this);
            services.AddSingleton(OutputLog);
            var serviceProvider = services.BuildServiceProvider();

            var baseDir = AppContext.BaseDirectory;
            var plugins = Path.Combine(baseDir, "plugins");
            foreach(var pluginPath in Directory.EnumerateFiles(plugins, "*.dll"))
            {
                var pluginName = Path.GetFileNameWithoutExtension(pluginPath);
                var pluginDir = Path.Combine(plugins, pluginName);
                var context = new PluginLoadContext();
                context.AddDirectory(pluginDir);
                context.AddDirectory(plugins);
                context.AddDirectory(baseDir);

                int count = 0;

                var asm = context.LoadFromAssemblyPath(pluginPath);
                foreach(var type in asm.ExportedTypes)
                {
                    if(!type.IsAbstract && type.IsClass && !type.IsGenericTypeDefinition)
                    {
                        object instance = null;
                        bool error = false;

                        bool CreateInstance<T>(out T result) where T : class
                        {
                            if(instance == null && !error)
                            {
                                try
                                {
                                    instance = ActivatorUtilities.CreateInstance(serviceProvider, type);
                                }catch(Exception e)
                                {
                                    OutputLog?.WriteLine($"An exception occurred while creating an instance of type {type} from plugin {pluginName}: {e}");
                                    error = true;
                                }
                            }
                            result = instance as T;
                            return result != null;
                        }

                        if(type.IsEntityAnalyzerType())
                        {
                            if(CreateInstance<object>(out var analyzer))
                            {
                                Analyzers.Add(analyzer);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IBinaryFileFormat)))
                        {
                            if(CreateInstance<IBinaryFileFormat>(out var format))
                            {
                                DataAnalyzer.DataFormats.Add(format);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IXmlDocumentFormat)))
                        {
                            if(CreateInstance<IXmlDocumentFormat>(out var format))
                            {
                                XmlAnalyzer.XmlFormats.Add(format);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IContainerAnalyzerProvider)))
                        {
                            if(CreateInstance<IContainerAnalyzerProvider>(out var provider))
                            {
                                ContainerProviders.Add(provider);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IDataHashAlgorithm)))
                        {
                            if(CreateInstance<IDataHashAlgorithm>(out var hash))
                            {
                                DataAnalyzer.HashAlgorithms.Add(hash);
                                ImageAnalyzer.DataHashAlgorithms.Add(hash);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IFileHashAlgorithm)))
                        {
                            if(CreateInstance<IFileHashAlgorithm>(out var hash))
                            {
                                FileAnalyzer.HashAlgorithms.Add(hash);
                            }
                        }
                        if(type.IsAssignableTo(typeof(IObjectHashAlgorithm<Image>)))
                        {
                            if(CreateInstance<IObjectHashAlgorithm<Image>>(out var hash))
                            {
                                ImageAnalyzer.LowFrequencyImageHashAlgorithms.Add(hash);
                            }
                        }

                        if(instance != null)
                        {
                            count++;
                        }
                    }
                }

                if(count > 0)
                {
                    OutputLog?.WriteLine($"Loaded {count} component{(count == 1 ? "" : "s")} from plugin {pluginName}.");
                }
            }
        }

        public override void AddDefault()
        {
            base.AddDefault();

            DataAnalyzer.DataFormats.Add(new ZipFormat());
            DataAnalyzer.DataFormats.Add(new RarFormat());
            DataAnalyzer.DataFormats.Add(new SevenZipFormat());
            DataAnalyzer.DataFormats.Add(new GZipFormat());
            DataAnalyzer.DataFormats.Add(new TarFormat());
            DataAnalyzer.DataFormats.Add(new SzFormat());
            DataAnalyzer.DataFormats.Add(new ImageMetadataFormat());
            DataAnalyzer.DataFormats.Add(new ImageFormat());
            DataAnalyzer.DataFormats.Add(new TagLibFormat());
            DataAnalyzer.DataFormats.Add(new IsoFormat());
            DataAnalyzer.DataFormats.Add(new DosModuleFormat());
            DataAnalyzer.DataFormats.Add(new GenericModuleFormat());
            DataAnalyzer.DataFormats.Add(new LinearModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win16ModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win32ModuleFormatManaged());
            DataAnalyzer.DataFormats.Add(new WaveFormat());
            //DataAnalyzer.Formats.Add(new OggFormat());
            DataAnalyzer.DataFormats.Add(new WasapiFormat(false));
            DataAnalyzer.DataFormats.Add(new WasapiFormat(true));
            DataAnalyzer.DataFormats.Add(new DelphiFormFormat());
            DataAnalyzer.DataFormats.Add(new CabinetFormat());
            DataAnalyzer.DataFormats.Add(new OleStorageFormat());

            ContainerProviders.Add(new OpenPackageFormat());
            ContainerProviders.Add(new PackageDescriptionProvider());
            ContainerProviders.Add(new ExcelXmlDocumentFormat());
            ContainerProviders.Add(new ExcelDocumentFormat());
            ContainerProviders.Add(new WordXmlDocumentFormat());

            XmlAnalyzer.XmlFormats.Add(new SvgFormat());

            Analyzers.Add(new ArchiveAnalyzer());
            Analyzers.Add(new ArchiveReaderAnalyzer());
            Analyzers.Add(new FileSystemAnalyzer());
            Analyzers.Add(ImageMetadataAnalyzer.CreateDefault());
            Analyzers.Add(new TagLibAnalyzer());
            Analyzers.Add(new DosModuleAnalyzer());
            Analyzers.Add(new WinModuleAnalyzer());
            Analyzers.Add(new WinVersionAnalyzerManaged());
            Analyzers.Add(new SvgAnalyzer());
            Analyzers.Add(new WaveAnalyzer());
            Analyzers.Add(new DelphiObjectAnalyzer());
            Analyzers.Add(new CabinetAnalyzer());
            Analyzers.Add(new OleStorageAnalyzer());
            Analyzers.Add(new PackageDescriptionAnalyzer());

            AddPlugins();
        }
    }
}
