using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Threading.Tasks;

namespace IS4.SFI
{
    /// <summary>
    /// Stores extension methods for operations on a <see cref="ILinkedNode"/> or <see cref="IEntityAnalyzers"/>.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Dynamically calls <see cref="IEntityAnalyzers.Analyze{T}(T, AnalysisContext)"/>
        /// based on the runtime type of <paramref name="entity"/> constrained to type <typeparamref name="TConstraint"/>.
        /// </summary>
        /// <param name="analyzers">The instance of <see cref="IEntityAnalyzers"/> to use.</param>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="context">The context to be passed to <see cref="IEntityAnalyzers.Analyze{T}(T, AnalysisContext)"/>.</param>
        /// <typeparam name="TConstraint">The constraining type to affect the selected runtime type.</typeparam>
        /// <returns>The result from the method, or the default value of <see cref="AnalysisResult"/> on failure.</returns>
        public static ValueTask<AnalysisResult> TryAnalyze<TConstraint>(this IEntityAnalyzers analyzers, TConstraint entity, AnalysisContext context) where TConstraint : class
        {
            if(entity == null) return default;
            try{
                return Constrained<TConstraint>.Analyze(analyzers, (dynamic)entity, context);
            }catch(RuntimeBinderException)
            {
                return default;
            }
        }

        /// <summary>
        /// Helper class to constrain the <see cref="Analyze{T}(IEntityAnalyzers, T, AnalysisContext)"/> method
        /// to an argument compatible with <typeparamref name="TConstraint"/>.
        /// </summary>
        /// <typeparam name="TConstraint">The constraining type.</typeparam>
        static class Constrained<TConstraint>
        {
            /// <inheritdoc cref="IEntityAnalyzers.Analyze{T}(T, AnalysisContext)"/>
            /// <param name="analyzers">The instance of <see cref="IEntityAnalyzers"/> to use.</param>
            /// <param name="entity"><inheritdoc cref="IEntityAnalyzers.Analyze{T}(T, AnalysisContext)" path="/param[@name='entity']"/></param>
            /// <param name="context"><inheritdoc cref="IEntityAnalyzers.Analyze{T}(T, AnalysisContext)" path="/param[@name='context']"/></param>
            public static ValueTask<AnalysisResult> Analyze<T>(IEntityAnalyzers analyzers, T entity, AnalysisContext context) where T : class, TConstraint
            {
                return analyzers.Analyze(entity, context);
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
        /// <typeparam name="TProp">The type of <paramref name="propertyValue"/>.</typeparam>
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
