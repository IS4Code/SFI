using IS4.SFI.Tools;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the PE module format storing a .NET assembly.
    /// </summary>
    [Description("Represents the PE module format storing a .NET assembly.")]
    public class CliModuleFormat : ModuleFormat<Assembly>
    {
        const string baseType = "application/vnd.microsoft.portable-executable";

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public CliModuleFormat() : base("PE", baseType + ";type=cli", null)
        {

        }

        /// <inheritdoc/>
        public override string? GetExtension(Assembly value)
        {
            return value.EntryPoint == null ? "dll" : "exe";
        }

        /// <inheritdoc/>
        public override string? GetMediaType(Assembly value)
        {
            return baseType;
        }

        readonly ArrayPool<MetadataLoadContext?> contextPool = ArrayPool<MetadataLoadContext?>.Create(1, 50);

        const string coreAssemblyName = "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<Assembly, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var leash = contextPool.Rent(1, out var buffer);
            var loadContext = buffer[0] ??= new(Resolver.Instance, coreAssemblyName);
            var assembly = loadContext.LoadFromStream(stream);
            try{
                return await resultFactory(assembly, args);
            }finally{
                loadContext.Dispose();
                buffer[0] = null;
            }
        }

        class Resolver : MetadataAssemblyResolver
        {
            public static readonly Resolver Instance = new();

            public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
            {
                if(dummyAssemblies.TryGetValue(assemblyName.Name, out var assemblyData))
                {
                    return context.LoadFromByteArray(assemblyData);
                }
                return null;
            }
        }

        static readonly Dictionary<string, byte[]> dummyAssemblies = GetAssemblies();

        static Dictionary<string, byte[]> GetAssemblies()
        {
            var assemblies = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            using var resource = typeof(Resolver).Assembly.GetManifestResourceStream("EmbeddedAssemblies.txt");
            using var reader = new StreamReader(resource);
            string? name;
            while((name = reader.ReadLine()) != null)
            {
                var base64 = reader.ReadLine()!;
                var bytes = Convert.FromBase64String(base64);
                using var buffer = new MemoryStream(bytes);
                using var deflate = new DeflateStream(buffer, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflate.CopyTo(output);
                assemblies[name] = output.ToArray();
            }

            return assemblies;
        }
    }
}
