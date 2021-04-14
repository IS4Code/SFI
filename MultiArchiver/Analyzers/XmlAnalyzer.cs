using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : FormatAnalyzer<XmlReader>
    {
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new List<IXmlDocumentFormat>();

        public XmlAnalyzer() : base(Classes.ContentAsXML)
        {

        }

        public override bool Analyze(ILinkedNode node, XmlReader reader, ILinkedNodeFactory nodeFactory)
        {
            XDocumentType docType = null;
            do
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.XmlDeclaration:
                        var version = reader.GetAttribute("version");
                        if(version != null)
                        {
                            node.Set(Properties.XmlVersion, version);
                        }
                        var encoding = reader.GetAttribute("encoding");
                        if(encoding != null)
                        {
                            node.Set(Properties.XmlEncoding, encoding);
                        }
                        var standalone = reader.GetAttribute("standalone");
                        if(standalone != null)
                        {
                            node.Set(Properties.XmlStandalone, standalone);
                        }
                        break;
                    case XmlNodeType.DocumentType:
                        var dtd = node["doctype"];
                        dtd.SetClass(Classes.DoctypeDecl);
                        var name = reader.Name;
                        if(name != null)
                        {
                            dtd.Set(Properties.DoctypeName, name);
                        }
                        var pubid = reader.GetAttribute("PUBLIC");
                        if(pubid != null)
                        {
                            dtd.Set(Properties.PublicId, pubid);
                            node.SetClass(UriTools.PublicIdFormatter.Instance, pubid);
                        }
                        var sysid = reader.GetAttribute("SYSTEM");
                        if(sysid != null)
                        {
                            dtd.Set(Properties.PublicId, sysid, Datatypes.AnyURI);
                        }
                        node.Set(Properties.DtDecl, dtd);
                        docType = new XDocumentType(name, pubid, sysid, reader.Value);
                        break;
                    case XmlNodeType.Element:
                        foreach(var format in XmlFormats)
                        {
                            if(format.Match(reader, docType, nodeFactory) is ILinkedNode node2)
                            {
                                node2.Set(Properties.HasFormat, node);
                                break;
                            }
                        }
                        return false;
                }
            }while(reader.Read());
            return false;
        }
    }
}
