﻿using HtmlAgilityPack;
using IS4.SFI.RDF;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An RDFa analyzer derived from <see cref="HtmlAnalyzer"/>.
    /// </summary>
    [Description("An RDFa analyzer based on HTML.")]
    public class RdfAHtmlAnalyzer : HtmlAnalyzer
    {
        /// <summary>
        /// The RDFa parser used for HTML.
        /// </summary>
        public RdfAParserBase<HtmlDocument, HtmlNode, HtmlNode, HtmlAttribute> RdfAParser { get; } = new Parser(RdfASyntax.AutoDetect);

        /// <summary>
        /// The RDF/XML writer used when serializing RDFa data.
        /// </summary>
        public VDS.RDF.Writing.RdfXmlWriter RdfXmlWriter { get; } = new()
        {
            UseDtd = false,
            PrettyPrintMode = false
        };

        static string? initialContext;

        /// <summary>
        /// Whether to ignore any parsing exceptions when loading RDFa data.
        /// </summary>
        [Description("Whether to ignore any parsing exceptions when loading RDFa data.")]
        public bool SuppressParsingErrors { get; set; } = true;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public RdfAHtmlAnalyzer() : base()
        {

        }

        /// <inheritdoc/>
        public override async ValueTask<AnalysisResult> Analyze(HtmlDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var result = await base.Analyze(document, context, analyzers);
            if(result.Node is ILinkedNode node)
            {
                try{
                    initialContext = await RdfAInitialContext.PrefixString;
                    var reader = new Parser.DummyReader(document);
                    if(!node.TryDescribe(RdfAParser, baseUri => {
                        Parser.SetBase(document, baseUri);
                        return reader;
                    }))
                    {
                        node.Describe(uri => {
                            var graph = new Graph(true);
                            graph.BaseUri = uri;
                            RdfAParser.Load(graph, reader);
                            var buffer = new MemoryStream();
                            RdfXmlWriter.Save(graph, new StreamWriter(buffer, Encoding.UTF8), true);
                            buffer.Position = 0;
                            var xmlReader = XmlReader.Create(new StreamReader(buffer, Encoding.UTF8));
                            if(xmlReader.MoveToContent() == XmlNodeType.Element) return xmlReader;
                            return null;
                        });
                    }
                }catch(RdfParseException) when(SuppressParsingErrors)
                {

                }catch(FormatException) when(SuppressParsingErrors)
                {

                }
            }
            return result;
        }

        /// <summary>
        /// A custom subclass of <see cref="VDS.RDF.Parsing.RdfAParser"/>
        /// that supports loading a pre-existing document, and allows
        /// setting the base of <see cref="HtmlDocument"/>.
        /// </summary>
        class Parser : RdfAParser
        {
            static readonly ConditionalWeakTable<HtmlDocument, HtmlNode> baseElements = new();

            public Parser(RdfASyntax syntax) : base(syntax)
            {

            }

            protected override HtmlDocument LoadAndParse(TextReader input)
            {
                if(input is DummyReader reader)
                {
                    return reader.Document;
                }
                var doc = new HtmlDocument();
                doc.Load(input);
                return doc;
            }

            public static void SetBase(HtmlDocument document, Uri uri)
            {
                var elem = document.CreateElement("base");
                elem.SetAttributeValue("href", uri.AbsoluteUri);
                baseElements.Add(document, elem);
            }

            protected override HtmlNode GetBaseElement(HtmlDocument document)
            {
                if(baseElements.TryGetValue(document, out var baseElement))
                {
                    return baseElement;
                }
                return base.GetBaseElement(document);
            }

            protected override string GetAttribute(HtmlNode element, string attributeName)
            {
                var value = base.GetAttribute(element, attributeName);
                if(GetElementName(element) == "html" && attributeName == "prefix")
                {
                    value = initialContext + " " + value;
                }
                return value;
            }

            protected override IEnumerable<HtmlAttribute> GetAttributes(HtmlNode element)
            {
                if(GetElementName(element) == "html")
                {
                    var attributes = base.GetAttributes(element).ToList();
                    var prefixAttr = attributes.FirstOrDefault(attr => GetAttributeName(attr) == "prefix");
                    if(prefixAttr == null)
                    {
                        SetAttribute(element, "prefix", "");
                        return base.GetAttributes(element);
                    }
                    return attributes;
                }
                return base.GetAttributes(element);
            }

            protected override string GetAttributeValue(HtmlAttribute attribute)
            {
                var value = base.GetAttributeValue(attribute);
                if(GetAttributeName(attribute) == "prefix" && GetElementName(attribute.OwnerNode) == "html")
                {
                    value = initialContext + " " + value;
                }
                return value;
            }

            /// <summary>
            /// A dummy subclass of <see cref="TextReader"/> whose
            /// purpose is to pass the <see cref="HtmlDocument"/>
            /// inside <see cref="LoadAndParse(TextReader)"/>.
            /// </summary>
            public class DummyReader : TextReader
            {
                public HtmlDocument Document { get; }

                public DummyReader(HtmlDocument document)
                {
                    Document = document;
                }
            }
        }
    }
}
