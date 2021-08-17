using System;
using System.Reflection;

namespace IS4.MultiArchiver.Vocabulary
{
    public static class Extensions
    {
        public static void InitializeUris(this Type type)
        {
            foreach(var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var uriAttribute = field.GetCustomAttribute<UriAttribute>();
                if(uriAttribute != null)
                {
                    field.SetValue(null, Activator.CreateInstance(field.FieldType, uriAttribute, field.Name));
                }
            }
        }

        public static string ToCamelCase(this string str)
        {
            if(str.Length == 0) return str;
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
        }
    }
}
