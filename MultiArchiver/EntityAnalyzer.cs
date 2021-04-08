using IS4.MultiArchiver.Services;
using System.Collections.Concurrent;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzer : IEntityAnalyzer
    {
        public ConcurrentBag<object> Analyzers { get; } = new ConcurrentBag<object>();

        public ILinkedNode Analyze<T>(T entity, ILinkedNodeFactory nodeFactory) where T : class
        {
            if(entity == null) return null;
            foreach(var obj in Analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    var result = analyzer.Analyze(entity, nodeFactory);
                    if(result != null) return result;
                }
            }
            return null;
        }
    }
}
