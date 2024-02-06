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
        public async override ValueTask<AnalysisResult> Analyze(Type type, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = type.Name;
            var node = GetNode(type, context);

            if(type.IsGenericParameter)
            {
                node.SetClass(Classes.CodeTypeVariable);
            }else if(type.IsPrimitive)
            {
                node.SetClass(Classes.CodePrimitiveType);
            }else if(type.IsConstructedGenericType)
            {
                node.SetClass(Classes.CodeGenericParameterizedType);
            }else if(!type.HasElementType)
            {
                node.SetClass(Classes.CodeComplexType);
            }
            if(type.IsClass)
            {
                node.SetClass(Classes.CodeClass);
            }
            if(type.IsInterface)
            {
                node.SetClass(Classes.CodeInterface);
            }
            if(type.IsEnum)
            {
                node.SetClass(Classes.CodeEnum);
            }
            if(type.IsArray)
            {
                node.SetClass(Classes.CodeArrayType);
            }

            if(name != null)
            {
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, type.FullName ?? name);
            }
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, type);

            if(type.IsGenericParameter)
            {
                node.Set(Properties.CodePosition, type.GenericParameterPosition);
            }else{
                SetModifiers(
                    node,
                    isPublic: type.IsPublic || type.IsNestedPublic,
                    isPrivate: type.IsNestedPrivate,
                    isFamily: type.IsNestedFamily,
                    isFamilyAndAssembly: type.IsNestedFamANDAssem,
                    isFamilyOrAssembly: type.IsNestedFamORAssem,
                    isAssembly: type.IsNotPublic || type.IsNestedAssembly,
                    isAbstract: type.IsAbstract && !type.IsSealed,
                    isFinal: type.IsSealed && !type.IsAbstract,
                    isStatic: type.IsAbstract && type.IsSealed
                );
            }

            if(type.BaseType is { } baseType)
            {
                await ReferenceMember(node, Properties.CodeExtends, baseType, context, analyzers);
            }
            foreach(var implemented in type.GetInterfaces())
            {
                await ReferenceMember(node, Properties.CodeImplements, implemented, context, analyzers);
            }

            await AnalyzeCustomAttributes(node, context, analyzers, type.GetCustomAttributesData());

            if(type.IsGenericTypeDefinition)
            {
                var genParamContext = context.WithParentLink(node, Properties.CodeTypeParameter);
                foreach(var genParam in type.GetGenericArguments())
                {
                    await analyzers.Analyze(genParam, genParamContext);
                }
            }

            var fieldContext = context.WithParentLink(node, Properties.CodeField);
            foreach(var field in type.GetFields(BindingFlags))
            {
                if(!ExportedOnly || field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly)
                {
                    await analyzers.Analyze(field, fieldContext);
                }
            }

            var constructorContext = context.WithParentLink(node, Properties.CodeConstructor);
            foreach(var ctor in type.GetConstructors(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(ctor))
                {
                    await analyzers.Analyze(ctor, constructorContext);
                }
            }

            var methodContext = context.WithParentLink(node, Properties.CodeMethod);
            foreach(var method in type.GetMethods(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await analyzers.Analyze(method, methodContext);
                }
            }

            var declaresContext = context.WithParentLink(node, Properties.CodeDeclares);
            foreach(var nested in type.GetNestedTypes(BindingFlags))
            {
                if(!ExportedOnly || nested.IsNestedPublic || nested.IsNestedFamily || nested.IsNestedFamORAssem)
                {
                    await analyzers.Analyze(nested, declaresContext);
                }
            }
            
            foreach(var property in type.GetProperties(BindingFlags))
            {
                if(!ExportedOnly || property.GetAccessors(true).Any(IsPublic))
                {
                    await analyzers.Analyze(property, declaresContext);
                }
            }

            foreach(var evnt in type.GetEvents(BindingFlags))
            {
                if(!ExportedOnly || IsPublic(evnt.AddMethod) || IsPublic(evnt.RemoveMethod) || IsPublic(evnt.RaiseMethod) || evnt.GetOtherMethods(true).Any(IsPublic))
                {
                    await analyzers.Analyze(evnt, declaresContext);
                }
            }

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(Type type, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = type.Name;

            if(type.IsConstructedGenericType)
            {
                node.SetClass(Classes.CodeGenericParameterizedType);
                await ReferenceMember(node, Properties.CodeGenericTypeDefinition, type.GetGenericTypeDefinition(), context, analyzers);
                foreach(var typeArg in type.GetGenericArguments())
                {
                    node.Set(Properties.CodeTypeArgument, ClrNamespaceUriFormatter.Instance, typeArg);
                }
            }else if(type.IsArray)
            {
                node.SetClass(Classes.CodeArrayType);
                node.Set(Properties.CodeArrayDimensions, type.GetArrayRank());
                await ReferenceMember(node, Properties.CodeArrayElementType, type.GetElementType(), context, analyzers);
            }else if(type.HasElementType)
            {
                // Some other form of constructed type not expressible by the vocabulary
                await ReferenceMember(node, Properties.CodeReferences, type.GetElementType(), context, analyzers);
            }else if(type.IsGenericParameter)
            {
                node.SetClass(Classes.CodeTypeVariable);
                node.Set(Properties.CodePosition, type.GenericParameterPosition);

                if(type.DeclaringMethod is { } declaringMethod)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringMethod, context, analyzers);
                }else if(type.DeclaringType is { } declaringType)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringType, context, analyzers);
                }
            }else{
                if(!type.IsPrimitive)
                {
                    node.SetClass(Classes.CodeComplexType);
                }
                node.Set(Properties.CodeSimpleName, name);
                node.Set(Properties.CodeCanonicalName, type.FullName ?? name);

                if(type.DeclaringType is { } declaringType)
                {
                    await ReferenceMember(node, Properties.CodeDeclaredBy, declaringType, context, analyzers);
                }else if(Namespace.FromAssembly(type.Assembly, type.Namespace) is { } ns)
                {
                    await ReferenceMember(node, Properties.CodeTypeDeclaredBy, ns, context, analyzers);
                }
            }

            return new(node, name);
        }
    }
}
