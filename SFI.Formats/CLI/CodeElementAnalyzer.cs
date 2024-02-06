using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace IS4.SFI.Analyzers
{
    using static BindingFlags;

    /// <summary>
    /// An analyzer of .NET code elements.
    /// </summary>
    /// <typeparam name="T">The type of the code element.</typeparam>
    public abstract class CodeElementAnalyzer<T> : EntityAnalyzer<T> where T : ICustomAttributeProvider
    {
        readonly ClassUri elementClass;

        /// <summary>
        /// Whether to analyze exported members only.
        /// </summary>
        [Description("Whether to analyze exported members only.")]
        public bool ExportedOnly { get; set; }

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
        /// Analyzes the collection of custom attributes on a node.
        /// </summary>
        /// <param name="node">The linked node to use.</param>
        /// <param name="attributes">The custom attributes.</param>
        protected void AnalyzeCustomAttributes(ILinkedNode node, IEnumerable<CustomAttributeData> attributes)
        {
            foreach(var attr in attributes)
            {
                node.Set(Properties.CodeAnnotation, ClrNamespaceUriFormatter.Instance, attr.AttributeType);
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
    }
}
