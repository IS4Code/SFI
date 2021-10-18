using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools.Xml;

namespace IS4.MultiArchiver.Analyzers
{
    public class RdfXmlAnalyzer : MediaObjectAnalyzer<RdfXmlAnalyzer.Document>
    {
        public override AnalysisResult Analyze(Document entity, AnalysisContext context, IEntityAnalyzerProvider analyzers)
        {
            var node = GetNode(context);
            node.Describe(entity.RdfDocument);
            return new AnalysisResult(node);
        }

        public class Document
        {
            public BaseXmlDocument RdfDocument { get; }

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
