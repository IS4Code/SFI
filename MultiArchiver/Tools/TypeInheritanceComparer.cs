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
            if(x == null) throw new ArgumentException(null, nameof(x));
            if(y == null) throw new ArgumentException(null, nameof(y));
            int order = 0;
            foreach(var t1 in SelectType(x.GetType()))
            {
                foreach(var t2 in SelectType(y.GetType()))
                {
                    int subOrder = t1.IsAssignableFrom(t2) ? -1 : t2.IsAssignableFrom(t1) ? 1 : 0;
                    if(subOrder != 0)
                    {
                        if(order == 0)
                        {
                            order = subOrder;
                        }else if(order != subOrder)
                        {
                            throw new NotSupportedException("Cannot determine order of objects.");
                        }
                    }
                }
            }
            return order;
        }

        protected virtual IEnumerable<Type> SelectType(Type initial)
        {
            yield return initial;
        }
    }
}
