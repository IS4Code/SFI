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

            if(!evnt.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, evnt);

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, evnt.EventHandlerType);

            if(evnt.AddMethod is { } addMethod && (!ExportedOnly || IsPublic(addMethod)))
            {
                node.Set(Properties.CodeReferences, ClrNamespaceUriFormatter.Instance, addMethod);
            }
            if(evnt.RemoveMethod is { } removeMethod && (!ExportedOnly || IsPublic(removeMethod)))
            {
                node.Set(Properties.CodeReferences, ClrNamespaceUriFormatter.Instance, removeMethod);
            }
            if(evnt.RaiseMethod is { } raiseMethod && (!ExportedOnly || IsPublic(raiseMethod)))
            {
                node.Set(Properties.CodeReferences, ClrNamespaceUriFormatter.Instance, raiseMethod);
            }

            foreach(var method in evnt.GetOtherMethods(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    node.Set(Properties.CodeReferences, ClrNamespaceUriFormatter.Instance, method);
                }
            }

            AnalyzeCustomAttributes(node, evnt.GetCustomAttributesData());

            return new(node, name);
        }
    }
}
