using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET members, as instances of <see cref="MemberInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET members.")]
    public abstract class MemberAnalyzer<T> : CodeElementAnalyzer<T> where T : MemberInfo
    {
        /// <inheritdoc cref="CodeElementAnalyzer{T}.CodeElementAnalyzer"/>
        public MemberAnalyzer()
        {

        }

        /// <inheritdoc cref="CodeElementAnalyzer{T}.CodeElementAnalyzer(ClassUri)"/>
        public MemberAnalyzer(ClassUri elementClass) : base(elementClass)
        {

        }

        /// <inheritdoc cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)"/>
        /// <param name="member">The member to analyze.</param>
        public abstract override ValueTask<AnalysisResult> Analyze(T member, AnalysisContext context, IEntityAnalyzers analyzers);

        /// <inheritdoc cref="EntityAnalyzer.GetNode(string, AnalysisContext)"/>
        /// <param name="member">The member to provide the identifier of the node.</param>
        protected ILinkedNode GetNode(MemberInfo member, AnalysisContext context)
        {
            var id = TextTools.FormatMemberId(member, MemberIdFormatOptions.UriEscaping);
            var node = GetNode(id, context);
            if(!context.Initialized && member.Module == null)
            {
                // Self-contained
                node.SetClass(Classes.MediaObject);
            }
            return node;
        }

        internal bool IsPublic(MethodBase? method)
        {
            return method != null && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }
    }
}
