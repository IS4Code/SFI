using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace IS4.MultiArchiver.ConsoleApp
{
    /// <summary>
    /// The specific implementation of <see cref="Inspector"/> for the console application.
    /// </summary>
    class ConsoleInspector : Inspector
    {
        /// <summary>
        /// The default image analyzer.
        /// </summary>
        public ImageAnalyzer ImageAnalyzer { get; }

        public override ICollection<IDataHashAlgorithm> ImageDataHashAlgorithms => ImageAnalyzer.DataHashAlgorithms;

        public override ICollection<IHashAlgorithm> ImageHashAlgorithms => new ConcreteCollectionWrapper<IHashAlgorithm, IObjectHashAlgorithm<Image>>(ImageAnalyzer.LowFrequencyImageHashAlgorithms);

        readonly Plugins plugins;

        /// <inheritdoc/>
        public ConsoleInspector()
        {
            plugins = new Plugins(this);

            Analyzers.Add(ImageAnalyzer = new ImageAnalyzer());

            ImageAnalyzer.LowFrequencyImageHashAlgorithms.Add(Analysis.Images.DHash.Instance);

            var algorithms = ImageAnalyzer.DataHashAlgorithms;
            DataAnalyzer.HashAlgorithms.Add(Blake3Hash.Instance);
            foreach(var algorithm in DataAnalyzer.HashAlgorithms)
            {
                algorithms.Add(algorithm);
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
            DataAnalyzer.DataFormats.Add(new PdfFormat());
            DataAnalyzer.DataFormats.Add(new IsoFormat());
            DataAnalyzer.DataFormats.Add(new DosModuleFormat());
            DataAnalyzer.DataFormats.Add(new GenericModuleFormat());
            DataAnalyzer.DataFormats.Add(new LinearModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win16ModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win32ModuleFormatManaged());
            DataAnalyzer.DataFormats.Add(new WaveFormat());
            DataAnalyzer.DataFormats.Add(new WasapiFormat(false));
            DataAnalyzer.DataFormats.Add(new WasapiFormat(true));
            DataAnalyzer.DataFormats.Add(new DelphiFormFormat());
            DataAnalyzer.DataFormats.Add(new CabinetFormat());
            DataAnalyzer.DataFormats.Add(new OleStorageFormat());
            DataAnalyzer.DataFormats.Add(new InternetShortcutFormat());
            DataAnalyzer.DataFormats.Add(new ShellLinkFormat());

            ContainerProviders.Add(new OpenPackageFormat());
            ContainerProviders.Add(new PackageDescriptionProvider());
            ContainerProviders.Add(new ExcelXmlDocumentFormat());
            ContainerProviders.Add(new ExcelDocumentFormat());
            ContainerProviders.Add(new WordXmlDocumentFormat());
            ContainerProviders.Add(new WordDocumentFormat());

            XmlAnalyzer.XmlFormats.Add(new SvgFormat());

            Analyzers.Add(new ArchiveAnalyzer());
            Analyzers.Add(new ArchiveReaderAnalyzer());
            Analyzers.Add(new FileSystemAnalyzer());
            Analyzers.Add(new ImageMetadataAnalyzer());
            Analyzers.Add(new ExifMetadataAnalyzer());
            Analyzers.Add(new XmpMetadataAnalyzer());
            Analyzers.Add(new TagLibAnalyzer());
            Analyzers.Add(new XmpTagAnalyzer());
            Analyzers.Add(new DosModuleAnalyzer());
            Analyzers.Add(new WinModuleAnalyzer());
            Analyzers.Add(new WinVersionAnalyzerManaged());
            Analyzers.Add(new SvgAnalyzer());
            Analyzers.Add(new WaveAnalyzer());
            Analyzers.Add(new DelphiObjectAnalyzer());
            Analyzers.Add(new CabinetAnalyzer());
            Analyzers.Add(new OleStorageAnalyzer());
            Analyzers.Add(new PackageDescriptionAnalyzer());
            Analyzers.Add(new OleDocumentAnalyzer());
            Analyzers.Add(new OpenXmlDocumentAnalyzer());
            Analyzers.Add(new PdfAnalyzer());
            Analyzers.Add(new InternetShortcutAnalyzer());
            Analyzers.Add(new ShellLinkAnalyzer());

            LoadPlugins();
        }

        void LoadPlugins()
        {
            var loaded = new Dictionary<Assembly, int>();

            foreach(var (type, ctor) in plugins.LoadPlugins("plugins"))
            {
                object instance = null;
                bool error = false;

                bool CreateInstance<T>(out T result) where T : class
                {
                    // Re-use instance if already created for different types
                    if(instance == null && !error)
                    {
                        try{
                            instance = ctor();
                            if(!loaded.TryGetValue(type.Assembly, out var count))
                            {
                                count = 0;
                            }
                            loaded[type.Assembly] = count + 1;
                        }catch(Exception e) when(GlobalOptions.SuppressNonCriticalExceptions)
                        {
                            OutputLog?.WriteLine($"An exception occurred while creating an instance of type {type} from assembly {type.Assembly.GetName().Name}: {e}");
                            // Prevents attempting to create the instance next time
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
            }

            foreach(var (asm, count) in loaded)
            {
                OutputLog?.WriteLine($"Loaded {count} component{(count == 1 ? "" : "s")} from assembly {asm.GetName().Name}.");
            }
        }
    }
}
