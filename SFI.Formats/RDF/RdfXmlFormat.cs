using IS4.SFI.Analyzers;
using IS4.SFI.Tools.Xml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the RDF/XML format, producing instances of <see cref="RdfXmlAnalyzer.Document"/>.
    /// </summary>
    [Description("Represents the RDF/XML format.")]
    public class RdfXmlFormat : XmlDocumentFormat<RdfXmlAnalyzer.Document>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public RdfXmlFormat() : base(null, null, new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"), "application/rdf+xml", "rdf")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, ResultFactory<RdfXmlAnalyzer.Document, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var document = new BaseXmlDocument(reader.NameTable);
            document.Load(reader);
            return await resultFactory.Invoke(new RdfXmlAnalyzer.Document(document), args);
        }
    }
}
