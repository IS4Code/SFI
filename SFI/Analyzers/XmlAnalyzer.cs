﻿using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using IS4.SFI.Tools.Xml;
using IS4.SFI.Vocabulary;
using MorseCode.ITask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of XML documents, expressed using the common class <see cref="XmlReader"/>.
    /// </summary>
    public class XmlAnalyzer : MediaObjectAnalyzer<XmlReader>, IResultFactory<AnalysisResult, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzers analyzer)>
    {
        /// <summary>
        /// A collection of XML formats, as instances of <see cref="IXmlDocumentFormat"/>,
        /// to use when recognizing the format of the document.
        /// </summary>
        [ComponentCollection("xml-format")]
        public ICollection<IXmlDocumentFormat> XmlFormats { get; } = new SortedSet<IXmlDocumentFormat>(TypeInheritanceComparer<IXmlDocumentFormat>.Instance);

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public XmlAnalyzer() : base(Classes.ContentAsXML, Classes.XmlDocument)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(XmlReader reader, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            XDocumentType? docType = null;
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
                            var dtd = node["#dtd()"];
                            node.Set(Properties.DtDecl, dtd);
                            dtd.SetClass(Classes.DoctypeDecl);
                            if(name != null)
                            {
                                dtd.Set(Properties.DoctypeName, name);
                            }
                            dtd.Set(Properties.PublicId, pubid);
                            // A URI is created from the PUBLIC identifier
                            var dtdParent = context.NodeFactory.Create(UriTools.PublicIdFormatter, pubid);
                            if(dtdParent != null)
                            {
                                dtd.Set(Properties.SeeAlso, dtdParent);
                            }
                            if(sysid != null)
                            {
                                dtd.Set(Properties.SystemId, sysid, Datatypes.AnyUri);
                                var definedNode = context.NodeFactory.Create(UriFormatter.Instance, sysid);
                                if(dtdParent != null && definedNode != null)
                                {
                                    // The SYSTEM identifier can be used to define the DTD
                                    dtdParent.Set(Properties.IsDefinedBy, definedNode);
                                }
                            }
                        }
                        docType = new XDocumentType(name, pubid, sysid, reader.Value);
                        break;
                    case XmlNodeType.Element:
                        // Describe the root element using the XIS vocabulary
                        var elem = node["#element(/1)"];
                        node.Set(Properties.DocumentElement, elem);
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

                        context = context.WithNode(null);

                        // Capture the state of the XmlReader
                        var rootState = new XmlReaderState(reader);

                        // Filter out formats not matching the DTD or root element
                        var formats = XmlFormats.Where(fmt => fmt.CheckDocument(docType, rootState)).ToList();

                        async Task<bool> MatchFormat(IXmlDocumentFormat format, XmlReader localReader)
                        {
                            var result = await format.Match(localReader, docType, context.MatchContext, this, (format, context, analyzers));
                            if(result.Node != null)
                            {
                                node.Set(Properties.HasFormat, result.Node);
                                return true;
                            }
                            return false;
                        }

                        bool any = false;
                        if(formats.Count <= 1)
                        {
                            foreach(var format in formats)
                            {
                                // For a single XML format, just give the reader to it
                                if(await MatchFormat(format, reader))
                                {
                                    any = true;
                                }
                            }
                        }else{
                            // For multiple formats, each shall get its own channel of replicated XML states
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
                                    await writer.WriteAsync(state);
                                }
                            }
                            foreach(var writer in writers)
                            {
                                writer.TryComplete();
                            }

                            await Task.WhenAll(tasks);

                            any = tasks.Any(t => t.Result);
                        }
                        if(!any)
                        {
                            await MatchFormat(ImprovisedXmlFormat.Instance, rootState);
                        }

                        return new AnalysisResult(node, xmlName);
                }
            }while(await ReadSafe(reader));

            throw new InvalidOperationException();
        }

        static async ValueTask<bool> ReadSafe(XmlReader reader)
        {
            try{
                return await reader.ReadAsync();
            }catch(XmlException)
            {
                return false;
            }
        }

        async ITask<AnalysisResult> IResultFactory<AnalysisResult, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzers analyzer)>.Invoke<T>(T value, (IXmlDocumentFormat format, AnalysisContext context, IEntityAnalyzers analyzer) args)
        {
            var (format, context, analyzer) = args;
            try{
                var obj = new FormatObject<T>(format, value);
                return await analyzer.Analyze(obj, context);
            }catch(InternalApplicationException)
            {
                throw;
            }catch(Exception e)
            {
                throw new InternalApplicationException(e);
            }
        }

        /// <summary>
        /// This is an improvised format implied by the root namespace or PUBLIC identifier
        /// referenced by the document.
        /// </summary>
        class ImprovisedXmlFormat : XmlDocumentFormat<ImprovisedXmlFormat.XmlFormat>
        {
            public static readonly ImprovisedXmlFormat Instance = new();

            private ImprovisedXmlFormat() : base(null, null, null, null, null)
            {

            }

            public override string? GetPublicId(XmlFormat value)
            {
                if(!String.IsNullOrEmpty(value.DocType?.PublicId))
                {
                    return value.DocType!.PublicId;
                }
                return base.GetPublicId(value);
            }

            public override string? GetSystemId(XmlFormat value)
            {
                if(!String.IsNullOrEmpty(value.DocType?.SystemId))
                {
                    return value.DocType!.SystemId;
                }
                return base.GetSystemId(value);
            }

            public override Uri? GetNamespace(XmlFormat value)
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

            /// <summary>
            /// The media type is produced by <see cref="TextTools.GetImpliedMediaTypeFromXml(Uri, string, string)"/>.
            /// </summary>
            public override string? GetMediaType(XmlFormat value)
            {
                return TextTools.GetImpliedMediaTypeFromXml(GetNamespace(value), GetPublicId(value), value.RootName.Name);
            }

            /// <summary>
            /// The extension is the local name of the root element.
            /// </summary>
            public override string? GetExtension(XmlFormat value)
            {
                return value.RootName?.Name ?? base.GetExtension(value);
            }

            public override bool CheckDocument(XDocumentType? docType, XmlReader rootReader)
            {
                return true;
            }

            public async override ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext matchContext, ResultFactory<XmlFormat, TResult, TArgs> resultFactory, TArgs args) where TResult : default
            {
                return await resultFactory(new XmlFormat(reader, docType), args);
            }

            /// <summary>
            /// A class storing all the necessary information to identify an
            /// implied XML format from the DTD and the name of the root element.
            /// </summary>
            public class XmlFormat
            {
                public XDocumentType? DocType { get; }
                public XmlQualifiedName RootName { get; }

                public XmlFormat(XmlReader reader, XDocumentType? docType)
                {
                    DocType = docType;
                    RootName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);
                }
            }
        }
    }
}
