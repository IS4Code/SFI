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
        /// <param name="member">The parameter to analyze.</param>
        public async override ValueTask<AnalysisResult> Analyze(ParameterInfo member, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;
            var node = GetNode(member.Position.ToString(), context);

            node.Set(Properties.PrefLabel, member.ToString());
            if(!String.IsNullOrEmpty(name))
            {
                node.Set(Properties.CodeName, name);
            }
            await ReferenceMember(node, Properties.Broader, member, context, analyzers);
            node.Set(Properties.Identifier, member.MetadataToken);

            node.Set(Properties.CodePosition, member.Position);

            if(member.HasDefaultValue)
            {
                node.TrySet(Properties.DefaultValue, member.RawDefaultValue ?? DBNull.Value);
            }
            
            await ReferenceMember(node, Properties.CodeType, member.ParameterType, context, analyzers);

            await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData(), member.GetOptionalCustomModifiers(), member.GetRequiredCustomModifiers());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(ParameterInfo member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            if(!String.IsNullOrEmpty(name))
            {
                node.Set(Properties.CodeName, name);
            }

            node.Set(Properties.Identifier, TextTools.FormatMemberId(member.Member) + "/" + member.Position);
            node.Set(Properties.PrefLabel, member.Position.ToString());

            node.Set(Properties.CodePosition, member.Position);

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, member.ParameterType);

            return new(node, name);
        }
    }
}
