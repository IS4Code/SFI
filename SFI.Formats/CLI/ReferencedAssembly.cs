using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Security;

namespace IS4.SFI.Analyzers
{
    internal class ReferencedAssembly : Assembly
    {
        readonly Assembly referencingAssembly;
        readonly AssemblyName name;

        readonly Lazy<Assembly?> resolved;

        public Assembly? Resolved => resolved.Value;

        public ReferencedAssembly(Assembly referencing, AssemblyName referenced)
        {
            referencingAssembly = referencing is ReferencedAssembly indirect ? indirect.referencingAssembly : referencing;
            name = referenced;
            resolved = new(Resolve);
        }

        Assembly? Resolve()
        {
            var refType = referencingAssembly.GetType();
            var prop = refType.GetProperty("Loader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(prop != null && prop.PropertyType.Equals(typeof(MetadataLoadContext)) && prop.GetValue(referencingAssembly) is MetadataLoadContext context)
            {
                return context.LoadFromAssemblyName(name);
            }
            if(AssemblyLoadContext.GetLoadContext(referencingAssembly) is { } context2)
            {
                return context2.LoadFromAssemblyName(name);
            }
            return null;
        }

        public override AssemblyName GetName()
        {
            return name;
        }

        public override string ToString()
        {
            return name.ToString();
        }

        public override bool Equals(object o)
        {
            switch(o)
            {
                case ReferencedAssembly another:
                    return name.Equals(another.name);
                case Assembly other:
                    return Resolved is { } inner ? inner.Equals(other) : name.Equals(other.GetName());
            }
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override Type? GetType(string name)
        {
            if(name == ClrNamespaceUriFormatter.ReferenceAssemblyMarkerClass)
            {
                return null;
            }
            if(Resolved is { } inner)
            {
                return inner.GetType(name);
            }
            return base.GetType(name);
        }

        public override Type? GetType(string name, bool throwOnError)
        {
            if(name == ClrNamespaceUriFormatter.ReferenceAssemblyMarkerClass)
            {
                if(throwOnError)
                {
                    return base.GetType(name, true);
                }
                return null;
            }
            if(Resolved is { } inner)
            {
                return inner.GetType(name, throwOnError);
            }
            return base.GetType(name, throwOnError);
        }

        public override Type? GetType(string name, bool throwOnError, bool ignoreCase)
        {
            if(ClrNamespaceUriFormatter.ReferenceAssemblyMarkerClass.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                if(throwOnError)
                {
                    return base.GetType(name, true, ignoreCase);
                }
                return null;
            }
            if(Resolved is { } inner)
            {
                return inner.GetType(name, throwOnError, ignoreCase);
            }
            return base.GetType(name, throwOnError, ignoreCase);
        }

        #region Implementation
        public override string CodeBase => Resolved is { } inner ? inner.CodeBase : base.CodeBase;

        public override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if(Resolved is { } inner)
            {
                return inner.CreateInstance(typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
            }
            return base.CreateInstance(typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
        }

        public override IEnumerable<CustomAttributeData> CustomAttributes => Resolved is { } inner ? inner.CustomAttributes : base.CustomAttributes;

        public override IEnumerable<TypeInfo> DefinedTypes => Resolved is { } inner ? inner.DefinedTypes : base.DefinedTypes;

        public override MethodInfo EntryPoint => Resolved is { } inner ? inner.EntryPoint : base.EntryPoint;

        public override string EscapedCodeBase => Resolved is { } inner ? inner.EscapedCodeBase : base.EscapedCodeBase;

        public override IEnumerable<Type> ExportedTypes => Resolved is { } inner ? inner.ExportedTypes : base.ExportedTypes;

        public override string FullName => Resolved is { } inner ? inner.FullName : base.FullName;

        public override object[] GetCustomAttributes(bool inherit)
        {
            if(Resolved is { } inner)
            {
                return inner.GetCustomAttributes(inherit);
            }
            return base.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if(Resolved is { } inner)
            {
                return inner.GetCustomAttributes(attributeType, inherit);
            }
            return base.GetCustomAttributes(attributeType, inherit);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            if(Resolved is { } inner)
            {
                return inner.GetCustomAttributesData();
            }
            return base.GetCustomAttributesData();
        }

        public override Type[] GetExportedTypes()
        {
            if(Resolved is { } inner)
            {
                return inner.GetExportedTypes();
            }
            return base.GetExportedTypes();
        }

        public override FileStream GetFile(string name)
        {
            if(Resolved is { } inner)
            {
                return inner.GetFile(name);
            }
            return base.GetFile(name);
        }

        public override FileStream[] GetFiles()
        {
            if(Resolved is { } inner)
            {
                return inner.GetFiles();
            }
            return base.GetFiles();
        }

        public override FileStream[] GetFiles(bool getResourceModules)
        {
            if(Resolved is { } inner)
            {
                return inner.GetFiles(getResourceModules);
            }
            return base.GetFiles(getResourceModules);
        }

        public override Module[] GetLoadedModules(bool getResourceModules)
        {
            if(Resolved is { } inner)
            {
                return inner.GetLoadedModules(getResourceModules);
            }
            return base.GetLoadedModules(getResourceModules);
        }

        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            if(Resolved is { } inner)
            {
                return inner.GetManifestResourceInfo(resourceName);
            }
            return base.GetManifestResourceInfo(resourceName);
        }

        public override string[] GetManifestResourceNames()
        {
            if(Resolved is { } inner)
            {
                return inner.GetManifestResourceNames();
            }
            return base.GetManifestResourceNames();
        }

        public override Stream GetManifestResourceStream(string name)
        {
            if(Resolved is { } inner)
            {
                return inner.GetManifestResourceStream(name);
            }
            return base.GetManifestResourceStream(name);
        }

        public override Stream GetManifestResourceStream(Type type, string name)
        {
            if(Resolved is { } inner)
            {
                return inner.GetManifestResourceStream(type, name);
            }
            return base.GetManifestResourceStream(type, name);
        }

        public override Module GetModule(string name)
        {
            if(Resolved is { } inner)
            {
                return inner.GetModule(name);
            }
            return base.GetModule(name);
        }

        public override Module[] GetModules(bool getResourceModules)
        {
            if(Resolved is { } inner)
            {
                return inner.GetModules(getResourceModules);
            }
            return base.GetModules(getResourceModules);
        }

        public override AssemblyName GetName(bool copiedName)
        {
            if(!copiedName)
            {
                return GetName();
            }
            if(Resolved is { } inner)
            {
                return inner.GetName(copiedName);
            }
            return base.GetName(copiedName);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(Resolved is { } inner)
            {
                base.GetObjectData(info, context);
            }
            base.GetObjectData(info, context);
        }

        public override AssemblyName[] GetReferencedAssemblies()
        {
            if(Resolved is { } inner)
            {
                return inner.GetReferencedAssemblies();
            }
            return base.GetReferencedAssemblies();
        }

        public override Assembly GetSatelliteAssembly(CultureInfo culture)
        {
            if(Resolved is { } inner)
            {
                return inner.GetSatelliteAssembly(culture);
            }
            return base.GetSatelliteAssembly(culture);
        }

        public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
        {
            if(Resolved is { } inner)
            {
                return inner.GetSatelliteAssembly(culture, version);
            }
            return base.GetSatelliteAssembly(culture, version);
        }

        public override Type[] GetTypes()
        {
            if(Resolved is { } inner)
            {
                return inner.GetTypes();
            }
            return base.GetTypes();
        }

        public override bool GlobalAssemblyCache => Resolved is { } inner ? inner.GlobalAssemblyCache : base.GlobalAssemblyCache;

        public override long HostContext => Resolved is { } inner ? inner.HostContext : base.HostContext;

        public override string ImageRuntimeVersion => Resolved is { } inner ? inner.ImageRuntimeVersion : base.ImageRuntimeVersion;

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if(Resolved is { } inner)
            {
                return inner.IsDefined(attributeType, inherit);
            }
            return base.IsDefined(attributeType, inherit);
        }

        public override bool IsDynamic => Resolved is { } inner ? inner.IsDynamic : base.IsDynamic;

        public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
        {
            if(Resolved is { } inner)
            {
                return inner.LoadModule(moduleName, rawModule, rawSymbolStore);
            }
            return base.LoadModule(moduleName, rawModule, rawSymbolStore);
        }

        public override string Location => Resolved is { } inner ? inner.Location : base.Location;

        public override Module ManifestModule => Resolved is { } inner ? inner.ManifestModule : base.ManifestModule;

        public override event ModuleResolveEventHandler ModuleResolve {
            add{
                if(Resolved is { } inner)
                {
                    inner.ModuleResolve += value;
                }else{
                    base.ModuleResolve += value;
                }
            }
            remove{
                if(Resolved is { } inner)
                {
                    inner.ModuleResolve -= value;
                }else{
                    base.ModuleResolve -= value;
                }
            }
        }

        public override IEnumerable<Module> Modules => Resolved is { } inner ? inner.Modules : base.Modules;

        public override bool ReflectionOnly => Resolved is { } inner ? inner.ReflectionOnly : base.ReflectionOnly;

        public override SecurityRuleSet SecurityRuleSet => Resolved is { } inner ? inner.SecurityRuleSet : base.SecurityRuleSet;
        #endregion
    }
}
