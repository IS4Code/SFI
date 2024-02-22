using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET types, as instances of <see cref="Type"/>.
    /// </summary>
    [Description("An analyzer of .NET types.")]
    public class TypeAnalyzer : MemberAnalyzer<Type>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public TypeAnalyzer() : base(Classes.CodeType)
        {

        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeDefinition(Type member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            if(member.IsGenericParameter)
            {
                node.SetClass(Classes.CodeTypeVariable);
            }else if(member.IsPrimitive)
            {
                node.SetClass(Classes.CodePrimitiveType);
            }else if(member.IsConstructedGenericType)
            {
                node.SetClass(Classes.CodeGenericParameterizedType);
            }else if(!member.HasElementType)
            {
                node.SetClass(Classes.CodeComplexType);
            }
            if(member.IsClass)
            {
                node.SetClass(Classes.CodeClass);
            }
            if(member.IsInterface)
            {
                node.SetClass(Classes.CodeInterface);
            }
            if(member.IsEnum)
            {
                node.SetClass(Classes.CodeEnum);
            }
            if(member.IsArray)
            {
                node.SetClass(Classes.CodeArrayType);
            }
            if(IsAttribute(member))
            {
                node.SetClass(Classes.CodeAnnotationType);
            }

            node.Set(Properties.PrefLabel, member.ToString());
            if(name != null)
            {
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, member.FullName ?? name);
            }
            await ReferenceMember(node, Properties.Broader, member, context, analyzers);
            node.Set(Properties.Identifier, member.MetadataToken);

            if(member.IsGenericParameter)
            {
                node.Set(Properties.CodePosition, member.GenericParameterPosition);
            }else{
                SetModifiers(
                    node,
                    isPublic: member.IsPublic || member.IsNestedPublic,
                    isPrivate: member.IsNestedPrivate,
                    isFamily: member.IsNestedFamily,
                    isFamilyAndAssembly: member.IsNestedFamANDAssem,
                    isFamilyOrAssembly: member.IsNestedFamORAssem,
                    isAssembly: member.IsNotPublic || member.IsNestedAssembly,
                    isAbstract: member.IsAbstract && !member.IsSealed,
                    isFinal: member.IsSealed && !member.IsAbstract,
                    isStatic: member.IsAbstract && member.IsSealed
                );
            }

            if(member.BaseType is { } baseType)
            {
                await ReferenceMember(node, Properties.CodeExtends, baseType, context, analyzers);
            }
            foreach(var implemented in member.GetInterfaces())
            {
                await ReferenceMember(node, Properties.CodeImplements, implemented, context, analyzers);
            }

            await AnalyzeCustomAttributes(node, context, analyzers, member, member.GetCustomAttributesData());

            if(member.IsGenericTypeDefinition)
            {
                var genParamContext = context.WithParentLink(node, Properties.CodeTypeParameter);
                foreach(var genParam in member.GetGenericArguments())
                {
                    await analyzers.Analyze(genParam, genParamContext);
                }
            }

            var fieldContext = context.WithParentLink(node, Properties.CodeField);
            foreach(var field in member.GetFields(BindingFlags))
            {
                if(!ExportedOnly || field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly)
                {
                    await analyzers.Analyze(field, fieldContext);
                }
            }

            var constructorContext = context.WithParentLink(node, Properties.CodeConstructor);
            foreach(var ctor in member.GetConstructors(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(ctor))
                {
                    await analyzers.Analyze(ctor, constructorContext);
                }
            }

            var methodContext = context.WithParentLink(node, Properties.CodeMethod);
            foreach(var method in member.GetMethods(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await analyzers.Analyze(method, methodContext);
                }
            }

            var declaresContext = context.WithParentLink(node, Properties.CodeDeclares);
            foreach(var nested in member.GetNestedTypes(BindingFlags))
            {
                if(!ExportedOnly || nested.IsNestedPublic || nested.IsNestedFamily || nested.IsNestedFamORAssem)
                {
                    await analyzers.Analyze(nested, declaresContext);
                }
            }
            
            foreach(var property in member.GetProperties(BindingFlags))
            {
                if(!ExportedOnly || property.GetAccessors(true).Any(IsPublic))
                {
                    await analyzers.Analyze(property, declaresContext);
                }
            }

            foreach(var evnt in member.GetEvents(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(evnt.AddMethod) || IsPublic(evnt.RemoveMethod) || IsPublic(evnt.RaiseMethod) || evnt.GetOtherMethods(true).Any(IsPublic))
                {
                    await analyzers.Analyze(evnt, declaresContext);
                }
            }

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(Type member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.Name;

            node.Set(Properties.Identifier, TextTools.FormatMemberId(member));
            node.Set(Properties.PrefLabel, TextTools.FormatMemberId(member, MemberIdFormatOptions.None));

            if(member.IsConstructedGenericType)
            {
                node.SetClass(Classes.CodeGenericParameterizedType);
                await ReferenceMember(node, Properties.CodeGenericTypeDefinition, member.GetGenericTypeDefinition(), context, analyzers);
                foreach(var typeArg in member.GetGenericArguments())
                {
                    node.Set(Properties.CodeTypeArgument, ClrNamespaceUriFormatter.Instance, typeArg);
                }
            }else if(member.IsArray)
            {
                node.SetClass(Classes.CodeArrayType);
                node.Set(Properties.CodeArrayDimensions, member.GetArrayRank());
                await ReferenceMember(node, Properties.CodeArrayElementType, member.GetElementType(), context, analyzers);
            }else if(member.HasElementType)
            {
                // Some other form of constructed type not expressible by the vocabulary
                await ReferenceMember(node, Properties.CodeReferences, member.GetElementType(), context, analyzers);
            }else if(member.IsGenericParameter)
            {
                node.SetClass(Classes.CodeTypeVariable);
                node.Set(Properties.CodePosition, member.GenericParameterPosition);

                if(member.DeclaringMethod is { } declaringMethod)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringMethod, context, analyzers);
                }else if(member.DeclaringType is { } declaringType)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringType, context, analyzers);
                }
            }else{
                if(!member.IsPrimitive)
                {
                    node.SetClass(Classes.CodeComplexType);
                }
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, member.FullName ?? name);

                if(member.DeclaringType is { } declaringType)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringType, context, analyzers);
                }else if(Namespace.FromAssembly(member.Assembly, member.Namespace) is { } ns)
                {
                    await ReferenceMember(node, Properties.CodeTypeDeclaredBy, ns, context, analyzers);
                }
            }

            return new(node, name);
        }

        static bool IsAttribute(Type type)
        {
            try{
                while(type != null)
                {
                    if(!type.IsClass || (type.IsSealed && type.IsAbstract))
                    {
                        return false;
                    }
                    if(type.FullName == AttributeConstants.AttributeType)
                    {
                        return true;
                    }
                    if(type.GetCustomAttributesData().Any(a => a.AttributeType.FullName == AttributeConstants.AttributeUsageAttributeType))
                    {
                        return true;
                    }
                    type = type.BaseType;
                }
                return false;
            }catch(TypeLoadException)
            {
                return type.Name.EndsWith("Attribute", StringComparison.Ordinal);
            }
        }
    }
}
