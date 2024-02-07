using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET namespaces, as instances of <see cref="Namespace"/>.
    /// </summary>
    [Description("An analyzer of .NET namespaces.")]
    public class NamespaceAnalyzer : CodeElementAnalyzer<Namespace>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public NamespaceAnalyzer() : base(Classes.CodePackage)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Namespace ns, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = ns.Name;
            var node = GetNode(Uri.EscapeDataString(name), context);

            if(!String.IsNullOrEmpty(name))
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            var fullName = ns.FullName;
            if(!String.IsNullOrEmpty(fullName))
            {
                node.Set(Properties.PrefLabel, fullName);
                node.Set(Properties.CodeCanonicalName, fullName);
            }
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, ns);

            await AnalyzeCustomAttributes(node, context, analyzers, ns.GetCustomAttributesData());

            var declaresContext = context.WithParentLink(node, Properties.CodeDeclares);
            foreach(var ns2 in ns.Namespaces)
            {
                await analyzers.Analyze(ns2, declaresContext);
            }

            var typeContext = context.WithParentLink(node, Properties.CodeDeclaresType);
            foreach(var type in ExportedOnly ? ns.ExportedTypes : ns.DefinedTypes)
            {
                await analyzers.Analyze(type, typeContext);
            }

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(Namespace ns, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = ns.Name;

            if(!String.IsNullOrEmpty(name))
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            if(!String.IsNullOrEmpty(ns.FullName))
            {
                node.Set(Properties.CodeCanonicalName, ns.FullName);
            }

            if(!ns.IsGlobal)
            {
                await ReferenceMember(node, Properties.CodeDeclaredBy, ns.DeclaringNamespace, context, analyzers);
            }

            return new(node, name);
        }
    }
}
