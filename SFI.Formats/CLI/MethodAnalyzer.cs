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
    /// An analyzer of .NET fields, as instances of <see cref="MethodInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET fields.")]
    public class MethodAnalyzer : MemberAnalyzer<MethodInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public MethodAnalyzer() : base(Classes.CodeMethod)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(MethodInfo method, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = method.Name;
            var node = GetNode(method, context);

            if(!method.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, method);

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

            node.Set(Properties.CodeReturnType, ClrNamespaceUriFormatter.Instance, method.ReturnType);

            if(method.IsVirtual && (method.Attributes & MethodAttributes.NewSlot) == 0)
            {
                MethodInfo? baseMethod = null;
                try{
                    baseMethod = method.GetBaseDefinition();
                }catch(NotSupportedException)
                {
                    // MetadataLoadContext does not support it; look it up by signature
                    baseMethod = FindBaseMethod(method);
                }
                if(baseMethod != null && !method.Equals(baseMethod))
                {
                    node.Set(Properties.CodeOverrides, ClrNamespaceUriFormatter.Instance, baseMethod);
                }
            }

            AnalyzeCustomAttributes(node, method.GetCustomAttributesData());

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
