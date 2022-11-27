using IS4.SFI.Analyzers;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System.Collections.Generic;

namespace IS4.SFI
{
    /// <summary>
    /// Provides constructions of components in this project.
    /// </summary>
    public static class BaseFormats
    {
        /// <summary>
        /// Creates the provided components and adds them to the supplied collections.
        /// </summary>
        /// <param name="analyzers">The collection of analyzers, implementing <see cref="IEntityAnalyzer{T}"/>.</param>
        /// <param name="dataFormats">The collection of formats for <see cref="DataAnalyzer"/>.</param>
        /// <param name="xmlFormats">The collection of formats for <see cref="XmlAnalyzer"/>.</param>
        /// <param name="containerProviders">The collection of container providers, implementing <see cref="IContainerAnalyzerProvider"/>.</param>
        public static void AddDefault(ICollection<object> analyzers, ICollection<IBinaryFileFormat> dataFormats, ICollection<IXmlDocumentFormat> xmlFormats, ICollection<IContainerAnalyzerProvider> containerProviders)
        {
            dataFormats.Add(new GenericModuleFormat());
            dataFormats.Add(new LinearModuleFormat());
            dataFormats.Add(new Win16ModuleFormat());
            dataFormats.Add(new DelphiFormFormat());

            containerProviders.Add(new PackageDescriptionProvider());

            analyzers.Add(new ArchiveAnalyzer());
            analyzers.Add(new ArchiveReaderAnalyzer());
            analyzers.Add(new WinModuleAnalyzer());
            analyzers.Add(new WinVersionAnalyzerManaged());
            analyzers.Add(new DelphiObjectAnalyzer());
            analyzers.Add(new PackageDescriptionAnalyzer());
        }
    }
}
