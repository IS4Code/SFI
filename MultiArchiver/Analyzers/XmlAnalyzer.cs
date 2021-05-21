using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : BinaryFormatAnalyzer<XmlReader>
    {
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new SortedSet<IXmlDocumentFormat>(TypeInheritanceComparer<IXmlDocumentFormat>.Instance);

        public XmlAnalyzer() : base(Classes.ContentAsXML, Classes.Document)
        {

        }

        public override string Analyze(ILinkedNode node, XmlReader reader, object source, ILinkedNodeFactory nodeFactory)
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
                        var name = reader.Name;
                        var pubid = reader.GetAttribute("PUBLIC");
                        var sysid = reader.GetAttribute("SYSTEM");
                        if(!String.IsNullOrEmpty(pubid))
                        {
                            var dtd = nodeFactory.Create(UriTools.PublicIdFormatter, pubid);
                            dtd.SetClass(Classes.DoctypeDecl);
                            if(name != null)
                            {
                                dtd.Set(Properties.DoctypeName, name);
                            }
                            dtd.Set(Properties.PublicId, pubid);
                            if(sysid != null)
                            {
                                dtd.Set(Properties.SystemId, sysid, Datatypes.AnyUri);
                            }
                            node.Set(Properties.DtDecl, dtd);
                        }
                        docType = new XDocumentType(name, pubid, sysid, reader.Value);
                        break;
                    case XmlNodeType.Element:
                        var elem = node[reader.LocalName];
                        elem.SetClass(Classes.Element);
                        elem.Set(Properties.LocalName, reader.LocalName);
                        if(!String.IsNullOrEmpty(reader.Prefix))
                        {
                            elem.Set(Properties.XmlPrefix, reader.Prefix);
                        }
                        elem.Set(Properties.XmlName, reader.Name);
                        if(!String.IsNullOrEmpty(reader.NamespaceURI))
                        {
                            elem.Set(Properties.NamespaceName, reader.NamespaceURI, Datatypes.AnyUri);
                            elem.Set(Properties.SeeAlso, UriFormatter.Instance, reader.NamespaceURI);
                        }
                        node.Set(Properties.DocumentElement, elem);
                        foreach(var format in XmlFormats.Concat(new[] { ImprovisedXmlFormat.Instance }))
                        {
                            var resultFactory = new ResultFactory(node, source, format, nodeFactory);
                            var result = format.Match(reader, null, docType, resultFactory);
                            if(result != null)
                            {
                                result.Set(Properties.HasFormat, node);
                                return null;
                            }
                        }
                        return null;
                }
            }while(reader.Read());
            return null;
        }

        class ResultFactory : IGenericFunc<ILinkedNode>
        {
            readonly ILinkedNode parent;
            readonly IXmlDocumentFormat format;
            readonly ILinkedNodeFactory nodeFactory;
            readonly object source;

            public ResultFactory(ILinkedNode parent, object source, IXmlDocumentFormat format, ILinkedNodeFactory nodeFactory)
            {
                this.parent = parent;
                this.source = source;
                this.format = format;
                this.nodeFactory = nodeFactory;
            }

            ILinkedNode IGenericFunc<ILinkedNode>.Invoke<T>(T value)
            {
                return nodeFactory.Create(parent, new FormatObject<T, IXmlDocumentFormat>(format, value, source));
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

            static readonly Regex badCharacters = new Regex(@"://|//|[^a-zA-Z0-9._-]", RegexOptions.Compiled);

            public override string GetMediaType(XmlFormat value)
            {
                var ns = GetNamespace(value);
                if(ns == null)
                {
                    var pub = GetPublicId(value);
                    if(pub != null)
                    {
                        ns = UriTools.CreatePublicId(pub);
                    }
                }
                if(ns == null)
                {
                    return $"application/x.ns.{value.RootName.Name}+xml";
                }
                if(ns.HostNameType == UriHostNameType.Dns && !String.IsNullOrEmpty(ns.IdnHost))
                {
                    var host = ns.IdnHost;
                    var builder = new UriBuilder(ns);
                    builder.Host = String.Join(".", host.Split('.').Reverse());
                    if(!ns.Authority.EndsWith($":{builder.Port}", StringComparison.Ordinal))
                    {
                        builder.Port = -1;
                    }
                    ns = builder.Uri;
                }
                var replaced = badCharacters.Replace(ns.OriginalString, m => {
                    switch(m.Value)
                    {
                        case "%": return "&";
                        case ":":
                        case "/":
                        case "?":
                        case ";":
                        case "&":
                        case "=":
                        case "//":
                        case "://": return ".";
                        default: return String.Join("", Encoding.UTF8.GetBytes(m.Value).Select(b => $"&{b:X2}"));
                    }
                });
                return $"application/x.ns.{replaced}.{value.RootName.Name}+xml";
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
