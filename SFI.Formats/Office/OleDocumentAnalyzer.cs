﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using NPOI;
using System;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes OLE-based documents, as instances of <see cref="POIDocument"/>.
    /// </summary>
    public class OleDocumentAnalyzer : MediaObjectAnalyzer<POIDocument>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public OleDocumentAnalyzer() : base(Common.DocumentClasses)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(POIDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            string? label = null;
            var sum = document.SummaryInformation;
            if(sum != null)
            {
                if(IsDefined(sum.Author, out var creator))
                {
                    node.Set(Properties.Creator, creator);
                }
                if(IsDefined(sum.Subject, out var subject))
                {
                    node.Set(Properties.Subject, subject);
                }
                if(IsDefined(sum.LastSaveDateTime, out var modified))
                {
                    node.Set(Properties.Modified, modified);
                }
                if(IsDefined(sum.Keywords, out var keywords))
                {
                    node.Set(Properties.Keywords, keywords);
                }
                if(IsDefined(sum.RevNumber, out var revision))
                {
                    node.Set(Properties.Version, revision);
                }
                if(IsDefined(sum.CreateDateTime, out var created))
                {
                    node.Set(Properties.Created, created);
                }
                if(IsDefined(sum.Title, out var title))
                {
                    node.Set(Properties.Title, title);
                    label = title;
                }
                if(IsDefined(sum.CharCount, out var charCount))
                {
                    node.Set(Properties.CharacterCount, charCount);
                }
                if(IsDefined(sum.WordCount, out var wordCount))
                {
                    node.Set(Properties.WordCount, wordCount);
                }
                if(IsDefined(sum.PageCount, out var pageCount))
                {
                    node.Set(Properties.PageCount, pageCount);
                }
            }
            var doc = document.DocumentSummaryInformation;
            if(doc != null)
            {
                if(IsDefined(doc.Category, out var category))
                {
                    node.Set(Properties.Category, category);
                }
                if(IsDefined(doc.Language, out var language))
                {
                    node.Set(Properties.Language, language);
                }
                if(IsDefined(doc.LineCount, out var lineCount))
                {
                    node.Set(Properties.LineCount, lineCount);
                }
            }
            return new AnalysisResult(node, label);
        }
    }
}
