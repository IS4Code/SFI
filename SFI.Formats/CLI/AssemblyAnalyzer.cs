using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
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
    public class AssemblyAnalyzer : CodeElementAnalyzer<Assembly>
    {
        /// <summary>
        /// Whether to describe all namespaces in the assembly.
        /// </summary>
        [Description("Whether to describe all namespaces in the assembly.")]
        public bool DescribeNamespaces { get; set; }

        /// <summary>
        /// Whether to link all globally declared members and the entry point.
        /// </summary>
        [Description("Whether to link all globally declared members and the entry point.")]
        public bool LinkGlobals { get; set; }

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public AssemblyAnalyzer() : base(Common.ApplicationClasses, Classes.CodeElement)
        {
            SkipMediaObjectClass = false;
        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Assembly assembly, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);

            var name = assembly.GetName();
            node.Set(Properties.Name, name.FullName);
            node.Set(Properties.Version, name.Version.ToString());

            foreach(var reference in assembly.GetReferencedAssemblies())
            {
                await ReferenceMember(node, Properties.CodeReferences, new ReferencedAssembly(assembly, reference), context, analyzers);
            }

            await AnalyzeCustomAttributes(node, context, analyzers, assembly, assembly.GetCustomAttributesData());

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

            if(DescribeNamespaces)
            {
                await analyzers.Analyze(Namespace.FromAssembly(assembly), context.WithNode(node));
            }

            if(LinkGlobals)
            {
                if(assembly.EntryPoint is { } entryMethod)
                {
                    node.Set(Properties.CodeDeclares, ClrNamespaceUriFormatter.Instance, entryMethod);
                }

                foreach(var module in assembly.GetModules())
                {
                    if(!ExportedOnly)
                    {
                        foreach(var field in module.GetFields())
                        {
                            await ReferenceMember(node, Properties.CodeDeclares, field, context, analyzers);
                        }
                    }
                    foreach(var method in module.GetMethods())
                    {
                        if(!ExportedOnly || IsUnmanagedExport(method))
                        {
                            await ReferenceMember(node, Properties.CodeDeclares, method, context, analyzers);
                        }
                    }
                }
            }

            return new(node, assembly.ManifestModule.ScopeName);
        }

        /// <inheritdoc/>
        protected async override ValueTask<AnalysisResult> AnalyzeReference(Assembly member, ILinkedNode node, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var name = member.GetName();
            node.Set(Properties.Name, name.FullName);
            node.Set(Properties.Version, name.Version.ToString());
            return new(node, name.FullName);
        }

        static bool IsUnmanagedExport(MethodInfo method)
        {
            if(!method.IsPublic)
            {
                return false;
            }
            foreach(var attribute in method.GetCustomAttributesData())
            {
                string type;
                try{
                    type = attribute.AttributeType.FullName;
                }catch(TypeLoadException tle) when(!String.IsNullOrEmpty(tle.TypeName))
                {
                    type = tle.TypeName;
                }catch{
                    continue;
                }
                if(type == AttributeConstants.UnmanagedCallersOnlyAttributeType)
                {
                    return true;
                }
            }
            return false;
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
