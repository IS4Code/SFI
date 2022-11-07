using IS4.MultiArchiver.Services;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TagLib.Xmp;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// An analyzer of XMP metadata tags, as instances
    /// of <see cref="XmpTag"/>.
    /// </summary>
    public class XmpTagAnalyzer : EntityAnalyzer<XmpTag>
    {
        public async override ValueTask<AnalysisResult> Analyze(XmpTag tag, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var doc = new XmlDocument();
            var meta = doc.CreateElement("x", DataTools.XmpMetaName.Name, DataTools.XmpMetaName.Namespace);
            var rdf = doc.CreateElement("rdf", DataTools.RdfName.Name, DataTools.RdfName.Namespace);
            var description = doc.CreateElement("rdf", "Description", DataTools.RdfName.Namespace);
            tag.NodeTree.RenderInto(description);
            var about = doc.CreateAttribute("rdf", "about", DataTools.RdfName.Namespace);
            about.Value = "";
            description.SetAttributeNode(about);
            rdf.AppendChild(description);
            meta.AppendChild(rdf);
            doc.AppendChild(meta);

            // XmlNodeReader is not used because namespaces are not properly mapped for inner elements
            DataTools.DescribeAsXmp(node, new StringReader(doc.OuterXml));

            return new AnalysisResult(node);
        }
    }
}
