using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : BinaryFormatAnalyzer<XmlReader>
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
                        foreach(var format in XmlFormats.Concat(new[] { ImprovisedXmlFormat.Instance }))
                        {
                            var resultFactory = new ResultFactory(node, format, nodeFactory);
                            var result = format.Match(reader, docType, resultFactory);
                            if(result != null)
                            {
                                result.Set(Properties.HasFormat, node);
                                return false;
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
            readonly IXmlDocumentFormat format;
            readonly ILinkedNodeFactory nodeFactory;

            public ResultFactory(ILinkedNode parent, IXmlDocumentFormat format, ILinkedNodeFactory nodeFactory)
            {
                this.parent = parent;
                this.format = format;
                this.nodeFactory = nodeFactory;
            }

            ILinkedNode IGenericFunc<ILinkedNode>.Invoke<T>(T value)
            {
                return nodeFactory.Create(parent, new FormatObject<T, IXmlDocumentFormat>(format, value));
            }
        }

        class ImprovisedXmlFormat : XmlDocumentFormat<ImprovisedXmlFormat.XmlFormat>
        {
            public static readonly ImprovisedXmlFormat Instance = new ImprovisedXmlFormat();

            private ImprovisedXmlFormat() : base(null, null, null, null, null)
            {

            }

            public override string GetPublicId(XmlFormat value)
            {
                if(!String.IsNullOrEmpty(value.DocType?.PublicId))
                {
                    return value.DocType.PublicId;
                }
                return base.GetPublicId(value);
            }

            public override string GetSystemId(XmlFormat value)
            {
                if(!String.IsNullOrEmpty(value.DocType?.SystemId))
                {
                    return value.DocType.SystemId;
                }
                return base.GetSystemId(value);
            }

            public override Uri GetNamespace(XmlFormat value)
            {
                if(!String.IsNullOrEmpty(value.RootName?.Namespace))
                {
                    return new Uri(value.RootName.Namespace, UriKind.Absolute);
                }
                return base.GetNamespace(value);
            }

            public override TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<XmlFormat, TResult> resultFactory)
            {
                return resultFactory(new XmlFormat(reader, docType));
            }

            public class XmlFormat
            {
                public XDocumentType DocType { get; }
                public XmlQualifiedName RootName { get; }

                public XmlFormat(XmlReader reader, XDocumentType docType)
                {
                    DocType = docType;
                    RootName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);
                }
            }
        }
    }
}
