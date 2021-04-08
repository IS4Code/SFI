using IS4.MultiArchiver.Services;
using Microsoft.CSharp.RuntimeBinder;

namespace IS4.MultiArchiver
{
    internal static class Extensions
    {
        public static ILinkedNode TryCreate(this ILinkedNodeFactory factory, object value)
        {
            if(value == null) return null;
            try
            {
                return factory.Create((dynamic)value);
            }catch(RuntimeBinderException)
            {
                return null;
            }
        }
    }
}
