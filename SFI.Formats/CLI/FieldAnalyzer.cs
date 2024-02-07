using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET fields, as instances of <see cref="FieldInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET fields.")]
    public class FieldAnalyzer : MemberAnalyzer<FieldInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public FieldAnalyzer() : base(Classes.CodeField)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(FieldInfo member, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;
            var node = GetNode(member, context);

            node.Set(Properties.PrefLabel, member.ToString());
            node.Set(Properties.CodeName, name);
            await ReferenceMember(node, Properties.Broader, member, context, analyzers);
            node.Set(Properties.Identifier, member.MetadataToken);

            SetModifiers(
                node,
                isPublic: member.IsPublic,
                isPrivate: member.IsPrivate,
                isFamily: member.IsFamily,
                isFamilyAndAssembly: member.IsFamilyAndAssembly,
                isFamilyOrAssembly: member.IsFamilyOrAssembly,
                isAssembly: member.IsAssembly,
                isFinal: member.IsInitOnly,
                isStatic: member.IsStatic
            );

            if(member.IsInitOnly || member.IsLiteral)
            {
                node.Set(Properties.ReadonlyValue, true);
            }

            if(member.IsLiteral)
            {
                node.TrySet(Properties.Value, member.GetRawConstantValue() ?? DBNull.Value);
            }

            await ReferenceMember(node, Properties.CodeType, member.FieldType, context, analyzers);

            await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData(), member.GetOptionalCustomModifiers(), member.GetRequiredCustomModifiers());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(FieldInfo member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            node.Set(Properties.CodeName, name);

            node.Set(Properties.Identifier, TextTools.FormatMemberId(member));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(member, MemberIdFormatOptions.None));

            await ReferenceMember(node, Properties.CodeFieldOf, member.DeclaringType, context, analyzers);

            return new(node, name);
        }
    }
}
