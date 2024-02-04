using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SFI.Formats.CLI.Assembly
{
    /// <summary>
    /// Provides support for encoding API information from .NET assemblies.
    /// </summary>
    /// <remarks>
    /// This type can be used to remove information from .NET assemblies
    /// that is not necessary for loading them as references, such as
    /// custom attributes, method bodies, and private members.
    /// </remarks>
    public static class AssemblyApiEncoder
    {
        /// <summary>
        /// Encodes all reference assemblies from a particular .NET version.
        /// </summary>
        /// <param name="frameworkVersion">The version of .NET Framework to use.</param>
        /// <returns>A sequence of encoded assemblies, using their names as keys.</returns>
        public static IEnumerable<KeyValuePair<string, string>> EncodeSystemAssemblies(string frameworkVersion)
        {
            var referenceAssemblies = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v" + frameworkVersion);

            var assemblies = Directory.EnumerateFiles(referenceAssemblies, "*.dll", SearchOption.AllDirectories);

            foreach(var assembly in assemblies)
            {
                if(StripAssembly(assembly, out var output, out var fullName))
                {
                    string data;
                    if(output.TryGetBuffer(out var buffer))
                    {
                        data = Convert.ToBase64String(buffer.Array, buffer.Offset, buffer.Count);
                    }else{
                        data = Convert.ToBase64String(output.ToArray());
                    }
                    yield return new(fullName, data);
                }
            }
        }

        /// <summary>
        /// Strips an assembly of non-metadata or non-public elements.
        /// </summary>
        /// <param name="assemblyPath">The path to the assembly.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>Whether the assembly was successfully loaded.</returns>
        public static bool StripAssembly(string assemblyPath, out MemoryStream output, out string name)
        {
            AssemblyDefinition adef;
            try{
                adef = AssemblyDefinition.ReadAssembly(assemblyPath);
            }catch{
                output = null!;
                name = null!;
                return false;
            }
            var aname = adef.Name;
            aname.PublicKey = null;
            aname.PublicKeyToken = null;
            name = aname.Name;

            adef.CustomAttributes.Clear();
            adef.SecurityDeclarations.Clear();

            foreach(var mdef in adef.Modules)
            {
                // Remove all non-type data
                mdef.Attributes = ModuleAttributes.ILOnly;
                foreach(var aref in mdef.AssemblyReferences)
                {
                    aref.PublicKey = null;
                    aref.PublicKeyToken = null;
                }
                mdef.CustomAttributes.Clear();
                mdef.CustomDebugInformations.Clear();
                mdef.ModuleReferences.Clear();
                mdef.Resources.Clear();

                foreach(var type in mdef.Types.ToList())
                {
                    // Remove all non-public types
                    if(type.IsNotPublic && type.FullName != "<Module>")
                    {
                        mdef.Types.Remove(type);
                        continue;
                    }
                    StripType(type);
                }
            }

            output = new MemoryStream();
            var temporaryPath = Path.GetTempFileName();
            try{
                using var file = File.Open(temporaryPath, FileMode.Create);
                adef.Write(file);
                file.Position = 0;
                using var deflate = new DeflateStream(output, CompressionMode.Compress, true);
                file.CopyTo(deflate);
                return true;
            }finally{
                File.Delete(temporaryPath);
            }
        }
        
        static void StripType(TypeDefinition type)
        {
            // Type is an attribute type (heuristic)
            bool isAttribute =
                type.IsClass && !(type.IsSealed && type.IsAbstract) &&
                type.BaseType is not (null or { FullName: "System.Object" }) && (
                    type.Name.EndsWith("Attribute", StringComparison.Ordinal) ||
                    type.BaseType.Name.EndsWith("Attribute", StringComparison.Ordinal) ||
                    type.CustomAttributes.Any(attr => attr.Constructor is { DeclaringType: { FullName: "System.AttributeUsageAttribute" } })
                );

            // Clear metadata
            type.CustomAttributes.Clear();
            type.SecurityDeclarations.Clear();
                        
            foreach(var intf in type.Interfaces.ToList())
            {
                // Remove all private implementations
                var itype = intf.InterfaceType;
                var itypedef = itype as TypeDefinition ?? (itype.IsGenericInstance ? itype.GetElementType() as TypeDefinition : null);
                if(itypedef != null && itypedef.IsNotPublic)
                {
                    type.Interfaces.Remove(intf);
                    continue;
                }
                intf.CustomAttributes.Clear();
            }

            foreach(var genParam in type.GenericParameters)
            {
                // Remove all metadata
                genParam.CustomAttributes.Clear();
                genParam.Constraints.Clear();
                genParam.Name = null;
            }

            if(!isAttribute && !type.IsEnum)
            {
                // Fields are of no use for non-attribute and non-enum types
                type.Fields.Clear();
            }else{
                foreach(var field in type.Fields.ToList())
                {
                    if(
                        // Field is static
                        field.IsStatic ||
                        // or not public or protected (and not the enum field)
                        (!type.IsEnum && (field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly)))
                    {
                        type.Fields.Remove(field);
                        continue;
                    }
                    // Only public/protected instance attribute fields and the underlying enum field remain
                    field.CustomAttributes.Clear();
                }
            }

            ICollection<MethodDefinition> setByProperties;
            if(isAttribute)
            {
                // Store all methods set by attribute properties
                setByProperties = new HashSet<MethodDefinition>(
                    type.Properties.Select(p => p.SetMethod).Where(m => m != null)
                );
            }else{
                setByProperties = Array.Empty<MethodDefinition>();
            }

            foreach(var method in type.Methods.ToList())
            {
                if(
                    (
                        // Method is not an instance attribute set accessor method
                        (method.IsStatic || !setByProperties.Contains(method)) &&
                        // and not an attribute constructor
                        !(isAttribute && method.IsConstructor) &&
                        // and not overridable
                        (!method.IsVirtual || method.IsFinal || type.IsSealed)
                    ) ||
                    // or not public or protected
                    method.IsPrivate || method.IsAssembly || method.IsFamilyAndAssembly)
                {
                    type.Methods.Remove(method);
                    continue;
                }
                method.CustomAttributes.Clear();
                if(!method.IsAbstract)
                {
                    // Assume runtime implementation for validity
                    method.ImplAttributes = MethodImplAttributes.Runtime;
                }
                method.Body = null;
                method.CallingConvention = MethodCallingConvention.Default;

                // Clear all other metadata
                method.DebugInformation = null;
                method.SecurityDeclarations.Clear();
                method.CustomDebugInformations.Clear();

                var methodReturn = method.MethodReturnType;
                methodReturn.Name = null;
                methodReturn.HasConstant = false;
                methodReturn.HasDefault = false;
                methodReturn.MarshalInfo = null;
                methodReturn.CustomAttributes.Clear();

                foreach(var param in method.Parameters)
                {
                    param.Name = null;
                    param.HasConstant = false;
                    param.HasDefault = false;
                    param.IsIn = false;
                    param.IsOut = false;
                    param.IsLcid = false;
                    param.IsOptional = false;
                    param.MarshalInfo = null;
                    param.CustomAttributes.Clear();
                }

                foreach(var genParam in method.GenericParameters)
                {
                    genParam.Name = null;
                    genParam.Constraints.Clear();
                    genParam.CustomAttributes.Clear();
                }
            }

            if(!isAttribute)
            {
                // Properties are not needed for non-attribute types
                type.Properties.Clear();
            }else{
                foreach(var prop in type.Properties.ToList())
                {
                    // Clear accessors other than set
                    prop.GetMethod = null;
                    prop.OtherMethods.Clear();
                    // Property is not publicly assignable
                    if(prop.SetMethod == null || !type.Methods.Contains(prop.SetMethod))
                    {
                        type.Properties.Remove(prop);
                        continue;
                    }
                    prop.CustomAttributes.Clear();
                }
            }

            // Events are not needed
            type.Events.Clear();

            foreach(var nested in type.NestedTypes.ToList())
            {
                // Remove nested types that are not public or protected
                if(nested.IsNestedPrivate || nested.IsNestedAssembly || nested.IsNestedFamilyAndAssembly)
                {
                    type.NestedTypes.Remove(nested);
                    continue;
                }
                StripType(nested);
            }
        }
    }
}
