using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzer : IEntityAnalyzerProvider
    {
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(EntityAnalyzerComparer.Instance);

        public IEnumerable<IEntityAnalyzer<T>> GetAnalyzers<T>() where T : class
        {
            return Analyzers.OfType<IEntityAnalyzer<T>>();
        }

        class EntityAnalyzerComparer : TypeInheritanceComparer<object>
        {
            public static new readonly IComparer<object> Instance = new EntityAnalyzerComparer();

            private EntityAnalyzerComparer()
            {

            }

            static readonly Type analyzerType = typeof(IEntityAnalyzer<>);

            protected override IEnumerable<Type> SelectType(Type t)
            {
                foreach(var i in t.GetInterfaces())
                {
                    if(i.IsGenericType && i.GetGenericTypeDefinition().Equals(analyzerType))
                    {
                        yield return i;
                    }
                }
            }
        }
    }
}
