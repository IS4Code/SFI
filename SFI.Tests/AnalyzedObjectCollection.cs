using IS4.SFI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// A mock implementation of <see cref="IEntityAnalyzers"/>, capturing instances
    /// requested for analysis inside <see cref="Analyzed"/>.
    /// </summary>
    public class AnalyzedObjectCollection : IEntityAnalyzers
    {
        /// <summary>
        /// A collection of entities encountered in <see cref="Analyze{T}(T, AnalysisContext)"/>,
        /// grouped by the requested type.
        /// </summary>
        public ConcurrentDictionary<Type, List<(object entity, AnalysisContext context)>> Analyzed { get; } = new ConcurrentDictionary<Type, List<(object, AnalysisContext)>>();

        /// <inheritdoc/>
        public async ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context) where T : notnull
        {
            if(!Analyzed.TryGetValue(typeof(T), out var list))
            {
                Analyzed[typeof(T)] = list = new List<(object, AnalysisContext)>();
            }
            list.Add((entity, context));
            return default;
        }
    }
}
