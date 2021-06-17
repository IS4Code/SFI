using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IS4.MultiArchiver
{
    public class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
    {
        public new static readonly IEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
            
        public override bool Equals(T x, T y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
