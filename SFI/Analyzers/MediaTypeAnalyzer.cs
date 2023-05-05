using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of media types, as instances of <see cref="ContentType"/>.
    /// </summary>
    public sealed class MediaTypeAnalyzer : EntityAnalyzer<ContentType>
    {
        /// <summary>
        /// Whether to add class information to the created node.
        /// </summary>
        public bool AddClasses { get; set; } = true;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public MediaTypeAnalyzer()
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(ContentType entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var type = entity.ToString();
            var node = context.NodeFactory.Create(Vocabularies.Urim, UriTools.EscapePathString(type));
            if(AddClasses)
            {
                node.SetClass(Classes.MediaType);
                if(type.StartsWith(TextTools.ImpliedMediaTypePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    node.SetClass(Classes.MediaTypeImplied);
                }
                int semicolon = type.IndexOf(';');
                if(semicolon != -1)
                {
                    node.SetClass(Classes.MediaTypeParametrized);
                }
                int plus = type.IndexOf('+');
                if(plus != -1 && (semicolon == -1 || plus < semicolon))
                {
                    node.SetClass(Classes.MediaTypeStructured);
                }
            }
            return new(node, type);
        }
    }
}
