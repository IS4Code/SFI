using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Tools.Xml;
using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the RDF/XML format, producing instances of <see cref="RdfXmlAnalyzer.Document"/>.
    /// </summary>
    public class RdfXmlFormat : XmlDocumentFormat<RdfXmlAnalyzer.Document>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public RdfXmlFormat() : base(null, null, new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"), "application/rdf+xml", "rdf")
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, ResultFactory<RdfXmlAnalyzer.Document, TResult, TArgs> resultFactory, TArgs args)
        {
            var document = new BaseXmlDocument(null, reader.NameTable);
            document.Load(reader);
            return await resultFactory.Invoke(new RdfXmlAnalyzer.Document(document), args);
        }
    }
}
