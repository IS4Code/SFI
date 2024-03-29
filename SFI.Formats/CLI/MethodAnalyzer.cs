﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET methods and constructors, as instances of <see cref="MethodBase"/>.
    /// </summary>
    [Description("An analyzer of .NET methods and constructors.")]
    public class MethodAnalyzer : MemberAnalyzer<MethodBase>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public MethodAnalyzer() : base(Classes.CodeExecutable)
        {

        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeDefinition(MethodBase member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            if(member.IsConstructor || member.Equals(member.DeclaringType.TypeInitializer))
            {
                node.SetClass(Classes.CodeConstructor);
            }else{
                node.SetClass(Classes.CodeMethod);
            }

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
                isAbstract: member.IsAbstract,
                isFinal: member.IsFinal || !member.IsVirtual,
                isStatic: member.IsStatic
            );

            if((member.MethodImplementationFlags & MethodImplAttributes.Synchronized) != 0)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeSynchronizedModifier);
            }

            if(member is MethodInfo method)
            {
                await ReferenceMember(node, Properties.CodeReturnType, method.ReturnType, context, analyzers);

                if(member.IsVirtual && (member.Attributes & MethodAttributes.NewSlot) == 0)
                {
                    MethodInfo? baseMethod;
                    try{
                        baseMethod = method.GetBaseDefinition();
                    }catch(NotSupportedException)
                    {
                        // MetadataLoadContext does not support it; look it up by signature
                        baseMethod = FindBaseMethod(method);
                    }
                    if(baseMethod != null && !member.Equals(baseMethod))
                    {
                        await ReferenceMember(node, Properties.CodeOverrides, baseMethod, context, analyzers);
                    }
                }
                await AnalyzeCustomAttributes(node, context, analyzers, method, method.GetCustomAttributesData().Concat(method.ReturnParameter.GetCustomAttributesData()), method.ReturnParameter.GetOptionalCustomModifiers(), method.ReturnParameter.GetRequiredCustomModifiers());
            }else{
                await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData());
            }

            if(member.IsGenericMethodDefinition)
            {
                var genParamContext = context.WithParentLink(node, Properties.CodeTypeParameter);
                foreach(var genParam in member.GetGenericArguments())
                {
                    await analyzers.Analyze(genParam, genParamContext);
                }
            }

            var paramContext = context.WithParentLink(node, Properties.CodeParameter);
            foreach(var methodParam in member.GetParameters())
            {
                await analyzers.Analyze(methodParam, paramContext);
            }

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(MethodBase member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            if(member.IsConstructor || member.Equals(member.DeclaringType.TypeInitializer))
            {
                node.SetClass(Classes.CodeConstructor);
            }else{
                node.SetClass(Classes.CodeMethod);
            }

            node.Set(Properties.Identifier, TextTools.FormatMemberId(member));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(member, MemberIdFormatOptions.None));

            if(member.IsGenericMethod && !member.IsGenericMethodDefinition && member is MethodInfo methodInfo)
            {
                await ReferenceMember(node, Properties.CodeOverrides, methodInfo.GetGenericMethodDefinition(), context, analyzers);
                foreach(var typeArg in member.GetGenericArguments())
                {
                    node.Set(Properties.CodeTypeArgument, ClrNamespaceUriFormatter.Instance, typeArg);
                }
            }else{
                node.Set(Properties.CodeName, name);

                foreach(var methodParam in member.GetParameters())
                {
                    await ReferenceMember(node, Properties.CodeParameter, methodParam, context, analyzers);
                }

                await ReferenceMember(node, Properties.CodeMethodOf, member.DeclaringType, context, analyzers);
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
