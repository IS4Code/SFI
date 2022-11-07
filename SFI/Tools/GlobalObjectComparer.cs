using System;
using System.Collections.Generic;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// An abstract comparer that provides an order for instances of type
    /// <typeparamref name="T"/> while allowing custom comparison and keeping
    /// non-identical instances ordered.
    /// </summary>
    /// <typeparam name="T">The object type to compare.</typeparam>
    public abstract class GlobalObjectComparer<T> : IComparer<T> where T : class
    {
        /// <inheritdoc/>
        public int Compare(T x, T y)
        {
            if(x == null) throw new ArgumentNullException(nameof(x));
            if(y == null) throw new ArgumentNullException(nameof(y));
            if(x == y) return 0;
            var result = CompareInner(x, y);
            if(result == 0)
            {
                // SortedSet works better without consistent order
                result = 1;
            }
            return result;
        }

        /// <summary>
        /// The internal comparison method.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// An integer with the same semantics as <see cref="IComparer{T}.Compare(T, T)"/>.
        /// Even if it returns 0, order is maintained.
        /// </returns>
        protected abstract int CompareInner(T x, T y);
    }
}
