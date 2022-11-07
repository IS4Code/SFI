using System;
using System.Reflection;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Extension methods for defining RDF vocabularies.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Initializes all static (<see cref="BindingFlags.Static"/>) fields on the type
        /// specified by <paramref name="type"/>, whether they are public (<see cref="BindingFlags.Public"/>)
        /// or non-public (<see cref="BindingFlags.NonPublic"/>), if they have a corresponding
        /// custom attribute of type <see cref="UriAttribute"/> to the value specified
        /// by the attribute, such as by calling one of the constructors
        /// <see cref="ClassUri.ClassUri(UriAttribute, string)"/>,
        /// <see cref="PropertyUri.PropertyUri(UriAttribute, string)"/>,
        /// <see cref="IndividualUri.IndividualUri(UriAttribute, string)"/>,
        /// <see cref="DatatypeUri.DatatypeUri(UriAttribute, string)"/>,
        /// or <see cref="GraphUri.GraphUri(UriAttribute, string)"/>.
        /// When called from the type initializer, this also works on read-only fields.
        /// </summary>
        /// <param name="type">The type whose fields to initialize.</param>
        public static void InitializeUris(this Type type)
        {
            foreach(var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var uriAttribute = field.GetCustomAttribute<UriAttribute>();
                if(uriAttribute != null)
                {
                    // Assumes that the constructor fieldType(UriAttribute, string) exists
                    field.SetValue(null, Activator.CreateInstance(field.FieldType, uriAttribute, field.Name));
                }
            }
        }

        /// <summary>
        /// Converts the first character of a string to lowercase.
        /// </summary>
        /// <param name="str">The input string to convert.</param>
        /// <returns>
        /// A string with the first character of <paramref name="str"/>
        /// converted to lowercase, followed by the resf of the string.
        /// </returns>
        public static string ToCamelCase(this string str)
        {
            if(str.Length == 0) return str;
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
        }
    }
}
