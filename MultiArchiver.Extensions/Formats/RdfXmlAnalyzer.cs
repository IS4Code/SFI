using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools.Xml;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    public class RdfXmlAnalyzer : MediaObjectAnalyzer<RdfXmlAnalyzer.Document>
    {
        public override ValueTask<AnalysisResult> Analyze(Document entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            node.Describe(entity.RdfDocument);
            return new ValueTask<AnalysisResult>(new AnalysisResult(node));
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
