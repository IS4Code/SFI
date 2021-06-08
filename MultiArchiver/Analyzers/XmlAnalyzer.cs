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

        public XmlAnalyzer() : base(Classes.ContentAsXML, Classes.Document)
        {

        }

        public override string Analyze(ILinkedNode parent, ILinkedNode node, XmlReader reader, object source, ILinkedNodeFactory nodeFactory)
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
                        var xmlMame = reader.Name;
                        elem.Set(Properties.XmlName, xmlMame);
                        if(!String.IsNullOrEmpty(reader.NamespaceURI))
                        {
                            elem.Set(Properties.NamespaceName, reader.NamespaceURI, Datatypes.AnyUri);
                            elem.Set(Properties.SeeAlso, UriFormatter.Instance, reader.NamespaceURI);
                        }
                        node.Set(Properties.DocumentElement, elem);
                        foreach(var format in XmlFormats.Concat(new[] { ImprovisedXmlFormat.Instance }))
                        {
                            var resultFactory = new ResultFactory(parent, source, format, nodeFactory);
                            var result = format.Match(reader, null, docType, resultFactory);
                            if(result != null)
                            {
                                result.Set(Properties.HasFormat, node);

                                if(resultFactory.Extension is string extension)
                                {
                                    if(resultFactory.Label is string formatLabel)
                                    {
                                        result.Set(Properties.PrefLabel, $"{extension.ToUpperInvariant()} object ({formatLabel})", "en");
                                    }else{
                                        result.Set(Properties.PrefLabel, $"{extension.ToUpperInvariant()} object", "en");
                                    }
                                }
                                break;
                            }
                        }
                        return xmlMame;
                }
            }while(ReadSafe(reader));
            return null;
        }

        static bool ReadSafe(XmlReader reader)
        {
            try{
                return reader.Read();
            }catch(XmlException)
            {
                return false;
            }
        }

        class ResultFactory : IResultFactory<ILinkedNode>
        {
            readonly ILinkedNode parent;
            readonly IXmlDocumentFormat format;
            readonly ILinkedNodeFactory nodeFactory;
            readonly object source;

            public string Extension { get; private set; }
            public string Label { get; private set; }

            public ResultFactory(ILinkedNode parent, object source, IXmlDocumentFormat format, ILinkedNodeFactory nodeFactory)
            {
                this.parent = parent;
                this.source = source;
                this.format = format;
                this.nodeFactory = nodeFactory;
            }

            ILinkedNode IResultFactory<ILinkedNode>.Invoke<T>(T value)
            {
                try{
                    var obj = new FormatObject<T, IXmlDocumentFormat>(format, value, source);
                    var result = nodeFactory.Create(parent, obj);
                    Extension = obj.Extension;
                    Label = obj.Label;
                    return result;
                }catch(Exception e)
                {
                    throw new InternalArchiverException(e);
                }
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

            public override string GetMediaType(XmlFormat value)
            {
                return DataTools.GetFakeMediaTypeFromXml(GetNamespace(value), GetPublicId(value), value.RootName.Name);
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
