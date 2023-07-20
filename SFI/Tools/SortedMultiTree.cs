using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace IS4.SFI.Tools
{
    /// <summary>
    /// Provides the basis for a tree-like data structure that sorts nodes according to a partial
    /// ordering.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the tree.</typeparam>
    public abstract class SortedMultiTree<TValue> : IProducerConsumerCollection<TValue>, ICollection<TValue>
    {
        /// <inheritdoc/>
        public bool IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => throw new NotSupportedException();

        /// <inheritdoc/>
        public abstract int Count { get; }

        /// <inheritdoc/>
        public abstract void Add(TValue item);

        /// <inheritdoc/>
        public abstract void Clear();

        /// <inheritdoc/>
        public abstract bool Contains(TValue item);

        /// <inheritdoc/>
        public void CopyTo(TValue[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <inheritdoc/>
        public abstract void CopyTo(Array array, int index);

        /// <inheritdoc/>
        public abstract IEnumerator<TValue> GetEnumerator();

        /// <inheritdoc/>
        public abstract bool Remove(TValue item);

        /// <inheritdoc/>
        public abstract TValue[] ToArray();

        /// <inheritdoc/>
        public abstract bool TryAdd(TValue item);

        /// <inheritdoc/>
        public abstract bool TryTake(out TValue item);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Provides a tree-like data structure that sorts nodes according to a partial
    /// ordering based on a collection of aspects, allowing traversal from minimum to maximum values.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the tree.</typeparam>
    /// <typeparam name="TAspect">The type of the aspects that determine the ordering of nodes.</typeparam>
    public class SortedMultiTree<TValue, TAspect> : SortedMultiTree<TValue>, IProducerConsumerCollection<KeyValuePair<TAspect, TValue>>, ICollection<KeyValuePair<TAspect, TValue>>
    {
        readonly Func<TValue, IEnumerable<TAspect>> aspectsProvider;
        readonly IComparer<TValue> valueComparer;
        readonly IComparer<TAspect> aspectComparer;
        readonly IEqualityComparer<TValue> valueEqualityComparer;
        readonly IEqualityComparer<TAspect> aspectEqualityComparer;
        readonly ConcurrentDictionary<TAspect, ConcurrentDictionary<TValue, SortedMultiTree<TValue, TAspect>>> nodes;

        /// <summary>
        /// The value of this node.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// The ordering aspect of this node.
        /// </summary>
        public TAspect Aspect { get; }

        /// <summary>
        /// The key-value pair of this node.
        /// </summary>
        public KeyValuePair<TAspect, TValue> NodeKey => new(Aspect, Value);

        private IEnumerable<TValue> AllValues => nodes.Values.SelectMany(aspect => aspect.Values).SelectMany(t => t.AllValues).Concat(new[] { Value });

        private IEnumerable<KeyValuePair<TAspect, TValue>> AllPairs => nodes.Values.SelectMany(aspect => aspect.Values).SelectMany(t => t.AllPairs).Concat(new[] { NodeKey });

        private IEnumerable<SortedMultiTree<TValue, TAspect>> AllTrees => nodes.Values.SelectMany(aspect => aspect.Values).Concat(new[] { this });

        private PairEqualityComparer equalityComparer => new(aspectEqualityComparer, valueEqualityComparer);

        /// <inheritdoc/>
        public override int Count => AllValues.Distinct(valueEqualityComparer).Count();

        int ICollection<KeyValuePair<TAspect, TValue>>.Count => AllPairs.Distinct(equalityComparer).Count();

        /// <summary>
        /// Creates a new value of the tree.
        /// </summary>
        /// <param name="rootValue">The value of <see cref="Value"/>.</param>
        /// <param name="rootAspect">The value of <see cref="Aspect"/>.</param>
        /// <param name="aspectsProvider">
        /// The function used to retrieve the collection of aspects of a value.
        /// </param>
        /// <param name="valueComparer">
        /// The <see cref="IComparer{T}"/> instance used to determine
        /// the placement of newly added items based in their value
        /// in case the aspects are ambiguous.
        /// </param>
        /// <param name="aspectComparer">
        /// The <see cref="IComparer{T}"/> instance used to determine
        /// the placement of newly added items based on their aspect.
        /// </param>
        /// <param name="valueEqualityComparer">
        /// The <see cref="IEqualityComparer{T}"/> instance used to determine
        /// the presence of values. If <see langword="null"/>,
        /// <see cref="EqualityComparer{T}.Default"/> is used.
        /// </param>
        /// <param name="aspectEqualityComparer">
        /// The <see cref="IEqualityComparer{T}"/> instance used to determine
        /// the presence of aspects. If <see langword="null"/>,
        /// <see cref="EqualityComparer{T}.Default"/> is used.
        /// </param>
        public SortedMultiTree(TValue rootValue, TAspect rootAspect, Func<TValue, IEnumerable<TAspect>> aspectsProvider, IComparer<TValue> valueComparer, IComparer<TAspect> aspectComparer, IEqualityComparer<TValue>? valueEqualityComparer = null, IEqualityComparer<TAspect>? aspectEqualityComparer = null)
        {
            Value = rootValue;
            Aspect = rootAspect;
            this.aspectsProvider = aspectsProvider;
            this.valueComparer = valueComparer;
            this.aspectComparer = aspectComparer;
            this.valueEqualityComparer = valueEqualityComparer ?? EqualityComparer<TValue>.Default;
            this.aspectEqualityComparer = aspectEqualityComparer ?? EqualityComparer<TAspect>.Default;
            nodes = new(this.aspectEqualityComparer);
        }

        /// <inheritdoc/>
        public override void Add(TValue item)
        {
            if(!TryAdd(item))
            {
                throw AddFailed(item, nameof(item));
            }
        }

        /// <inheritdoc/>
        public override bool TryAdd(TValue item)
        {
            bool added = false;
            foreach(var key in GetAspects(item))
            {
                if(TryAdd(new KeyValuePair<TAspect, TValue>(key, item)))
                {
                    added = true;
                }
            }
            return added;
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<TAspect, TValue> item)
        {
            if(!TryAdd(item))
            {
                throw AddFailed(item.Value, nameof(item));
            }
        }

        private Exception AddFailed(TValue item, string argName)
        {
            if(valueEqualityComparer.Equals(Value, item))
            {
                return new ArgumentException("The argument must precede this node's aspect.", argName);
            }
            // Currently unused
            return new ArgumentException("The argument cannot be stored in the tree as it would lead to ambiguous ordering.", argName);
        }

        private bool Precedes(KeyValuePair<TAspect, TValue> item)
        {
            int aspectCompare = aspectComparer.Compare(Aspect, item.Key);
            return aspectCompare < 0 || (aspectCompare <= 0 && valueComparer.Compare(Value, item.Value) < 0);
        }

        private bool PrecededBy(KeyValuePair<TAspect, TValue> item)
        {
            int aspectCompare = aspectComparer.Compare(item.Key, Aspect);
            return aspectCompare < 0 || (aspectCompare <= 0 && valueComparer.Compare(item.Value, Value) < 0);
        }

        private SortedMultiTree<TValue, TAspect> NewTree(KeyValuePair<TAspect, TValue> item)
        {
            return new(item.Value, item.Key, aspectsProvider, valueComparer, aspectComparer, valueEqualityComparer, aspectEqualityComparer);
        }

        /// <inheritdoc/>
        public bool TryAdd(KeyValuePair<TAspect, TValue> item)
        {
            if(IdEquals(item))
            {
                // Adding the current node
                return true;
            }
            if(!Precedes(item))
            {
                // Value must strictly precede item
                return false;
            }
            bool added = false;
            if(nodes.TryGetValue(item.Key, out var keyCollection))
            {
                var byAspect = keyCollection;
                // There are values for this aspect already
                if(byAspect.ContainsKey(item.Value))
                {
                    // The value is already among them
                    return true;
                }
                // We are adding different values for the same aspect,
                // but it may still fit due to value ordering
                List<TValue>? newSubtrees = null;
                List<KeyValuePair<TAspect, TValue>>? itemsToCopy = null;
                foreach(var pair in byAspect)
                {
                    // Try add recursively
                    if(pair.Value.TryAdd(item))
                    {
                        added = true;
                        continue;
                    }
                    if(pair.Value.PrecededBy(item))
                    {
                        // item should precede this node
                        (newSubtrees ??= new()).Add(pair.Key);
                        continue;
                    }
                    // The tree cannot be moved directly, but some values might be duplicated
                    foreach(var tree in pair.Value.AllTrees)
                    {
                        if(tree.PrecededBy(item))
                        {
                            (itemsToCopy ??= new()).Add(tree.NodeKey);
                        }
                    }
                }
                if(!added || newSubtrees != null || itemsToCopy != null)
                {
                    // None of the subnodes can lead to the item,
                    // or some values are reachable through this value
                    var tree = byAspect[item.Value] = NewTree(item);

                    if(newSubtrees != null)
                    {
                        // All aspects are the same here
                        var byAspect2 = tree.nodes[item.Key] = new(valueEqualityComparer);
                        foreach(var subtreeValue in newSubtrees)
                        {
                            // Move more specific subtrees to this one
                            if(byAspect.TryRemove(subtreeValue, out var subtree))
                            {
                                byAspect2[subtreeValue] = subtree;
                            }
                        }
                    }

                    if(itemsToCopy != null)
                    {
                        foreach(var pair in itemsToCopy)
                        {
                            tree.TryAdd(pair);
                        }
                    }
                }
                return true;
            }else{
                // Adding a new aspect; let's check if it fits somewhere
                // or covers other subtrees
                List<KeyValuePair<ConcurrentDictionary<TValue, SortedMultiTree<TValue, TAspect>>, TValue>>? newSubtrees = null;
                List<KeyValuePair<TAspect, TValue>>? itemsToCopy = null;
                foreach(var aspectPair in nodes)
                {
                    var aspect = aspectPair.Key;
                    var byAspect = aspectPair.Value;
                    foreach(var pair in byAspect)
                    {
                        // Try add recursively
                        if(pair.Value.TryAdd(item))
                        {
                            added = true;
                            continue;
                        }
                        if(pair.Value.PrecededBy(item))
                        {
                            // item should precede this node
                            (newSubtrees ??= new()).Add(new(byAspect, pair.Key));
                            continue;
                        }
                        // The tree cannot be moved directly, but some values might be duplicated
                        foreach(var tree in pair.Value.AllTrees)
                        {
                            if(tree.PrecededBy(item))
                            {
                                (itemsToCopy ??= new()).Add(tree.NodeKey);
                            }
                        }
                    }
                }
                if(!added || newSubtrees != null || itemsToCopy != null)
                {
                    // None of the subnodes can lead to the item,
                    // or some values are reachable through this value
                    var byAspect = nodes[item.Key] = new(valueEqualityComparer);
                    var tree = byAspect[item.Value] = NewTree(item);

                    if(newSubtrees != null)
                    {
                        foreach(var subtreePair in newSubtrees)
                        {
                            // Move more specific subtrees to this one
                            if(subtreePair.Key.TryRemove(subtreePair.Value, out var subtree))
                            {
                                // This subtree likely has a different (more specific) aspect
                                var aspect = subtree.Aspect;

                                if(!tree.nodes.TryGetValue(aspect, out var byAspect2))
                                {
                                    byAspect2 = tree.nodes[aspect] = new(valueEqualityComparer);
                                }

                                byAspect2[subtreePair.Value] = subtree;
                            }
                        }
                    }

                    if(itemsToCopy != null)
                    {
                        foreach(var pair in itemsToCopy)
                        {
                            tree.TryAdd(pair);
                        }
                    }
                }
                return true;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{TextTools.GetUserFriendlyName(Aspect)}: {TextTools.GetUserFriendlyName(Value)} ({AllTrees.Count()})";
        }

        /// <summary>
        /// Looks for all values matching a particular aspect.
        /// </summary>
        /// <param name="aspect">The aspect the values must provide.</param>
        /// <returns>
        /// The collection of all values with <paramref name="aspect"/>,
        /// ordered from the most specific ones.
        /// </returns>
        public IEnumerable<TValue> Find(TAspect aspect)
        {
            return Find(aspect, 0).OrderByDescending(t => t.level).Select(t => t.value);
        }

        private IEnumerable<(int level, TValue value)> Find(TAspect aspect, int level)
        {
            if(nodes.TryGetValue(aspect, out var byAspect))
            {
                // this aspect leads to a single subtree
                foreach(var result in Find(aspect, byAspect.Values, level + 1))
                {
                    yield return result;
                }
            }else{
                // this aspect is more specific than any single subtree
                foreach(var pair in nodes)
                {
                    if(aspectComparer.Compare(pair.Key, aspect) < 0)
                    {
                        // the aspect can be found in this subtree
                        foreach(var result in Find(aspect, pair.Value.Values, level + 1))
                        {
                            yield return result;
                        }
                    }
                }
            }
            yield return (level, Value);
        }

        private IEnumerable<(int level, TValue value)> Find(TAspect aspect, ICollection<SortedMultiTree<TValue, TAspect>> trees, int level)
        {
            foreach(var tree in trees)
            {
                foreach(var result in tree.Find(aspect, level))
                {
                    yield return result;
                }
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            nodes.Clear();
        }

        /// <inheritdoc/>
        public override bool Contains(TValue item)
        {
            return valueEqualityComparer.Equals(Value, item) || nodes.Any(n => n.Value.Any(m => m.Value.Contains(item)));
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TAspect, TValue> item)
        {
            return IdEquals(item) || nodes.Any(n => n.Value.Any(m => m.Value.Contains(item)));
        }

        bool IdEquals(KeyValuePair<TAspect, TValue> item)
        {
            return aspectEqualityComparer.Equals(item.Key, Aspect) && valueEqualityComparer.Equals(item.Value, Value);
        }

        /// <inheritdoc/>
        public override void CopyTo(Array array, int arrayIndex)
        {
            switch(array)
            {
                case TValue[] values:
                    foreach(var item in AllValues.Distinct(valueEqualityComparer))
                    {
                        values[arrayIndex++] = item;
                    }
                    break;
                case KeyValuePair<TAspect, TValue>[] pairs:
                    foreach(var item in AllPairs.Distinct(equalityComparer))
                    {
                        pairs[arrayIndex++] = item;
                    }
                    break;
                default:
                    foreach(var item in this)
                    {
                        array.SetValue(item, arrayIndex++);
                    }
                    break;

            }
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TAspect, TValue>[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <inheritdoc/>
        public override bool Remove(TValue item)
        {
            if(valueEqualityComparer.Equals(Value, item))
            {
                return false;
            }
            bool removed = false;
            foreach(var byAspect in nodes.Values)
            {
                var matching = byAspect.Keys.Where(key => valueEqualityComparer.Equals(item, key)).ToList();
                foreach(var key in matching)
                {
                    if(byAspect.TryRemove(key, out var tree))
                    {
                        // Directly removed, but inner nodes need to be reincluded
                        foreach(var inner in tree)
                        {
                            if(!valueEqualityComparer.Equals(inner, item))
                            {
                                Add(inner);
                            }
                        }
                        removed = true;
                    }
                }
            }
            // Remove from nested trees
            foreach(var tree in nodes.Values.SelectMany(byAspect => byAspect.Values))
            {
                if(tree.Remove(item))
                {
                    removed = true;
                }
            }
            return removed;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TAspect, TValue> item)
        {
            if(IdEquals(item))
            {
                throw new ArgumentException("The root node cannot be removed.", nameof(item));
            }
            if(nodes.TryGetValue(item.Key, out var byAspect))
            {
                bool removed = false;
                if(byAspect.TryRemove(item.Value, out var existingTree))
                {
                    // Directly removed, but inner node need to be reincluded
                    foreach(var inner in existingTree)
                    {
                        Add(inner);
                    }
                    removed = true;
                }
                // Remove from nested trees
                foreach(var tree in nodes.Values.SelectMany(byAspect => byAspect.Values))
                {
                    if(tree.Remove(item))
                    {
                        removed = true;
                    }
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the collection of keys associated with <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to retrieve the keys from.</param>
        /// <returns>A collection of keys that affect ordering of <paramref name="item"/>.</returns>
        private IEnumerable<TAspect> GetAspects(TValue item)
        {
            return aspectsProvider(item);
        }

        /// <inheritdoc/>
        public override IEnumerator<TValue> GetEnumerator()
        {
            return AllValues.Distinct(valueEqualityComparer).GetEnumerator();
        }

        IEnumerator<KeyValuePair<TAspect, TValue>> IEnumerable<KeyValuePair<TAspect, TValue>>.GetEnumerator()
        {
            return AllPairs.Distinct(equalityComparer).GetEnumerator();
        }

        /// <inheritdoc/>
        public override TValue[] ToArray()
        {
            var arr = new TValue[Count];
            CopyTo(arr, 0);
            return arr;
        }

        KeyValuePair<TAspect, TValue>[] IProducerConsumerCollection<KeyValuePair<TAspect, TValue>>.ToArray()
        {
            var arr = new KeyValuePair<TAspect, TValue>[Count];
            CopyTo(arr, 0);
            return arr;
        }

#pragma warning disable CS8765 // item is unspecified when false
        /// <inheritdoc/>
        public override bool TryTake([MaybeNullWhen(false)] out TValue item)
#pragma warning restore CS8765
        {
            foreach(var byAspect in nodes.Values)
            {
                foreach(var key in byAspect.Keys)
                {
                    if(byAspect.TryRemove(key, out _))
                    {
                        item = key;
                        return true;
                    }
                }
            }
            item = default;
            return false;
        }

        bool IProducerConsumerCollection<KeyValuePair<TAspect, TValue>>.TryTake(out KeyValuePair<TAspect, TValue> item)
        {
            foreach(var aspectPair in nodes)
            {
                var byAspect = aspectPair.Value;
                foreach(var key in byAspect.Keys)
                {
                    if(byAspect.TryRemove(key, out _))
                    {
                        item = new(aspectPair.Key, key);
                        return true;
                    }
                }
            }
            item = default;
            return false;
        }

        class PairEqualityComparer : IEqualityComparer<KeyValuePair<TAspect, TValue>>
        {
            public IEqualityComparer<TAspect> Key { get; }
            public IEqualityComparer<TValue> Value { get; }

            public PairEqualityComparer(IEqualityComparer<TAspect> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Key = keyComparer;
                Value = valueComparer;
            }

            public bool Equals(KeyValuePair<TAspect, TValue> x, KeyValuePair<TAspect, TValue> y)
            {
                return Key.Equals(x.Key, y.Key) && Value.Equals(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<TAspect, TValue> obj)
            {
                return HashCode.Combine(Key.GetHashCode(obj.Key), Value.GetHashCode(obj.Value));
            }
        }
    }
}
