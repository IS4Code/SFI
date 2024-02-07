using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET parameters, as instances of <see cref="ParameterInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET parameters.")]
    public class ParameterAnalyzer : CodeElementAnalyzer<ParameterInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public ParameterAnalyzer() : base(Classes.CodeParameter)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(ParameterInfo param, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = param.Name;
            var node = GetNode(param.Position.ToString(), context);

            node.Set(Properties.PrefLabel, param.ToString());
            if(name != null)
            {
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, name);
            }
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, param);
            node.Set(Properties.Identifier, param.MetadataToken);

            node.Set(Properties.CodePosition, param.Position);

            await ReferenceMember(node, Properties.CodeType, param.ParameterType, context, analyzers);

            await AnalyzeCustomAttributes(node, context, analyzers, param.GetCustomAttributesData(), param.GetOptionalCustomModifiers(), param.GetRequiredCustomModifiers());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(ParameterInfo param, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = param.Name;

            if(name != null)
            {
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, name);
            }

            node.Set(Properties.CodePosition, param.Position);

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, param.ParameterType);

            return new(node, name);
        }
    }
}
