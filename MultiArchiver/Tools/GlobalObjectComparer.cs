using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IS4.MultiArchiver.Tools
{
    static class GlobalObjectComparer
    {
        static readonly ConditionalWeakTable<object, ValueType> ranks = new ConditionalWeakTable<object, ValueType>();
        static long rank;
        static readonly ConditionalWeakTable<object, ValueType>.CreateValueCallback increment = obj => Interlocked.Increment(ref rank);

        public static long GetKey(object obj)
        {
            return (long)ranks.GetValue(obj, increment);
        }
    }

    public abstract class GlobalObjectComparer<T> : IComparer<T> where T : class
    {
        public int Compare(T x, T y)
        {
            if(x == null) throw new ArgumentNullException(nameof(x));
            if(y == null) throw new ArgumentNullException(nameof(y));
            if(x == y) return 0;
            var result = CompareInner(x, y);
            if(result == 0)
            {
                result = RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));
                if(result == 0)
                {
                    result = GlobalObjectComparer.GetKey(x).CompareTo(GlobalObjectComparer.GetKey(y));
                }
            }
            return result;
        }

        protected abstract int CompareInner(T x, T y);
    }
}
