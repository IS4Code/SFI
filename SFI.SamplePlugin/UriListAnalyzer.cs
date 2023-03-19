using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer for the text/uri-list format, as a list of <see cref="Uri"/> instances.
    /// </summary>
    public class UriListAnalyzer : EntityAnalyzer<IReadOnlyList<Uri>>
    {
        /// <inheritdoc/>
        public override async ValueTask<AnalysisResult> Analyze(IReadOnlyList<Uri> entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            for(int i = 0; i < entity.Count; i++)
            {
                node.Set(Properties.MemberAt, i + 1, entity[i]);
            }

            return new(node);
        }
    }
}
