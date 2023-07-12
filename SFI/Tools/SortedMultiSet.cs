using System.Collections.Generic;
using System.Linq;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Provides an alternative to <see cref="SortedSet{T}"/> that allows easier
    /// manipulation with duplicate values by using an extra <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    public class SortedMultiSet<T> : SortedDictionary<T, HashSet<T>>, ICollection<T>, IReadOnlyCollection<T>
    {
        readonly IEqualityComparer<T> equalityComparer;

        /// <summary>
        /// Creates a new instance of the collection.
        /// </summary>
        /// <param name="comparer">The <see cref="IComparer{T}"/> instance used for sorting values.</param>
        /// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> instance used for distinguishing values.</param>
        public SortedMultiSet(IComparer<T>? comparer = null, IEqualityComparer<T>? equalityComparer = null) : base(comparer ?? Comparer<T>.Default)
        {
            this.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        bool ICollection<T>.IsReadOnly => false;

        /// <inheritdoc/>
        public new int Count => Values.Sum(s => s.Count);

        int ICollection<T>.Count => Count;

        int IReadOnlyCollection<T>.Count => Count;

        /// <inheritdoc/>
        public void Add(T item)
        {
            if(TryGetValue(item, out var set))
            {
                set.Add(item);
            }else{
                this[item] = new(equalityComparer)
                {
                    item
                };
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return TryGetValue(item, out var set) && set.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach(var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        /// <inheritdoc/>
        public new bool Remove(T item)
        {
            if(TryGetValue(item, out var set))
            {
                if(set.Remove(item))
                {
                    if(set.Count == 0)
                    {
                        Remove(item);
                    }
                    return true;
                }
            }
            return false;
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
        }

        /// <inheritdoc/>
        public new IEnumerator<T> GetEnumerator()
        {
            return Values.SelectMany(s => s).GetEnumerator();
        }
    }
}
