using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NPOI;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
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

        public async override ValueTask<AnalysisResult> Analyze(POIDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var sum = document.SummaryInformation;
            if(sum != null)
            {
                if(sum.Author is string creator)
                {
                    node.Set(Properties.Creator, creator);
                }
                if(sum.Subject is string subject)
                {
                    node.Set(Properties.Subject, subject);
                }
                if(sum.LastSaveDateTime is DateTime modified)
                {
                    node.Set(Properties.Modified, modified);
                }
                if(sum.Keywords is string keywords)
                {
                    node.Set(Properties.Keywords, keywords);
                }
                if(sum.RevNumber is string revision)
                {
                    node.Set(Properties.Version, revision);
                }
                if(sum.CreateDateTime is DateTime created)
                {
                    node.Set(Properties.Created, created);
                }
                if(sum.Title is string title)
                {
                    node.Set(Properties.Title, title);
                }
                if(sum.CharCount > 0)
                {
                    node.Set(Properties.CharacterCount, sum.CharCount);
                }
                if(sum.WordCount > 0)
                {
                    node.Set(Properties.WordCount, sum.WordCount);
                }
                if(sum.PageCount > 0)
                {
                    node.Set(Properties.PageCount, sum.PageCount);
                }
            }
            var doc = document.DocumentSummaryInformation;
            if(doc != null)
            {
                if(doc.Category is string category)
                {
                    node.Set(Properties.Category, category);
                }
                if(doc.Language is string language)
                {
                    node.Set(Properties.Language, language);
                }
                if(doc.LineCount > 0)
                {
                    node.Set(Properties.LineCount, doc.LineCount);
                }
            }
            return new AnalysisResult(node);
        }
    }
}
