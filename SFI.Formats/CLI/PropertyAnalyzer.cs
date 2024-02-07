using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET properties, as instances of <see cref="PropertyInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET properties.")]
    public class PropertyAnalyzer : MemberAnalyzer<PropertyInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public PropertyAnalyzer() : base(Classes.CodeVariable)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(PropertyInfo member, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;
            var node = GetNode(member, context);

            node.Set(Properties.PrefLabel, member.ToString());
            if(!member.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);
            await ReferenceMember(node, Properties.Broader, member, context, analyzers);
            node.Set(Properties.Identifier, member.MetadataToken);

            await ReferenceMember(node, Properties.CodeType, member.PropertyType, context, analyzers);

            if(member.CanRead && !member.CanWrite)
            {
                node.Set(Properties.ReadonlyValue, true);
            }

            if(member.GetMethod is {  } getMethod && (!ExportedOnly || IsPublic(getMethod)))
            {
                await ReferenceMember(node, Properties.CodeReturnedBy, getMethod, context, analyzers);
            }

            foreach(var method in member.GetAccessors(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await ReferenceMember(node, Properties.CodeReferences, method, context, analyzers);
                }
            }

            await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData(), member.GetOptionalCustomModifiers(), member.GetRequiredCustomModifiers());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(PropertyInfo member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            node.Set(Properties.CodeName, name);

            node.Set(Properties.Identifier, TextTools.FormatMemberId(member));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(member, MemberIdFormatOptions.None));

            await ReferenceMember(node, Properties.CodeDeclaredBy, member.DeclaringType, context, analyzers);

            return new(node, name);
        }
    }
}
