using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using System;

namespace IS4.MultiArchiver.WebApp
{
    /// <summary>
    /// The specific implementation of <see cref="Archiver"/> for the web application.
    /// </summary>
    public class WebArchiver : Archiver
    {
        /// <inheritdoc/>
        public WebArchiver()
        {
            DataAnalyzer.FileSizeToWriteToDisk = Int64.MaxValue;
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
            DataAnalyzer.DataFormats.Add(new TagLibFormat());
            DataAnalyzer.DataFormats.Add(new IsoFormat());
            DataAnalyzer.DataFormats.Add(new DosModuleFormat());
            DataAnalyzer.DataFormats.Add(new GenericModuleFormat());
            DataAnalyzer.DataFormats.Add(new LinearModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win16ModuleFormat());
            DataAnalyzer.DataFormats.Add(new Win32ModuleFormatManaged());
            DataAnalyzer.DataFormats.Add(new DelphiFormFormat());
            DataAnalyzer.DataFormats.Add(new OleStorageFormat());

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
        }
    }
}
