using HtmlAgilityPack;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VDS.RDF.Parsing;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzer of RDF/XML documents, as instances of <see cref="HtmlDocument"/>.
    /// </summary>
    public class HtmlAnalyzer : MediaObjectAnalyzer<HtmlDocument>
    {
        /// <summary>
        /// The RDFa parser used for HTML.
        /// </summary>
        public RdfAParserBase<HtmlDocument, HtmlNode, HtmlNode, HtmlAttribute> RdfAParser { get; } = new Parser(RdfASyntax.AutoDetect);

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public HtmlAnalyzer() : base(Common.DocumentClasses)
        {

        }

        /// <inheritdoc/>
        public override async ValueTask<AnalysisResult> Analyze(HtmlDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            foreach(var title in document.DocumentNode.SelectNodes("//head/title") ?? (IEnumerable<HtmlNode>)Array.Empty<HtmlNode>())
            {
                if(GetLanguage(title) is LanguageCode lang)
                {
                    node.Set(Properties.Title, title.InnerText, lang);
                }else{
                    node.Set(Properties.Title, title.InnerText);
                }
            }
            node.TryDescribe(RdfAParser, baseUri => {
                Parser.SetBase(document, baseUri);
                return new Parser.DummyReader(document);
            });
            return new AnalysisResult(node);
        }

        static LanguageCode? GetLanguage(HtmlNode node)
        {
            var langNodes = node.SelectNodes("ancestor-or-self::*[@lang]");
            if(langNodes == null || langNodes.Count == 0) return null;
            var lang = langNodes[langNodes.Count - 1].GetAttributeValue("lang", null);
            if(lang == null) return null;
            return new LanguageCode(lang);
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
