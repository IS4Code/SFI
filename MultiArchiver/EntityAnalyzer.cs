using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzer : IEntityAnalyzer
    {
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(EntityAnalyzerComparer.Instance);

        public ILinkedNode Analyze<T>(ILinkedNode parent, T entity, ILinkedNodeFactory nodeFactory) where T : class
        {
            if(entity == null) return null;
            foreach(var obj in Analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    try{
                        var result = analyzer.Analyze(parent, entity, nodeFactory);
                        if(result != null) return result;
                    }catch(Exception e)
                    {
                        Console.Error.WriteLine("Error in analyzer " + obj);
                        Console.Error.WriteLine(e);
                    }
                }
            }
            return null;
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
