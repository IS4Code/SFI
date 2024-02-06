﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET fields, as instances of <see cref="FieldInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET fields.")]
    public class FieldAnalyzer : MemberAnalyzer<FieldInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public FieldAnalyzer() : base(Classes.CodeField)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(FieldInfo field, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = field.Name;
            var node = GetNode(field, context);

            if(!field.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, field);
            
            SetModifiers(
                node,
                isPublic: field.IsPublic,
                isPrivate: field.IsPrivate,
                isFamily: field.IsFamily,
                isFamilyAndAssembly: field.IsFamilyAndAssembly,
                isFamilyOrAssembly: field.IsFamilyOrAssembly,
                isAssembly: field.IsAssembly,
                isFinal: field.IsInitOnly,
                isStatic: field.IsStatic
            );

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, field.FieldType);

            AnalyzeCustomAttributes(node, field.GetCustomAttributesData());

            return new(node, name);
        }
    }
}