using IS4.SFI.Services;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Stores cached values pertaining to an instance of <see cref="IPersistentKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class PersistenceStore<TKey, TValue> where TKey : IPersistentKey
    {
        /// <summary>
        /// The identity of this object is used instead of a <see langword="null"/> value as the key.
        /// </summary>
        static readonly object NullPlaceholder = new object();

        readonly ConditionalWeakTable<object, ConcurrentDictionary<object, Lazy<TValue>>> data = new();
        readonly Func<TKey, TValue> factory;

        /// <summary>
        /// Creates a new instance of the store from a factory function.
        /// </summary>
        /// <param name="factory">
        /// A function providing the cached value from a key.
        /// It is called only once for a given key.
        /// </param>
        public PersistenceStore(Func<TKey, TValue> factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Accesses the stored value for a particular key.
        /// </summary>
        /// <param name="key">The key to lookup in the store.</param>
        /// <returns>The value corresponding to <paramref name="key"/>.</returns>
        public TValue this[TKey key] {
            get{
                var refKey = key.ReferenceKey ?? NullPlaceholder;
                var dataKey = key.DataKey ?? NullPlaceholder;
                return data.GetOrCreateValue(refKey).GetOrAdd(dataKey, _ => new Lazy<TValue>(() => factory(key))).Value;
            }
            set{
                var refKey = key.ReferenceKey ?? NullPlaceholder;
                var dataKey = key.DataKey ?? NullPlaceholder;
                var lazy = new Lazy<TValue>(() => value, false);
                _ = lazy.Value;
                data.GetOrCreateValue(refKey)[dataKey] = lazy;
            }
        }
    }
}
