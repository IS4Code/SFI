using System;
using System.Reflection;

namespace IS4.SFI.Application.Tools
{
    /// <summary>
    /// Provides extension methods for application-related classes.
    /// </summary>
    public static class Extensions
    {
        static readonly Func<Assembly, Type[]> getForwardedTypes;

        static Extensions()
        {
            var forwardedTypesMethod = typeof(Assembly).GetMethod("GetForwardedTypes", Type.EmptyTypes);
            if(forwardedTypesMethod != null)
            {
                getForwardedTypes = (Func<Assembly, Type[]>)Delegate.CreateDelegate(typeof(Func<Assembly, Type[]>), forwardedTypesMethod);
            }else{
                getForwardedTypes = _ => Array.Empty<Type>();
            }
        }

        /// <summary>
        /// Retrieves the collection of forwarded types in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to browse.</param>
        /// <returns>The array of types forwarded to another assembly.</returns>
        public static Type[] GetForwardedTypes(this Assembly assembly)
        {
            return getForwardedTypes(assembly);
        }
    }
}
