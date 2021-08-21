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
    public class XmlAnalyzer : MediaObjectAnalyzer<XmlReader>
    {
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new SortedSet<IXmlDocumentFormat>(TypeInheritanceComparer<IXmlDocumentFormat>.Instance);

        public XmlAnalyzer() : base(Classes.ContentAsXML, Classes.Document)
        {

        }

        public override AnalysisResult Analyze(XmlReader reader, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
        {
            var node = GetNode(context);

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
                            var dtd = context.NodeFactory.Create(UriTools.PublicIdFormatter, pubid);
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
                        var xmlName = reader.Name;
                        elem.Set(Properties.XmlName, xmlName);
                        if(!String.IsNullOrEmpty(reader.NamespaceURI))
                        {
                            elem.Set(Properties.NamespaceName, reader.NamespaceURI, Datatypes.AnyUri);
                            try{
                                elem.Set(Properties.SeeAlso, UriFormatter.Instance, reader.NamespaceURI);
                            }catch(UriFormatException)
                            {
                                try{
                                    elem.Set(Properties.SeeAlso, UriTools.PublicIdFormatter, XmlConvert.VerifyPublicId(reader.NamespaceURI));
                                }catch(XmlException)
                                {

                                }
                            }
                        }
                        node.Set(Properties.DocumentElement, elem);

                        bool any = false;
                        foreach(var format in XmlFormats)
                        {
                            var resultFactory = new ResultFactory(format, context.WithParent(node), globalAnalyzer);

                            if(format.Match(reader, docType, null, resultFactory, default) is AnalysisResult result)
                            {
                                any = true;
                                result.Node.Set(Properties.HasFormat, node);
                            }
                        }

                        if(!any)
                        {
                            var resultFactory = new ResultFactory(ImprovisedXmlFormat.Instance, context.WithParent(node), globalAnalyzer);
                            var result = ImprovisedXmlFormat.Instance.Match(reader, docType, null, resultFactory, default).Value;
                            result.Node.Set(Properties.HasFormat, node);
                        }

                        return new AnalysisResult(node, xmlName);
                }
            }while(ReadSafe(reader));

            throw new InvalidOperationException();
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

        class ResultFactory : IResultFactory<AnalysisResult?, ValueTuple>
        {
            readonly IXmlDocumentFormat format;
            readonly AnalysisContext context;
            readonly IEntityAnalyzer analyzer;

            public ResultFactory(IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzer analyzer)
            {
                this.format = format;
                this.context = context;
                this.analyzer = analyzer;
            }

            AnalysisResult? IResultFactory<AnalysisResult?, ValueTuple>.Invoke<T>(T value, ValueTuple args)
            {
                try{
                    var obj = new FormatObject<T>(format, value);
                    return analyzer.Analyze(obj, context);
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
                var ns = value.RootName?.Namespace;
                if(!String.IsNullOrEmpty(ns))
                {
                    try{
                        return new Uri(ns, UriKind.Absolute);
                    }catch(UriFormatException)
                    {
                        try{
                            return UriTools.CreatePublicId(XmlConvert.VerifyPublicId(ns));
                        }catch(XmlException)
                        {
                            return base.GetNamespace(value);
                        }
                    }
                }
                return base.GetNamespace(value);
            }

            public override string GetMediaType(XmlFormat value)
            {
                return DataTools.GetFakeMediaTypeFromXml(GetNamespace(value), GetPublicId(value), value.RootName.Name);
            }

            public override string GetExtension(XmlFormat value)
            {
                return value.RootName?.Name ?? base.GetExtension(value);
            }

            public override TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, ResultFactory<XmlFormat, TResult, TArgs> resultFactory, TArgs args)
            {
                return resultFactory(new XmlFormat(reader, docType), args);
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
