using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// The specific implementation of <see cref="Inspector"/> for the web application.
    /// </summary>
    public class WebInspector : ExtensibleInspector
    {
        /// <inheritdoc/>
        public WebInspector()
        {
            DataAnalyzer.FileSizeToWriteToDisk = Int64.MaxValue;
        }

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            DataAnalyzer.DataFormats.Add(new ZipFormat());
            DataAnalyzer.DataFormats.Add(new RarFormat());
            DataAnalyzer.DataFormats.Add(new SevenZipFormat());
            DataAnalyzer.DataFormats.Add(new GZipFormat());
            DataAnalyzer.DataFormats.Add(new TarFormat());
            DataAnalyzer.DataFormats.Add(new SzFormat());
            DataAnalyzer.DataFormats.Add(new ImageMetadataFormat());
            DataAnalyzer.DataFormats.Add(new TagLibFormat());
            DataAnalyzer.DataFormats.Add(new PdfFormat());
            DataAnalyzer.DataFormats.Add(new IsoFormat());
            DataAnalyzer.DataFormats.Add(new DosModuleExeFormat());
            DataAnalyzer.DataFormats.Add(new DosModuleComFormat());
            DataAnalyzer.DataFormats.Add(new GenericModuleFormat());
            DataAnalyzer.DataFormats.Add(new LinearModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win16ModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win32ModuleFormatManaged());
            DataAnalyzer.DataFormats.Add(new DelphiFormFormat());
            DataAnalyzer.DataFormats.Add(new OleStorageFormat());
            DataAnalyzer.DataFormats.Add(new ShockwaveFlashFormat());
            DataAnalyzer.DataFormats.Add(new WarcFormat());
            DataAnalyzer.DataFormats.Add(new HtmlFormat());

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
            Analyzers.Add(new DelphiObjectAnalyzer());
            Analyzers.Add(new CabinetAnalyzer());
            Analyzers.Add(new OleStorageAnalyzer());
            Analyzers.Add(new PackageDescriptionAnalyzer());
            Analyzers.Add(new OleDocumentAnalyzer());
            Analyzers.Add(new OpenXmlDocumentAnalyzer());
            Analyzers.Add(new PdfAnalyzer());
            Analyzers.Add(new ShockwaveFlashAnalyzer());
            Analyzers.Add(new WarcAnalyzer());
            Analyzers.Add(new HtmlAnalyzer());

            Plugins.Clear();
            await LoadPlugins();

            await base.AddDefault();
        }

        /// <inheritdoc/>
        protected override Assembly LoadFromFile(IFileInfo file, IDirectoryInfo mainDirectory)
        {
            var context = new PluginLoadContext();
            context.AddDirectory(mainDirectory);
            return context.LoadFromFile(file);
        }

        async ValueTask LoadPlugins()
        {
            var files = Pages.Index.PluginFiles.ToList();
            foreach(var (name, file) in files)
            {
                ArraySegment<byte> data;
                using(var stream = file.OpenReadStream(Int64.MaxValue))
                {
                    var buffer = new MemoryStream();
                    await stream.CopyToAsync(buffer);
                    if(!buffer.TryGetBuffer(out data))
                    {
                        data = new ArraySegment<byte>(buffer.ToArray());
                    }
                }
                ZipArchive archive;
                try{
                    var buffer = new MemoryStream(data.Array!, data.Offset, data.Count, false);
                    archive = new ZipArchive(buffer, ZipArchiveMode.Read);
                }catch(Exception e)
                {
                    OutputLog?.WriteLine($"An error occurred while opening plugin archive {name}: " + e);
                    continue;
                }
                Plugins.Add(new Plugin(GetDirectory(archive), Path.ChangeExtension(name, ".dll")));
            }
        }
    }
}
