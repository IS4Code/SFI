using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Tools
{
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
                result = 1;
            }
            return result;
        }

        protected abstract int CompareInner(T x, T y);
    }
}
