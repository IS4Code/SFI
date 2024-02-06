using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;

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

        /// <inheritdoc cref="EntityAnalyzer.GetNode(string, AnalysisContext)"/>
        /// <param name="member">The member to provide the identifier of the node.</param>
        protected ILinkedNode GetNode(MemberInfo member, AnalysisContext context)
        {
            var id = TextTools.FormatMemberId(member, true, false, false);
            return GetNode(id, context);
        }

        internal bool IsPublic(MethodBase? method)
        {
            return method != null && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }
    }
}
