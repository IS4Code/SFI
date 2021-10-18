using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Tools.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class RdfXmlFormat : XmlDocumentFormat<RdfXmlAnalyzer.Document>
    {
        public RdfXmlFormat() : base(null, null, new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"), "application/rdf+xml", "rdf")
        {

        }

        public override TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, ResultFactory<RdfXmlAnalyzer.Document, TResult, TArgs> resultFactory, TArgs args)
        {
            var document = new BaseXmlDocument(null, reader.NameTable);
            document.Load(reader);
            return resultFactory.Invoke(new RdfXmlAnalyzer.Document(document), args);
        }
    }
}
