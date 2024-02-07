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
        public async override ValueTask<AnalysisResult> Analyze(EventInfo evnt, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = evnt.Name;
            var node = GetNode(evnt, context);

            node.Set(Properties.PrefLabel, evnt.ToString());
            node.Set(Properties.CodeName, name);
            await ReferenceMember(node, Properties.Broader, evnt, context, analyzers);
            node.Set(Properties.Identifier, evnt.MetadataToken);

            await ReferenceMember(node, Properties.CodeType, evnt.EventHandlerType, context, analyzers);

            if(evnt.AddMethod is { } addMethod && (!ExportedOnly || IsPublic(addMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, addMethod, context, analyzers);
            }
            if(evnt.RemoveMethod is { } removeMethod && (!ExportedOnly || IsPublic(removeMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, removeMethod, context, analyzers);
            }
            if(evnt.RaiseMethod is { } raiseMethod && (!ExportedOnly || IsPublic(raiseMethod)))
            {
                await ReferenceMember(node, Properties.CodeReferences, raiseMethod, context, analyzers);
            }

            foreach(var method in evnt.GetOtherMethods(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await ReferenceMember(node, Properties.CodeReferences, method, context, analyzers);
                }
            }

            await AnalyzeCustomAttributes(node, context, analyzers, evnt.GetCustomAttributesData());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(EventInfo evnt, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = evnt.Name;

            node.Set(Properties.CodeName, name);

            node.Set(Properties.Identifier, TextTools.FormatMemberId(evnt));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(evnt, MemberIdFormatOptions.None));

            await ReferenceMember(node, Properties.CodeDeclaredBy, evnt.DeclaringType, context, analyzers);

            return new(node, name);
        }
    }
}
