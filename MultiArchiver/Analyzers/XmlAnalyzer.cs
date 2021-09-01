using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Tools.Xml;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : MediaObjectAnalyzer<XmlReader>, IResultFactory<AnalysisResult, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>
    {
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new SortedSet<IXmlDocumentFormat>(TypeInheritanceComparer<IXmlDocumentFormat>.Instance);

        public XmlAnalyzer() : base(Classes.ContentAsXML, Classes.Document)
        {

        }

        public override AnalysisResult Analyze(XmlReader reader, AnalysisContext context, IEntityAnalyzerProvider globalAnalyzer)
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

                        context = context.WithNode(null);

                        var rootState = new XmlReaderState(reader);

                        var formats = XmlFormats.Where(fmt => fmt.CheckDocument(docType, rootState)).ToList();

                        bool MatchFormat(IXmlDocumentFormat format, XmlReader localReader)
                        {
                            var result = format.Match(localReader, docType, context.MatchContext, this, (format, context, globalAnalyzer));
                            if(result.Node != null)
                            {
                                result.Node.Set(Properties.HasFormat, node);
                                return true;
                            }
                            return false;
                        }

                        bool any = false;
                        if(formats.Count <= 1)
                        {
                            foreach(var format in formats)
                            {
                                if(MatchFormat(format, reader))
                                {
                                    any = true;
                                }
                            }
                        }else{
                            var tasks = new Task<bool>[formats.Count];
                            var writers = new ChannelWriter<XmlReaderState>[formats.Count];
                            for(int i = 0; i < formats.Count; i++)
                            {
                                var channelReader = ChannelXmlReader.Create(reader, out writers[i]);
                                var format = formats[i];
                                tasks[i] = Task.Run(() => MatchFormat(format, channelReader));
                            }

                            foreach(var state in new[] { rootState }.Concat(XmlReaderState.ReadFrom(reader)))
                            {
                                foreach(var writer in writers)
                                {
                                    var task = writer.WriteAsync(state);
                                    if(!task.IsCompleted) task.AsTask().Wait();
                                }
                            }
                            foreach(var writer in writers)
                            {
                                writer.TryComplete();
                            }

                            Task.WaitAll(tasks);

                            any = tasks.Any(t => t.Result);
                        }
                        if(!any)
                        {
                            MatchFormat(ImprovisedXmlFormat.Instance, rootState);
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

        AnalysisResult IResultFactory<AnalysisResult, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzerProvider analyzer)>.Invoke<T>(T value, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzerProvider analyzer) args)
        {
            var (format, context, analyzer) = args;
            try{
                var obj = new FormatObject<T>(format, value);
                return analyzer.Analyze(obj, context);
            }catch(Exception e)
            {
                throw new InternalArchiverException(e);
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

            public override bool CheckDocument(XDocumentType docType, XmlReader rootReader)
            {
                return true;
            }

            public override TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext matchContext, ResultFactory<XmlFormat, TResult, TArgs> resultFactory, TArgs args)
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
