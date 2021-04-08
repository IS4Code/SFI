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

        public override ILinkedNode Analyze(XmlReader reader, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(reader, nodeFactory);
            if(node != null)
            {
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
                            dtd.Set(Classes.DoctypeDecl);
                            var name = reader.GetAttribute("Name");
                            if(name != null)
                            {
                                dtd.Set(Properties.DoctypeName, name);
                            }
                            var pubid = reader.GetAttribute("PUBLIC");
                            if(pubid != null)
                            {
                                dtd.Set(Properties.PublicId, pubid);
                            }
                            var sysid = reader.GetAttribute("SYSTEM");
                            if(sysid != null)
                            {
                                dtd.Set(Properties.PublicId, sysid, Datatypes.AnyURI);
                            }
                            break;
                        case XmlNodeType.Element:
                            return node;
                    }
                }while(reader.Read());
            }
            return node;
        }
    }
}
