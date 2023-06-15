using IS4.SFI.Tools;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the PE module format storing a .NET assembly.
    /// </summary>
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
                if(coreAssemblyName == assemblyName.FullName)
                {
                    return context.LoadFromByteArray(dummyCore);
                }else if(assemblyName.Name is "netstandard" or "mscorlib" or "System.Runtime")
                {
                    return context.LoadFromAssemblyName(new AssemblyName(coreAssemblyName));
                }
                return null;
            }
        }

        static readonly byte[] dummyCore = Decompress(dummyCoreBase64);

        /// <summary>
        /// The following is a base64-encoded DEFLATE-compressed assembly
        /// with a few basic types from .NET Standard.
        /// The assembly is used only to pose as the core assembly for
        /// <see cref="MetadataLoadContext.MetadataLoadContext(MetadataAssemblyResolver, string?)"/>,
        /// and is deliberately empty to speed up the loading.
        /// </summary>
        /// <remarks>
        /// <code>
        /// .assembly netstandard
        /// {
        ///   .ver 2:0:0:0
        /// }
        /// .module netstandard.dll
        /// 
        /// .class public auto ansi beforefieldinit System.Object
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor() runtime managed
        /// {}
        /// }
        /// 
        /// .class public abstract auto ansi beforefieldinit System.ValueType extends System.Object
        /// {
        /// .method family hidebysig specialname rtspecialname instance void .ctor() runtime managed
        /// {}
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Void extends System.ValueType
        /// {
        ///   .pack 0
        ///   .size 1
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.String extends System.Object
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Char extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Boolean extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Byte extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.SByte extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Int16 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.UInt16 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Int32 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.UInt32 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Int64 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.UInt64 extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.IntPtr extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.UIntPtr extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Single extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public sequential ansi sealed beforefieldinit System.Double extends System.ValueType
        /// {
        /// }
        /// 
        /// .class public abstract auto ansi beforefieldinit System.Enum extends System.ValueType
        /// {
        /// .method family hidebysig specialname rtspecialname instance void .ctor() runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed System.Configuration.Assemblies.AssemblyHashAlgorithm extends System.Enum
        /// {
        /// .field public specialname rtspecialname int32 value__
        /// }
        /// 
        /// .class public auto ansi sealed System.Reflection.AssemblyNameFlags extends System.Enum
        /// {
        /// .field public specialname rtspecialname int32 value__
        /// }
        /// 
        /// .class public abstract auto ansi beforefieldinit System.Attribute extends System.Object
        /// {
        /// .method family hidebysig specialname rtspecialname instance void .ctor() runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyAlgorithmIdAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(uint32 algorithmId) runtime managed
        /// {}
        /// 
        /// .method public hidebysig specialname rtspecialname instance void .ctor(valuetype System.Configuration.Assemblies.AssemblyHashAlgorithm algorithmId) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyCompanyAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void  .ctor(string company) runtime managed
        /// {}
        /// 
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyConfigurationAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string configuration) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyCopyrightAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string copyright) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyCultureAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string culture) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyDefaultAliasAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string defaultAlias) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyDelaySignAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(bool delaySign) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyDescriptionAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string description) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyFileVersionAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string version) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyFlagsAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(valuetype System.Reflection.AssemblyNameFlags assemblyFlags) runtime managed
        /// {}
        /// 
        /// .method public hidebysig specialname rtspecialname instance void .ctor(uint32 'flags') runtime managed
        /// {}
        /// 
        /// .method public hidebysig specialname rtspecialname instance void .ctor(int32 assemblyFlags) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyInformationalVersionAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string informationalVersion) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyKeyFileAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string keyFile) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyKeyNameAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string keyName) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyMetadataAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string key, string 'value') runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyProductAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string product) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblySignatureKeyAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void  .ctor(string publicKey, string countersignature) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyTitleAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string title) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyTrademarkAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string trademark) runtime managed
        /// {}
        /// }
        /// 
        /// .class public auto ansi sealed beforefieldinit System.Reflection.AssemblyVersionAttribute extends System.Attribute
        /// {
        /// .method public hidebysig specialname rtspecialname instance void .ctor(string version) runtime managed
        /// {}
        /// }
        /// </code>
        /// </remarks>
        const string dummyCoreBase64 = "vZVNbBtFFMff2rFru4nrkJImFVI3DQKBwGrTNuGAUOJ80EBCA05CqaqWsXdib7veNbvjtEaoaqSChJAQILj0BDdAXCqQaJHKBQ5IqFyAK+TEBQ45caiEypu3Y3unNgL1wCb7f/ObN5/vzY6XT70DcQDow/fOHYDrED7T8O/PZXyzB77KwhfpW2PXjaVbY6tVOzDrvlfxWc0sM9f1hFnipt9wTds1504UzZpn8fzAQOZBNcbKPMCSYcDkwqbVGncb4mO7jRRA5q4JzdbCpsNyLFw3QMeSPxYW4zD9OkCO/ju2bf7fxwQ4HsG84BcF2p106KO9xrq6vJz3ueOVVcW0sv16O6wu3OuyjtO0CVjByT7EtRj3OpB6CsVnCoYaRS5zcyJ/KH/s0NTElKxJgIO6jckav4R5T2DeZLkofNutBLLFCq4BMw/ja0U4mQ5zNf702uIc2iryE5ILjldS82HIjBe3AGQY4bZxBIbDPcgz/Wjoh/34jkBY33oBHlY2CYMY/CS2kHpQHTpDqfzLUYsElbeQRhTFIG3kMPoZnN3AGQ2keeWLk2+p7ZP0vEYvaXRGI0ujcxq9otEFjS5pdEWjtzR6T6OrGn3Upi2kg5H9AXwMX8MAUh/RL7CDFEOScYkZcu9x8uWg39iB00gJokcU7SIqKEoRMUVpotcUZYg+ULSb6HNF/UQ/KBog+l1RligVC2kP0QFF94U5UjREdEbRXqILiu4nelfRMNGnivYRfadohOhXRaNEtxXth2RyNJ5VCnQu3xgZimfU+YrDlf9Mst+gRsPqwgjpATrzHerTKKFRUqNdERqjr6/jS2uUidBDmJPoyvoj9BjmJNovq9EejXIROoxfWtQ3pPn2ar5hjfZpFH7tY/GOPk56jHSWdJl0jbRE6pBeJL1M+nak/D7pJ6TXSG/E5f1yk8rfkH5PNT9T+TfSPyJjtn6l9Gv+z7heY8BUq/jksmc1HP4UFJuB4DU4UTrHywLWmdPgq806h3m3UQOXi0Aw12K+lbccB2pB2fMduwTrnm1BeLfCbJX5UPA8hzMXCk3BoUi66IrDk7AWGtQjEwRoUCePEqBBXRE+kbRFHNLhMOc1SmhmgoDXSk7zOAuqM07F821Rrak152c9d8OuNHwmbM/Nq6Y2D9q9nmM1vuAwvP5Vjxf4hoPbxOYwI3D1pYbozNEef9Hqds56tTpzm70ckUX0ctebvl2pih6uhiMaPu92zPENhr4Zx2ZBL6/DmkW70mOyOR6UfbveeyULtsPXuR/0dsoodVcvuhueX6OtMecfOz/LafCeDpmCbscyF8xignV7Vnw8l+Ue0ZI7ZjJeOGi3d9UWvRaw6jOL15h/vtvVtZt8WXg+bMoP4OxZYJ3DAOUw92gjqUZSmYVymEiwInlDUGnCUjsrsBnOCiwad9ggtXsEG86HsZVWhlLacI1QDyMFdfxQ7DJGBVfUcIXspyIFQkYFRCsI0c+ZrjSTroLgam7t1I2flj+L/Vj56+a1byH15aun10ePbr+JDYxYMtVnGkY6gTK4KotZKTHik7KYSpgxI5uFu5+/AQ==";

        static byte[] Decompress(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            using var buffer = new MemoryStream(bytes);
            using var deflate = new DeflateStream(buffer, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflate.CopyTo(output);
            return output.ToArray();
        }
    }
}
