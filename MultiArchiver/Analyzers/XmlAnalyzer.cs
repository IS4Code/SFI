using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : FormatAnalyzer<XmlReader>
    {
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new SortedSet<IXmlDocumentFormat>(TypeInheritanceComparer<IXmlDocumentFormat>.Instance);

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
                            var resultFactory = new ResultFactory(node, format, nodeFactory);
                            var result = format.Match(reader, docType, resultFactory);
                            if(result != null)
                            {
                                result.Set(Properties.HasFormat, node);
                                break;
                            }
                        }
                        return false;
                }
            }while(reader.Read());
            return false;
        }

        class ResultFactory : IGenericFunc<ILinkedNode>
        {
            readonly ILinkedNode parent;
            readonly IFileFormat format;
            readonly ILinkedNodeFactory nodeFactory;

            public ResultFactory(ILinkedNode parent, IFileFormat format, ILinkedNodeFactory nodeFactory)
            {
                this.parent = parent;
                this.format = format;
                this.nodeFactory = nodeFactory;
            }

            ILinkedNode IGenericFunc<ILinkedNode>.Invoke<T>(T value)
            {
                return nodeFactory.Create(parent, new FormatObject<T>(format, value));
            }
        }
    }
}
