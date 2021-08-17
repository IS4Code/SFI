using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;

namespace IS4.MultiArchiver
{
    public static class DynamicExtensions
    {
        public static ILinkedNode TryCreate(this ILinkedNodeFactory factory, ILinkedNode parent, object value)
        {
            if(value == null) return null;
            try
            {
                return factory.Create(parent, (dynamic)value);
            }catch(RuntimeBinderException)
            {
                return null;
            }
        }

        public static bool TrySet(this ILinkedNode node, PropertyUri property, ValueType value)
        {
            try{
                node.Set(property, (dynamic)value);
                return true;
            }catch(RuntimeBinderException)
            {
                return false;
            }catch(ArgumentException)
            {
                return false;
            }
        }

        public static bool TrySet<TProp>(this ILinkedNode node, IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ValueType value)
        {
            try{
                node.Set(propertyFormatter, propertyValue, (dynamic)value);
                return true;
            }catch(RuntimeBinderException)
            {
                return false;
            }catch(ArgumentException)
            {
                return false;
            }
        }
    }
}
