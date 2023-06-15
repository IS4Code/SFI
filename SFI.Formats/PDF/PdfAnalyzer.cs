using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using PdfSharpCore.Pdf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes PDF documents, as instances of <see cref="PdfDocument"/>.
    /// </summary>
    public class PdfAnalyzer : MediaObjectAnalyzer<PdfDocument>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public PdfAnalyzer() : base(Common.DocumentClasses)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(PdfDocument document, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            string? label = null;
            var info = document.Info;
            if(info != null)
            {
                if(IsDefined(info.Author, out var author))
                {
                    node.Set(Properties.Creator, author);
                }
                if(IsDefined(info.Subject, out var subject))
                {
                    node.Set(Properties.Subject, subject);
                }
                if(IsDefined(info.ModificationDate, out var modified))
                {
                    node.Set(Properties.Modified, modified);
                }
                if(IsDefined(info.Keywords, out var keywords))
                {
                    node.Set(Properties.Keywords, keywords);
                }
                if(IsDefined(info.CreationDate, out var created))
                {
                    node.Set(Properties.Created, created);
                }
                if(IsDefined(info.Title, out var title))
                {
                    node.Set(Properties.Title, title);
                    label = title;
                }
            }
            if(document.Internals?.Catalog.Elements.GetDictionary("/Metadata") is PdfDictionary metadata)
            {
                var data = metadata.Stream?.Value;
                if(data != null)
                {
                    using var stream = new MemoryStream(data, false);
                    DataTools.DescribeAsXmp(node, stream);
                }
            }
            return new AnalysisResult(node, label);
        }
    }
}
