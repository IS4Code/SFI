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
            bool isAttribute = type.Name.EndsWith("Attribute", StringComparison.Ordinal);

            type.CustomAttributes.Clear();
            type.SecurityDeclarations.Clear();
                        
            foreach(var intf in type.Interfaces.ToList())
            {
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
                genParam.CustomAttributes.Clear();
                genParam.Constraints.Clear();
                genParam.Name = null;
            }

            foreach(var field in type.Fields.ToList())
            {
                if((!isAttribute && !(field.Name == "value__" && field.FieldType.IsPrimitive)) || field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly)
                {
                    type.Fields.Remove(field);
                    continue;
                }
                field.CustomAttributes.Clear();
            }

            foreach(var method in type.Methods.ToList())
            {
                if(method.IsStatic || !method.IsVirtual || method.IsPrivate || method.IsAssembly || method.IsFamilyAndAssembly)
                {
                    type.Methods.Remove(method);
                    continue;
                }
                method.CustomAttributes.Clear();
                if(!method.IsAbstract)
                {
                    method.ImplAttributes = MethodImplAttributes.Runtime;
                }
                method.Body = null;
                method.CallingConvention = MethodCallingConvention.Default;
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

            foreach(var prop in type.Properties.ToList())
            {
                if(prop.GetMethod != null && !type.Methods.Contains(prop.GetMethod))
                {
                    prop.GetMethod = null;
                }
                if(prop.SetMethod != null && !type.Methods.Contains(prop.SetMethod))
                {
                    prop.SetMethod = null;
                }
                foreach(var other in prop.OtherMethods.ToList())
                {
                    if(!type.Methods.Contains(other))
                    {
                        prop.OtherMethods.Remove(other);
                    }
                }
                if(!isAttribute || prop.GetMethod == null && prop.SetMethod == null && !prop.HasOtherMethods)
                {
                    type.Properties.Remove(prop);
                    continue;
                }
                prop.CustomAttributes.Clear();
            }

            foreach(var evnt in type.Events.ToList())
            {
                if(evnt.AddMethod != null && !type.Methods.Contains(evnt.AddMethod))
                {
                    evnt.AddMethod = null;
                }
                if(evnt.RemoveMethod != null && !type.Methods.Contains(evnt.RemoveMethod))
                {
                    evnt.RemoveMethod = null;
                }
                if(evnt.InvokeMethod != null && !type.Methods.Contains(evnt.InvokeMethod))
                {
                    evnt.InvokeMethod = null;
                }
                foreach(var other in evnt.OtherMethods.ToList())
                {
                    if(!type.Methods.Contains(other))
                    {
                        evnt.OtherMethods.Remove(other);
                    }
                }
                if(!isAttribute || evnt.AddMethod == null && evnt.RemoveMethod == null && evnt.InvokeMethod == null && !evnt.HasOtherMethods)
                {
                    type.Events.Remove(evnt);
                    continue;
                }
                evnt.CustomAttributes.Clear();
            }

            foreach(var nested in type.NestedTypes.ToList())
            {
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
