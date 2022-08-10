using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Stores extension methods for operations on a <see cref="ILinkedNode"/> or <see cref="IEntityAnalyzerProvider"/>.
    /// </summary>
    public static class DynamicExtensions
    {
        public static async ValueTask<AnalysisResult> TryAnalyze(this IEntityAnalyzerProvider analyzers, object entity, AnalysisContext context)
        {
            if(entity == null) return default;
            try
            {
                return await analyzers.Analyze((dynamic)entity, context);
            }catch(RuntimeBinderException)
            {
                return default;
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
