using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET assemblies, expressed as instances of <see cref="Assembly"/>.
    /// </summary>
    [Description("An analyzer of .NET assemblies.")]
    public class AssemblyAnalyzer : MediaObjectAnalyzer<Assembly>
    {
        /// <summary>
        /// Whether to describe all namespaces in the assembly.
        /// </summary>
        [Description("Whether to describe all namespaces in the assembly.")]
        public bool Namespaces { get; set; }

        /// <summary>
        /// Whether to link all globally declared members and the entry point.
        /// </summary>
        [Description("Whether to link all globally declared members and the entry point.")]
        public bool Globals { get; set; }

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public AssemblyAnalyzer() : base(Common.ApplicationClasses, Classes.CodeElement)
        {

        }

        static readonly Dictionary<string, (PropertyUri propUri, bool useLang)> predefinedProperties = new()
        {
            //{ typeof(AssemblyProductAttribute).FullName, (Properties.) },
            { typeof(AssemblyCompanyAttribute).FullName, (Properties.Creator, true) },
            { typeof(AssemblyCopyrightAttribute).FullName, (Properties.CopyrightNotice, true) },
            { typeof(AssemblyInformationalVersionAttribute).FullName, (Properties.SoftwareVersion, false) },
            { typeof(AssemblyFileVersionAttribute).FullName, (Properties.Version, false) },
            { typeof(AssemblyTitleAttribute).FullName, (Properties.Title, true) },
            { typeof(AssemblyDescriptionAttribute).FullName, (Properties.Description, true) },
        };

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Assembly assembly, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var name = assembly.GetName();
            node.Set(Properties.Name, name.FullName);
            node.Set(Properties.Version, name.Version.ToString());

            var language = new LanguageCode(name.CultureInfo);

            foreach(var attribute in assembly.GetCustomAttributesData())
            {
                string type;
                object value;
                try{
                    if(attribute.ConstructorArguments.Count != 1)
                    {
                        continue;
                    }
                    try{
                        type = attribute.AttributeType.FullName;
                    }catch(TypeLoadException tle) when(!String.IsNullOrEmpty(tle.TypeName))
                    {
                        type = tle.TypeName;
                    }
                    value = attribute.ConstructorArguments[0].Value;
                }catch{
                    continue;
                }
                if(predefinedProperties.TryGetValue(type, out var def))
                {
                    var (propUri, useLang) = def;
                    if(useLang && value is string strValue)
                    {
                        node.Set(propUri, strValue, language);
                    }else{
                        node.Set(propUri, (dynamic)value);
                    }
                }else if(type == "System.Runtime.InteropServices.GuidAttribute" && value is string guidStr && Guid.TryParse(guidStr, out var guid))
                {
                    node.Set(Properties.Identifier, guidStr);
                    node.Set(Properties.Broader, UriTools.UuidUriFormatter, guid);
                }
            }

            foreach(var resName in assembly.GetManifestResourceNames())
            {
                var info = assembly.GetManifestResourceInfo(resName);
                if((info.ResourceLocation & ResourceLocation.Embedded) != 0)
                {
                    var fileInfo = new ResourceInfo(assembly, resName, info);
                    var resNode = node[UriTools.EscapePathString(resName)];
                    await analyzers.Analyze<IFileInfo>(fileInfo, context.WithParentLink(node, Properties.HasPart).WithNode(resNode));
                }
            }

            if(Namespaces)
            {
                await analyzers.Analyze(Namespace.FromAssembly(assembly), context.WithNode(node));
            }

            if(Globals)
            {
                if(assembly.EntryPoint is { } entryMethod)
                {
                    node.Set(Properties.CodeDeclares, ClrNamespaceUriFormatter.Instance, entryMethod);
                }

                foreach(var module in assembly.GetModules())
                {
                    foreach(var field in module.GetFields())
                    {
                        node.Set(Properties.CodeDeclares, ClrNamespaceUriFormatter.Instance, field);
                    }
                    foreach(var method in module.GetMethods())
                    {
                        node.Set(Properties.CodeDeclares, ClrNamespaceUriFormatter.Instance, method);
                    }
                }
            }

            return new(node, assembly.ManifestModule.ScopeName);
        }

        class ResourceInfo : IFileInfo
        {
            readonly Assembly assembly;
            readonly string name;
            readonly ManifestResourceInfo info;

            public ResourceInfo(Assembly assembly, string name, ManifestResourceInfo info)
            {
                this.assembly = assembly;
                this.name = name;
                this.info = info;

                using var stream = Open();
                Length = stream.Length;
            }

            public string? Name => info.FileName;

            public string? SubName => null;

            public string? Path => null;

            public int? Revision => null;

            public DateTime? CreationTime => null;

            public DateTime? LastWriteTime => null;

            public DateTime? LastAccessTime => null;

            public FileKind Kind => FileKind.Embedded;

            public FileAttributes Attributes => FileAttributes.Normal;

            public long Length { get; }

            public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

            public object? ReferenceKey => assembly;

            public object? DataKey => name;

            public Stream Open()
            {
                return assembly.GetManifestResourceStream(name);
            }

            public override string ToString()
            {
                return $"/{name}";
            }
        }
    }
}
