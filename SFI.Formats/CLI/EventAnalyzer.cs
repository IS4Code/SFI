using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET events, as instances of <see cref="EventInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET events.")]
    public class EventAnalyzer : MemberAnalyzer<EventInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public EventAnalyzer() : base(Classes.CodeVariable)
        {

        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeDefinition(EventInfo member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            node.Set(Properties.PrefLabel, member.ToString());
            node.Set(Properties.CodeName, name);
            await ReferenceMember(node, Properties.Broader, member, context, analyzers);
            node.Set(Properties.Identifier, member.MetadataToken);

            await ReferenceMember(node, Properties.CodeType, member.EventHandlerType, context, analyzers);

            if(member.AddMethod is { } addMethod && (!ExportedOnly || IsPublic(addMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, addMethod, context, analyzers);
            }
            if(member.RemoveMethod is { } removeMethod && (!ExportedOnly || IsPublic(removeMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, removeMethod, context, analyzers);
            }
            if(member.RaiseMethod is { } raiseMethod && (!ExportedOnly || IsPublic(raiseMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, raiseMethod, context, analyzers);
            }

            foreach(var method in member.GetOtherMethods(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await ReferenceMember(node, Properties.CodeReferences, method, context, analyzers);
                }
            }

            await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(EventInfo member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
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
