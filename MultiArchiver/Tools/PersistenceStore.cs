using IS4.MultiArchiver.Services;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace IS4.MultiArchiver.Tools
{
    public class PersistenceStore<TKey, TValue> where TKey : IPersistentKey
    {
        static readonly object NullPlaceholder = new object();

        readonly ConditionalWeakTable<object, ConcurrentDictionary<object, Lazy<TValue>>> data = new ConditionalWeakTable<object, ConcurrentDictionary<object, Lazy<TValue>>>();
        readonly Func<TKey, TValue> factory;

        public PersistenceStore(Func<TKey, TValue> factory)
        {
            this.factory = factory;
        }

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
