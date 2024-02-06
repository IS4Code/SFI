﻿using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET properties, as instances of <see cref="PropertyInfo"/>.
    /// </summary>
    [Description("An analyzer of .NET properties.")]
    public class PropertyAnalyzer : MemberAnalyzer<PropertyInfo>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public PropertyAnalyzer() : base(Classes.CodeVariable)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(PropertyInfo prop, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = prop.Name;
            var node = GetNode(prop, context);

            if(!prop.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);
            node.Set(Properties.Broader, ClrNamespaceUriFormatter.Instance, prop);

            await ReferenceMember(node, Properties.CodeType, prop.PropertyType, context, analyzers);

            if(prop.GetMethod is {  } getMethod && (!ExportedOnly || IsPublic(getMethod)))
            {
                await ReferenceMember(node, Properties.CodeReturnedBy, getMethod, context, analyzers);
            }

            foreach(var method in prop.GetAccessors(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    await ReferenceMember(node, Properties.CodeReferences, method, context, analyzers);
                }
            }

            await AnalyzeCustomAttributes(node, context, analyzers, prop.GetCustomAttributesData(), prop.GetOptionalCustomModifiers(), prop.GetRequiredCustomModifiers());

            return new(node, name);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(PropertyInfo prop, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = prop.Name;

            if(!prop.IsSpecialName)
            {
                node.Set(Properties.CodeSimpleName, name);
            }
            node.Set(Properties.CodeCanonicalName, name);

            await ReferenceMember(node, Properties.CodeDeclaredBy, prop.DeclaringType, context, analyzers);

            return new(node, name);
        }
    }
}
