﻿using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.ConsoleApp
{
    /// <summary>
    /// The specific implementation of <see cref="Inspector"/> for the console application.
    /// </summary>
    class ConsoleInspector : ExtensibleInspector
    {
        string baseDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");

        /// <summary>
        /// The default image analyzer.
        /// </summary>
        public ImageAnalyzer ImageAnalyzer { get; }

        /// <inheritdoc/>
        public ConsoleInspector()
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

        public async override ValueTask AddDefault()
        {
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

            Plugins.Clear();
            foreach(var plugin in LoadPlugins())
            {
                Plugins.Add(plugin);
            }

            await base.AddDefault();
        }

        IEnumerable<Plugin> LoadPlugins()
        {
            if(Directory.Exists(baseDirectory))
            {
                foreach(var dir in Directory.EnumerateDirectories(baseDirectory))
                {
                    yield return GetPluginFromDirectory(dir);
                }

                foreach(var zip in Directory.EnumerateFiles(baseDirectory, "*.zip"))
                {
                    yield return GetPluginFromZip(zip);
                }
            }
        }

        Plugin GetPluginFromDirectory(string dir)
        {
            // Look for a file with .dll and the same name as the directory
            var name = Path.GetFileName(dir) + ".dll";
            var info = new DirectoryInfo(dir);
            return new Plugin(GetDirectory(info), name);
        }

        Plugin GetPluginFromZip(string file)
        {
            // Look for a file with .zip changed to .dll
            var name = Path.ChangeExtension(Path.GetFileName(file), "dll");
            ZipArchive archive;
            try{
                archive = ZipFile.OpenRead(file);
            }catch(Exception e)
            {
                OutputLog?.WriteLine($"An error occurred while opening plugin archive {Path.GetFileName(file)}: " + e);
                return default;
            }
            return new Plugin(GetDirectory(archive), name);
        }

        protected override Assembly LoadFromFile(IFileInfo file, IDirectoryInfo mainDirectory)
        {
            // Add directories for assembly lookup:
            var context = new PluginLoadContext();
            context.AddDirectory(mainDirectory);
            context.AddDirectory(baseDirectory);
            context.AddDirectory(AppContext.BaseDirectory);
            return context.LoadFromFile(file);
        }
    }
}