using HtmlAgilityPack;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzer of HTML documents, as instances of <see cref="HtmlDocument"/>.
    /// </summary>
    [Description("Analyzer of HTML documents.")]
    public class HtmlAnalyzer : MediaObjectAnalyzer<HtmlDocument>
    {
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
            return new AnalysisResult(node);
        }

        static LanguageCode? GetLanguage(HtmlNode node)
        {
            var langNodes = node.SelectNodes("ancestor-or-self::*[@lang]");
            if(langNodes == null || langNodes.Count == 0) return null;
            var lang = langNodes[langNodes.Count - 1].GetAttributeValue("lang", null);
            if(String.IsNullOrEmpty(lang)) return null;
            return new LanguageCode(lang);
        }
    }
}
