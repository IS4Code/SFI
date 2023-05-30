using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of .NET assemblies, expressed as instances of <see cref="Assembly"/>.
    /// </summary>
    public class AssemblyAnalyzer : MediaObjectAnalyzer<Assembly>
    {
        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public AssemblyAnalyzer() : base(Common.ApplicationClasses)
        {

        }

        static readonly Dictionary<string, (PropertyUri propUri, bool useLang)> predefinedProperties = new()
        {
            //{ typeof(AssemblyProductAttribute).FullName, (Properties.) },
            { typeof(AssemblyCompanyAttribute).FullName, (Properties.Creator, true) },
            { typeof(AssemblyCopyrightAttribute).FullName, (Properties.CopyrightNotice, true) },
            { typeof(AssemblyInformationalVersionAttribute).FullName, (Properties.SoftwareVersion, false) },
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

            var module = assembly.ManifestModule;
            node.Set(Properties.OriginalName, module.Name);

            foreach(var attribute in assembly.GetCustomAttributesData())
            {
                string type;
                dynamic value;
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
                    if(useLang)
                    {
                        node.Set(propUri, value, language);
                    }else{
                        node.Set(propUri, value);
                    }
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

            return new(node, module.ScopeName);
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
