using System;
using System.Collections.Generic;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Compares objects of a specific type based on their concrete types,
    /// placing the more derived types before the less derived types.
    /// </summary>
    /// <typeparam name="T">The general type of the objects.</typeparam>
    public class TypeInheritanceComparer<T> : GlobalObjectComparer<T> where T : class
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        public static readonly IComparer<T> Instance = new TypeInheritanceComparer<T>();

        /// <inheritdoc/>
        protected TypeInheritanceComparer()
        {

        }

        /// <inheritdoc/>
        protected sealed override int CompareInner(T x, T y)
        {
            if(x == null) throw new ArgumentException(null, nameof(x));
            if(y == null) throw new ArgumentException(null, nameof(y));
            int order = 0;
            foreach(var t1 in SelectType(x.GetType()))
            {
                foreach(var t2 in SelectType(y.GetType()))
                {
                    int subOrder = t1.Equals(t2) ? 0 : t1.IsAssignableFrom(t2) ? -1 : t2.IsAssignableFrom(t1) ? 1 : 0;
                    if(subOrder != 0)
                    {
                        // This pair is ordered
                        if(order == 0)
                        {
                            order = subOrder;
                        }else if(order != subOrder)
                        {
                            // The order differs from a previous order, it is ambiguous
                            throw new NotSupportedException("Cannot determine order of objects.");
                        }
                    }
                }
            }
            return order;
        }

        /// <summary>
        /// Retrieves the collection of types relevant when comparing a given type.
        /// </summary>
        /// <param name="initial">The initial type to compare.</param>
        /// <returns>The collection of all compared types (only <paramref name="initial"/> by default)</returns>
        protected virtual IEnumerable<Type> SelectType(Type initial)
        {
            yield return initial;
        }
    }
}
