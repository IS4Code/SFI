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
        /// <summary>
        /// Dynamically calls <see cref="IEntityAnalyzerProvider.Analyze{T}(T, AnalysisContext)"/>
        /// based on the runtime type of <paramref name="entity"/>.
        /// </summary>
        /// <param name="analyzers">The instance of <see cref="IEntityAnalyzerProvider"/> to use.</param>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="context">The context to be passed to <see cref="IEntityAnalyzerProvider.Analyze{T}(T, AnalysisContext)"/>.</param>
        /// <returns>The result from the method, or the default value of <see cref="AnalysisResult"/> on failure.</returns>
        public static async ValueTask<AnalysisResult> TryAnalyze(this IEntityAnalyzerProvider analyzers, object entity, AnalysisContext context)
        {
            if(entity == null) return default;
            try{
                return await analyzers.Analyze((dynamic)entity, context);
            }catch(RuntimeBinderException)
            {
                return default;
            }
        }

        /// <summary>
        /// Dynamically calls <see cref="ILinkedNode.Set{TValue}(PropertyUri, TValue)"/>
        /// based on the runtime type of <paramref name="value"/>.
        /// </summary>
        /// <param name="node">The instance of <see cref="ILinkedNode"/> to use.</param>
        /// <param name="property">The property to assign.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>Whether the call was successful or not.</returns>
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

        /// <summary>
        /// Dynamically calls <see cref="ILinkedNode.Set{TProp, TValue}(IPropertyUriFormatter{TProp}, TProp, TValue)"/>
        /// based on the runtime type of <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="node">The instance of <see cref="ILinkedNode"/> to use.</param>
        /// <param name="propertyFormatter">The formatter to provide the property based on <paramref name="propertyValue"/>.</param>
        /// <param name="propertyValue">The value to pass to the <paramref name="propertyFormatter"/>.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>Whether the call was successful or not.</returns>
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
