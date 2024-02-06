using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
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
            var node = GetNode(name == null ? param.Position.ToString() : Uri.EscapeDataString(name), context);

            if(name != null)
            {
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, name);
            }
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, param);

            node.Set(Properties.CodePosition, param.Position);

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, param.ParameterType);

            AnalyzeCustomAttributes(node, param.GetCustomAttributesData());

            return new(node, name);
        }
    }
}
