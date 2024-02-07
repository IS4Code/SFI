using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET methods, as instances of <see cref="MethodBase"/>.
    /// </summary>
    [Description("An analyzer of .NET methods.")]
    public class MethodAnalyzer : MemberAnalyzer<MethodBase>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public MethodAnalyzer() : base(Classes.CodeMethod)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(MethodBase method, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = method.Name;
            var node = GetNode(method, context);

            node.Set(Properties.PrefLabel, method.ToString());
            node.Set(Properties.CodeName, name);
            await ReferenceMember(node, Properties.Broader, method, context, analyzers);
            node.Set(Properties.Identifier, method.MetadataToken);

            SetModifiers(
                node,
                isPublic: method.IsPublic,
                isPrivate: method.IsPrivate,
                isFamily: method.IsFamily,
                isFamilyAndAssembly: method.IsFamilyAndAssembly,
                isFamilyOrAssembly: method.IsFamilyOrAssembly,
                isAssembly: method.IsAssembly,
                isAbstract: method.IsAbstract,
                isFinal: method.IsFinal || !method.IsVirtual,
                isStatic: method.IsStatic
            );

            if(method is MethodInfo methodInfo)
            {
                await ReferenceMember(node, Properties.CodeReturnType, methodInfo.ReturnType, context, analyzers);

                if(method.IsVirtual && (method.Attributes & MethodAttributes.NewSlot) == 0)
                {
                    MethodInfo? baseMethod = null;
                    try{
                        baseMethod = methodInfo.GetBaseDefinition();
                    }catch(NotSupportedException)
                    {
                        // MetadataLoadContext does not support it; look it up by signature
                        baseMethod = FindBaseMethod(methodInfo);
                    }
                    if(baseMethod != null && !method.Equals(baseMethod))
                    {
                        await ReferenceMember(node, Properties.CodeOverrides, baseMethod, context, analyzers);
                    }
                }
                await AnalyzeCustomAttributes(node, context, analyzers, methodInfo.GetCustomAttributesData(), methodInfo.ReturnParameter.GetOptionalCustomModifiers(), methodInfo.ReturnParameter.GetRequiredCustomModifiers());
            }else{
                await AnalyzeCustomAttributes(node, context, analyzers, method.GetCustomAttributesData());
            }

            if(method.IsGenericMethodDefinition)
            {
                var genParamContext = context.WithParentLink(node, Properties.CodeTypeParameter);
                foreach(var genParam in method.GetGenericArguments())
                {
                    await analyzers.Analyze(genParam, genParamContext);
                }
            }

            var paramContext = context.WithParentLink(node, Properties.CodeParameter);
            foreach(var methodParam in method.GetParameters())
            {
                await analyzers.Analyze(methodParam, paramContext);
            }

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(MethodBase method, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = method.Name;

            node.Set(Properties.Identifier, TextTools.FormatMemberId(method));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(method, includeNamespace: false, includeDeclaringMember: false));

            if(method.IsGenericMethod && !method.IsGenericMethodDefinition && method is MethodInfo methodInfo)
            {
                await ReferenceMember(node, Properties.CodeOverrides, methodInfo.GetGenericMethodDefinition(), context, analyzers);
                foreach(var typeArg in method.GetGenericArguments())
                {
                    node.Set(Properties.CodeTypeArgument, ClrNamespaceUriFormatter.Instance, typeArg);
                }
            }else{
                node.Set(Properties.CodeName, name);

                foreach(var methodParam in method.GetParameters())
                {
                    await ReferenceMember(node, Properties.CodeParameter, methodParam, context, analyzers);
                }

                await ReferenceMember(node, Properties.CodeMethodOf, method.DeclaringType, context, analyzers);
            }

            return new(node, name);
        }

        static MethodInfo? FindBaseMethod(MethodInfo method)
        {
            Type baseType = method.DeclaringType;
            var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            while((baseType = baseType.BaseType) != null)
            {
                var baseMethod = baseType.GetMethod(method.Name, BindingFlags, null, paramTypes, null);
                if(baseMethod != null)
                {
                    return baseMethod;
                }
            }
            return null;
        }
    }
}
