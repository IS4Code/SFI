using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Xml;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : ClassRecognizingAnalyzer<XmlReader>
    {
        public XmlAnalyzer() : base(Classes.ContentAsXML)
        {

        }

        public override IRdfEntity Analyze(XmlReader entity, IRdfAnalyzer analyzer)
        {
            var node = base.Analyze(entity, analyzer);
            if(node != null)
            {

            }
            return node;
        }
    }
}
