using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    using static BindingFlags;

    /// <summary>
    /// An analyzer of .NET code elements.
    /// </summary>
    /// <typeparam name="T">The type of the code element.</typeparam>
    public abstract class CodeElementAnalyzer<T> : EntityAnalyzer<T>, IEntityAnalyzer<CodeElementAnalyzer<T>.Reference> where T : ICustomAttributeProvider
    {
        readonly ClassUri elementClass;

        /// <summary>
        /// Whether to analyze exported members only.
        /// </summary>
        [Description("Whether to analyze exported members only.")]
        public bool ExportedOnly { get; set; }

        /// <summary>
        /// Whether to analyze outgoing references to members.
        /// </summary>
        [Description("Whether to analyze outgoing references to members.")]
        public bool AnalyzeReferences { get; set; }

        /// <summary>
        /// The <see cref="BindingFlags"/> to use when browsing members.
        /// </summary>
        protected const BindingFlags BindingFlags = Instance | Static | Public | NonPublic | DeclaredOnly;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public CodeElementAnalyzer()
        {

        }

        /// <summary>
        /// Creates a new instance of the analyzer with a specific code element class.
        /// </summary>
        /// <param name="elementClass">The concrete class of the code element.</param>
        public CodeElementAnalyzer(ClassUri elementClass)
        {
            this.elementClass = elementClass;
        }

        /// <inheritdoc/>
        protected override void InitNode(ILinkedNode node, AnalysisContext context)
        {
            base.InitNode(node, context);

            node.SetClass(Classes.CodeElement);
            node.SetClass(elementClass);
        }

        /// <summary>
        /// Analyzes the collection of custom attributes and modifiers on a node.
        /// </summary>
        /// <param name="node">The linked node to use.</param>
        /// <param name="attributes">The custom attributes.</param>
        /// <param name="optionalModifiers">The optional modifiers.</param>
        /// <param name="requiredModifiers">The required modifiers.</param>
        /// <returns>The task representing the operation.</returns>
        protected async ValueTask AnalyzeCustomAttributes(ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers, IEnumerable<CustomAttributeData> attributes, IEnumerable<Type>? optionalModifiers = null, IEnumerable<Type>? requiredModifiers = null)
        {
            foreach(var attr in attributes)
            {
                await ReferenceMember(node, Properties.CodeAnnotation, attr.AttributeType, context, analyzers);
            }
            foreach(var type in optionalModifiers ?? Type.EmptyTypes)
            {
                await ReferenceMember(node, Properties.CodeAnnotation, type, context, analyzers);
            }
            foreach(var type in requiredModifiers ?? Type.EmptyTypes)
            {
                await ReferenceMember(node, Properties.CodeAnnotation, type, context, analyzers);
            }
        }

        /// <summary>
        /// Sets the member modifiers.
        /// </summary>
        /// <param name="node">The linked node to use.</param>
        /// <param name="isPublic">Whether the visibility is public.</param>
        /// <param name="isPrivate">Whether the visibility is private.</param>
        /// <param name="isFamily">Whether the visibility is family.</param>
        /// <param name="isFamilyAndAssembly">Whether the visibility is family and assembly.</param>
        /// <param name="isFamilyOrAssembly">Whether the visibility is family or assembly.</param>
        /// <param name="isAssembly">Whether the visibility is assembly.</param>
        /// <param name="isAbstract">Whether the member is abstract.</param>
        /// <param name="isFinal">Whether the member is final.</param>
        /// <param name="isStatic">Whether the member is static.</param>
        protected void SetModifiers(ILinkedNode node, bool isPublic = false, bool isPrivate = false, bool isFamily = false, bool isFamilyAndAssembly = false, bool isFamilyOrAssembly = false, bool isAssembly = false, bool isAbstract = false, bool isFinal = false, bool isStatic = false)
        {
            if(isPublic)
            {
                node.Set(Properties.CodeModifier, Individuals.CodePublicModifier);
            }
            if(isPrivate)
            {
                node.Set(Properties.CodeModifier, Individuals.CodePrivateModifier);
            }
            if(isFamily)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeProtectedModifier);
            }
            if(isFamilyAndAssembly)
            {
                node.Set(Properties.CodeModifier, Individuals.CodePrivateModifier);
                node.Set(Properties.CodeModifier, Individuals.CodeProtectedModifier);
            }
            if(isFamilyOrAssembly)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeProtectedModifier);
                node.Set(Properties.CodeModifier, Individuals.CodeDefaultModifier);
            }
            if(isAssembly)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeDefaultModifier);
            }
            if(isAbstract)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeAbstractModifier);
            }
            if(isFinal)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeFinalModifier);
            }
            if(isStatic)
            {
                node.Set(Properties.CodeModifier, Individuals.CodeStaticModifier);
            }
        }

        /// <summary>
        /// Links <paramref name="parent"/> to a referenced member. If the member
        /// is within the same assembly, it is sent to analysis.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="parent">The node referencing the member.</param>
        /// <param name="property">The property referencing the member.</param>
        /// <param name="member">The referenced member.</param>
        /// <param name="context">The context to use when analyzing the member.</param>
        /// <param name="analyzers">The <see cref="IEntityAnalyzers"/> collection.</param>
        /// <returns>The task representing the operation.</returns>
        protected async ValueTask ReferenceMember<TMember>(ILinkedNode parent, PropertyUri property, TMember? member, AnalysisContext context, IEntityAnalyzers analyzers) where TMember : MemberInfo
        {
            if(member == null)
            {
                return;
            }
            if(
                AnalyzeReferences &&
                GetMemberAssembly(member, out var assembly) &&
                await ReferenceMemberInAssembly(assembly, parent, property, member, ClrNamespaceUriFormatter.Instance, context, analyzers))
            {
                return;
            }
            parent.Set(property, ClrNamespaceUriFormatter.Instance, member);
        }

        /// <inheritdoc cref="ReferenceMember{TMember}(ILinkedNode, PropertyUri, TMember?, AnalysisContext, IEntityAnalyzers)"/>
        protected async ValueTask ReferenceMember(ILinkedNode parent, PropertyUri property, ParameterInfo? member, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            if(member == null)
            {
                return;
            }
            if(
                AnalyzeReferences &&
                GetMemberAssembly(member, out var assembly) &&
                await ReferenceMemberInAssembly(assembly, parent, property, member, ClrNamespaceUriFormatter.Instance, context, analyzers))
            {
                return;
            }
            parent.Set(property, ClrNamespaceUriFormatter.Instance, member);
        }

        /// <inheritdoc cref="ReferenceMember{TMember}(ILinkedNode, PropertyUri, TMember?, AnalysisContext, IEntityAnalyzers)"/>
        protected async ValueTask ReferenceMember(ILinkedNode parent, PropertyUri property, Namespace? member, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            if(member == null)
            {
                return;
            }
            if(
                AnalyzeReferences &&
                GetMemberAssembly(member, out var assembly) &&
                await ReferenceMemberInAssembly(assembly, parent, property, member, ClrNamespaceUriFormatter.Instance, context, analyzers))
            {
                return;
            }
            parent.Set(property, ClrNamespaceUriFormatter.Instance, member);
        }

        static bool GetMemberAssembly(MemberInfo member, [MaybeNullWhen(false)] out Assembly assembly)
        {
            var type = member.DeclaringType ?? (member as Type);
            assembly = type?.Assembly;
            return assembly != null;
        }

        static bool GetMemberAssembly(ParameterInfo param, [MaybeNullWhen(false)] out Assembly assembly)
        {
            return GetMemberAssembly(param.Member, out assembly);
        }

        static bool GetMemberAssembly(Namespace param, [MaybeNullWhen(false)] out Assembly assembly)
        {
            assembly = param.Assembly;
            return assembly != null;
        }

        static async ValueTask<bool> ReferenceMemberInAssembly<TMember>(Assembly assembly, ILinkedNode parent, PropertyUri property, TMember member, IIndividualUriFormatter<TMember> formatter, AnalysisContext context, IEntityAnalyzers analyzers) where TMember : ICustomAttributeProvider
        {
            // Check the assembly defining the member
            if(assembly.GetType(ClrNamespaceUriFormatter.ReferenceAssemblyMarkerClass, false) == null)
            {
                // Do not analyze reference assemblies
                var reference = new CodeElementAnalyzer<TMember>.Reference(member);
                var node = context.NodeFactory.Create(formatter, member);
                var newContext = context.WithParentLink(parent, property).WithNode(node);
                await analyzers.Analyze(reference, newContext);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Analyzes a reference to a member. When overriden, this method should
        /// describe only the members of <paramref name="member"/> that
        /// are defined by its signature, and not any attributes. Only the members
        /// declaring <paramref name="member"/> should be referenced in order
        /// not to cause loops.
        /// </summary>
        /// <param name="member">The member to analyze.</param>
        /// <param name="node">The node representing <paramref name="member"/>.</param>
        /// <param name="context"><inheritdoc cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)" path="/param[@name='context']"/></param>
        /// <param name="analyzers"><inheritdoc cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)" path="/param[@name='analyzers']"/></param>
        /// <returns><inheritdoc cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)" path="/returns"/></returns>
        protected abstract ValueTask<AnalysisResult> AnalyzeReference(T member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers);

        ValueTask<AnalysisResult> IEntityAnalyzer<MemberAnalyzer<T>.Reference>.Analyze(MemberAnalyzer<T>.Reference reference, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return AnalyzeReference(reference.Member, GetNode(context), context, analyzers);
        }

        /// <summary>
        /// Represents a reference to another member in the same assembly.
        /// </summary>
        public struct Reference : IEquatable<Reference>, IIdentityKey
        {
            /// <summary>
            /// The member this instance is a reference to.
            /// </summary>
            public T Member { get; }

            /// <summary>
            /// Creates a new instance of the reference.
            /// </summary>
            /// <param name="member">The value of <see cref="Member"/>.</param>
            public Reference(T member)
            {
                Member = member;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return Member.ToString();
            }

            /// <inheritdoc/>
            public bool Equals(Reference other)
            {
                return Member.Equals(other);
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return obj is Reference other && Equals(other);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return Member.GetHashCode();
            }

            static readonly Type thisType = typeof(Reference);

            object? IIdentityKey.ReferenceKey => Member;

            object? IIdentityKey.DataKey => thisType;
        }
    }
}
