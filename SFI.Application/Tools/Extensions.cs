using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Provides extension methods for application-related classes.
    /// </summary>
    public static class Extensions
    {
        static readonly Func<Assembly, IEnumerable<Type>>? getForwardedTypes;

        static Extensions()
        {
            var forwardedTypesMethod = typeof(Assembly).GetMethod("GetForwardedTypes", Type.EmptyTypes);
            if(forwardedTypesMethod != null)
            {
                getForwardedTypes = (Func<Assembly, Type[]>)Delegate.CreateDelegate(typeof(Func<Assembly, Type[]>), forwardedTypesMethod);
            }
        }

        /// <summary>
        /// Retrieves the collection of forwarded types in <paramref name="assembly"/>.
        /// If this information is unavailable, types in all referenced assemblies
        /// that are also matched via <see cref="InternalsVisibleToAttribute"/>
        /// are returned instead.
        /// </summary>
        /// <param name="assembly">The assembly to browse.</param>
        /// <returns>The array of types forwarded or referenced in another assembly.</returns>
        public static IEnumerable<Type> GetForwardedOrReferencedTypes(this Assembly assembly)
        {
            return getForwardedTypes?.Invoke(assembly) ?? GetReferencedTypes(assembly);
        }

        static IEnumerable<Type> GetReferencedTypes(Assembly assembly)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var attr in assembly.GetCustomAttributes<InternalsVisibleToAttribute>())
            {
                names.Add(new AssemblyName(attr.AssemblyName).Name);
            }
            var context = AssemblyLoadContext.GetLoadContext(assembly);
            if(context == null)
            {
                throw new ArgumentException("Assembly is not runtime-loaded.", nameof(assembly));
            }
            foreach(var refName in assembly.GetReferencedAssemblies())
            {
                if(names.Contains(refName.Name))
                {
                    var refAssembly = context.LoadFromAssemblyName(refName);
                    if(refAssembly != null)
                    {
                        foreach(var type in refAssembly.ExportedTypes)
                        {
                            yield return type;
                        }
                    }
                }
            }
        }
    }
}
