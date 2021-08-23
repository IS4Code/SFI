using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzer : IEntityAnalyzer
    {
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(EntityAnalyzerComparer.Instance);

        public AnalysisResult Analyze<T>(T entity, AnalysisContext context) where T : class
        {
            if(entity == null) return default;
            foreach(var obj in Analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    try{
                        if(typeof(T).Equals(typeof(IStreamFactory)))
                        {
                            Console.Error.WriteLine($"Data ({((IStreamFactory)entity).Length} B)");
                        }else{
                            Console.Error.WriteLine(entity);
                        }
                        var result = analyzer.Analyze(entity, context, this);
                        if(result.Node != null)
                        {
                            return result;
                        }
                    }catch(InternalArchiverException e)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                        throw;
                    }catch(Exception e)
                    {
                        Console.Error.WriteLine("Error in analyzer " + obj);
                        Console.Error.WriteLine(e);
                    }
                }
            }
            return default;
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
