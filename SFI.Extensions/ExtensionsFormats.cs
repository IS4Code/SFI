using IS4.SFI.Analyzers;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System.Collections.Generic;

namespace IS4.SFI
{
    /// <inheritdoc cref="BaseFormats"/>
    public static class ExtensionsFormats
    {
        /// <inheritdoc cref="BaseFormats.AddDefault(ICollection{object}, ICollection{IBinaryFileFormat}, ICollection{IXmlDocumentFormat}, ICollection{IContainerAnalyzerProvider})"/>
        public static void AddDefault(ICollection<object> analyzers, ICollection<IBinaryFileFormat> dataFormats, ICollection<IXmlDocumentFormat> xmlFormats, ICollection<IContainerAnalyzerProvider> containerProviders)
        {
            dataFormats.Add(new HtmlFormat());

            xmlFormats.Add(new RdfXmlFormat());

            analyzers.Add(new HtmlAnalyzer());
            analyzers.Add(new RdfXmlAnalyzer());
        }
    }
}