using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using NPOI;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzes OOXML-based documents, as instances of <see cref="POIXMLDocument"/>.
    /// </summary>
    public class OpenXmlDocumentAnalyzer : MediaObjectAnalyzer<POIXMLDocument>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public OpenXmlDocumentAnalyzer() : base(Common.DocumentClasses)
        {

        }

        public async override ValueTask<AnalysisResult> Analyze(POIXMLDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var properties = document.GetProperties();

            var core = properties.CoreProperties;
            if(core != null)
            {
                if(core.Creator is string creator)
                {
                    node.Set(Properties.Creator, creator);
                }
                if(core.Subject is string subject)
                {
                    node.Set(Properties.Subject, subject);
                }
                if(core.Modified is DateTime modified)
                {
                    node.Set(Properties.Modified, modified);
                }
                if(core.Keywords is string keywords)
                {
                    node.Set(Properties.Keywords, keywords);
                }
                if(core.Identifier is string identifier)
                {
                    node.Set(Properties.Identifier, identifier);
                }
                if(core.Description is string description)
                {
                    node.Set(Properties.Description, description);
                }
                if(core.Revision is string revision)
                {
                    node.Set(Properties.Version, revision);
                }
                if(core.Created is DateTime created)
                {
                    node.Set(Properties.Created, created);
                }
                if(core.Category is string category)
                {
                    node.Set(Properties.Category, category);
                }
                if(core.Title is string title)
                {
                    node.Set(Properties.Title, title);
                }
            }
            var ext = properties.ExtendedProperties;
            if(ext != null)
            {
                if(ext.CharactersWithSpaces > 0)
                {
                    node.Set(Properties.CharacterCount, ext.CharactersWithSpaces);
                }else if(ext.Characters > 0)
                {
                    node.Set(Properties.CharacterCount, ext.Characters);
                }
                if(ext.Words > 0)
                {
                    node.Set(Properties.WordCount, ext.Words);
                }
                if(ext.Lines > 0)
                {
                    node.Set(Properties.LineCount, ext.Lines);
                }
                if(ext.Pages > 0)
                {
                    node.Set(Properties.PageCount, ext.Pages);
                }
            }
            return new AnalysisResult(node);
        }
    }
}
