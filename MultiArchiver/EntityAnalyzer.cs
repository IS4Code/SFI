using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IS4.MultiArchiver
{
    public class EntityAnalyzer : IEntityAnalyzer
    {
        public ICollection<object> Analyzers { get; } = new SortedSet<object>(TypeInheritanceComparer.Instance);

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

        class TypeInheritanceComparer : GlobalObjectComparer<object>
        {
            public static readonly IComparer<object> Instance = new TypeInheritanceComparer();

            private TypeInheritanceComparer()
            {

            }

            protected override int CompareInner(object x, object y)
            {
                var i1 = FindInterfaceOfType(x.GetType()) ?? throw new ArgumentException(null, nameof(x));
                var i2 = FindInterfaceOfType(y.GetType()) ?? throw new ArgumentException(null, nameof(y));
                return i1.IsAssignableFrom(i2) ? -1 : i2.IsAssignableFrom(i1) ? 1 : 0;
            }

            static readonly Type analyzerType = typeof(IEntityAnalyzer<>);
            private Type FindInterfaceOfType(Type t)
            {
                foreach(var i in t.GetInterfaces())
                {
                    if(i.IsGenericType && i.GetGenericTypeDefinition().Equals(analyzerType))
                    {
                        return i;
                    }
                }
                return null;
            }
        }
    }
}
