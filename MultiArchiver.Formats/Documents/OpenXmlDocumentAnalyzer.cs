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
                if(IsDefined(core.Creator, out var creator))
                {
                    node.Set(Properties.Creator, creator);
                }
                if(IsDefined(core.Subject, out var subject))
                {
                    node.Set(Properties.Subject, subject);
                }
                if(IsDefined(core.Modified, out var modified))
                {
                    node.Set(Properties.Modified, modified);
                }
                if(IsDefined(core.Keywords, out var keywords))
                {
                    node.Set(Properties.Keywords, keywords);
                }
                if(IsDefined(core.Identifier, out var identifier))
                {
                    node.Set(Properties.Identifier, identifier);
                }
                if(IsDefined(core.Description, out var description))
                {
                    node.Set(Properties.Description, description);
                }
                if(IsDefined(core.Revision, out var revision))
                {
                    node.Set(Properties.Version, revision);
                }
                if(IsDefined(core.Created, out var created))
                {
                    node.Set(Properties.Created, created);
                }
                if(IsDefined(core.Category, out var category))
                {
                    node.Set(Properties.Category, category);
                }
                if(IsDefined(core.Title, out var title))
                {
                    node.Set(Properties.Title, title);
                }
            }
            var ext = properties.ExtendedProperties;
            if(ext != null)
            {
                if(
                    IsDefined(ext.CharactersWithSpaces, out var chars) ||
                    IsDefined(ext.Characters, out chars))
                {
                    node.Set(Properties.CharacterCount, chars);
                }
                if(IsDefined(ext.Words, out var words))
                {
                    node.Set(Properties.WordCount, words);
                }
                if(IsDefined(ext.Lines, out var lines))
                {
                    node.Set(Properties.LineCount, lines);
                }
                if(IsDefined(ext.Pages, out var pages))
                {
                    node.Set(Properties.PageCount, pages);
                }
            }
            return new AnalysisResult(node);
        }

        static bool IsDefined(string value, out string result)
        {
            result = value;
            return String.IsNullOrEmpty(value);
        }

        static bool IsDefined(int value, out int result)
        {
            result = value;
            return value > 0;
        }

        static bool IsDefined<T>(T? value, out T result) where T : struct
        {
            result = value.GetValueOrDefault();
            return value != null;
        }
    }
}
