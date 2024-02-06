using IS4.SFI.Services;
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

            node.Set(Properties.CodeType, ClrNamespaceUriFormatter.Instance, prop.PropertyType);

            if(prop.GetMethod is {  } getMethod && (!ExportedOnly || IsPublic(getMethod)))
            {
                node.Set(Properties.CodeReturnedBy, ClrNamespaceUriFormatter.Instance, getMethod);
            }

            foreach(var method in prop.GetAccessors(true))
            {
                if(!ExportedOnly || IsPublic(method))
                {
                    node.Set(Properties.CodeReferences, ClrNamespaceUriFormatter.Instance, method);
                }
            }

            AnalyzeCustomAttributes(node, prop.GetCustomAttributesData());

            return new(node, name);
        }
    }
}
