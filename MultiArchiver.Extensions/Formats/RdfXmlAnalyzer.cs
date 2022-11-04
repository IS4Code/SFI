﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools.Xml;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzer of RDF/XML documents, as instances of <see cref="Document"/>.
    /// The analyzer uses the data in the document to describe its node,
    /// assuming URIs relative to the XML base are used.
    /// </summary>
    public class RdfXmlAnalyzer : MediaObjectAnalyzer<RdfXmlAnalyzer.Document>
    {
        public async override ValueTask<AnalysisResult> Analyze(Document entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            node.Describe(entity.RdfDocument);
            return new AnalysisResult(node);
        }

        /// <summary>
        /// A representation of an RDF/XML document.
        /// </summary>
        public class Document
        {
            /// <summary>
            /// The XML document storing the RDF/XML data. The instance
            /// of <see cref="BaseXmlDocument"/> allows modifying the base
            /// of the document at runtime based on the URI of the current node.
            /// </summary>
            public BaseXmlDocument RdfDocument { get; }

            /// <summary>
            /// Creates a new instance of the document.
            /// </summary>
            /// <param name="rdfDocument">The value of <see cref="RdfDocument"/>.</param>
            public Document(BaseXmlDocument rdfDocument)
            {
                RdfDocument = rdfDocument;
            }

            public override string ToString()
            {
                return RdfDocument.ToString();
            }
        }
    }
}
