using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Tools
{
    public class TypeInheritanceComparer<T> : GlobalObjectComparer<T> where T : class
    {
        public static readonly IComparer<T> Instance = new TypeInheritanceComparer<T>();

        protected TypeInheritanceComparer()
        {

        }

        protected sealed override int CompareInner(T x, T y)
        {
            var t1 = SelectType(x.GetType()) ?? throw new ArgumentException(null, nameof(x));
            var t2 = SelectType(y.GetType()) ?? throw new ArgumentException(null, nameof(y));
            return t1.IsAssignableFrom(t2) ? -1 : t2.IsAssignableFrom(t1) ? 1 : 0;
        }

        protected virtual Type SelectType(Type initial)
        {
            return initial;
        }
    }
}
