using IS4.SFI.Analyzers;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System.Collections.Generic;

namespace IS4.SFI
{
    /// <inheritdoc cref="BaseFormats"/>
    public static class ExternalFormats
    {
        /// <inheritdoc cref="BaseFormats.AddDefault(ICollection{object}, ICollection{IBinaryFileFormat}, ICollection{IXmlDocumentFormat}, ICollection{IContainerAnalyzerProvider})"/>
        public static void AddDefault(ICollection<object> analyzers, ICollection<IBinaryFileFormat> dataFormats, ICollection<IXmlDocumentFormat> xmlFormats, ICollection<IContainerAnalyzerProvider> containerProviders)
        {
            dataFormats.Add(new ZipFormat());
            dataFormats.Add(new RarFormat());
            dataFormats.Add(new SevenZipFormat());
            dataFormats.Add(new GZipFormat());
            dataFormats.Add(new TarFormat());
            dataFormats.Add(new SzFormat());
            dataFormats.Add(new ImageMetadataFormat());
            dataFormats.Add(new TagLibFormat());
            dataFormats.Add(new PdfFormat());
            dataFormats.Add(new IsoFormat());
            dataFormats.Add(new DosModuleExeFormat());
            dataFormats.Add(new DosModuleComFormat());
            dataFormats.Add(new Win32ModuleFormatManaged());
            dataFormats.Add(new OleStorageFormat());
            dataFormats.Add(new ShockwaveFlashFormat());
            dataFormats.Add(new WarcFormat());

            xmlFormats.Add(new SvgFormat());

            containerProviders.Add(new OpenPackageFormat());
            containerProviders.Add(new ExcelXmlDocumentFormat());
            containerProviders.Add(new ExcelDocumentFormat());
            containerProviders.Add(new WordXmlDocumentFormat());
            containerProviders.Add(new WordDocumentFormat());

            analyzers.Add(new FileSystemAnalyzer());
            analyzers.Add(new ImageMetadataAnalyzer());
            analyzers.Add(new ExifMetadataAnalyzer());
            analyzers.Add(new XmpMetadataAnalyzer());
            analyzers.Add(new TagLibAnalyzer());
            analyzers.Add(new XmpTagAnalyzer());
            analyzers.Add(new DosModuleAnalyzer());
            analyzers.Add(new SvgAnalyzer());
            analyzers.Add(new CabinetAnalyzer());
            analyzers.Add(new OleStorageAnalyzer());
            analyzers.Add(new OleDocumentAnalyzer());
            analyzers.Add(new OpenXmlDocumentAnalyzer());
            analyzers.Add(new PdfAnalyzer());
            analyzers.Add(new ShockwaveFlashAnalyzer());
            analyzers.Add(new WarcAnalyzer());
        }
    }
}
