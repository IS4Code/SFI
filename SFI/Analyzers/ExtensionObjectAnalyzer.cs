using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of file extensions, expressed as instances of <see cref="ExtensionObject"/>.
    /// </summary>
    [Description("An analyzer of file extensions.")]
    public class ExtensionObjectAnalyzer : EntityAnalyzer<ExtensionObject>
    {
        /// <summary>
        /// Whether to add class information to the created node.
        /// </summary>
        [Description("Whether to add class information to the created node.")]
        public bool AddClasses { get; set; } = true;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ExtensionObjectAnalyzer()
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(ExtensionObject extension, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            if(context.Node is not ILinkedNode node)
            {
                var ext = (extension.Value.Value ?? "").ToLowerInvariant();
                node = context.NodeFactory.Create(Vocabularies.Uris, Uri.EscapeDataString(ext));
            }
            node = InitNewNode(node, context);
            if(AddClasses)
            {
                node.SetClass(Classes.Extension);
            }
            return new(node);
        }
    }
}
