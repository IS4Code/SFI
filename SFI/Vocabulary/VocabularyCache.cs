using IS4.SFI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// The implementation of <see cref="IVocabularyCache{TTerm, TNode}"/> for instances
    /// of <see cref="ClassUri"/>, <see cref="PropertyUri"/>,
    /// <see cref="IndividualUri"/>, and <see cref="DatatypeUri"/>.
    /// </summary>
    /// <typeparam name="TNode">The cached node corresponding to the terms.</typeparam>
    public class VocabularyCache<TNode> :
        IVocabularyCache<ClassUri, TNode>, IVocabularyCache<PropertyUri, TNode>,
        IVocabularyCache<IndividualUri, TNode>, IVocabularyCache<DatatypeUri, TNode>
    {
        readonly Func<Uri, TNode> nodeFactory;

        /// <inheritdoc/>
        public TNode this[ClassUri name] => CreateNode(name, classCache, nodeFactory);

        /// <inheritdoc/>
        public TNode this[PropertyUri name] => CreateNode(name, propertyCache, nodeFactory);

        /// <inheritdoc/>
        public TNode this[IndividualUri name] => CreateNode(name, individualCache, nodeFactory);

        /// <inheritdoc/>
        public TNode this[DatatypeUri name] => CreateNode(name, datatypeCache, nodeFactory);
        
        readonly ConcurrentDictionary<VocabularyUri, bool> vocabularies = new();

        /// <inheritdoc/>
        public ICollection<VocabularyUri> Vocabularies => vocabularies.Keys;

        readonly ConcurrentDictionary<ClassUri, TNode> classCache = new();
        readonly ConcurrentDictionary<PropertyUri, TNode> propertyCache = new();
        readonly ConcurrentDictionary<IndividualUri, TNode> individualCache = new();
        readonly ConcurrentDictionary<DatatypeUri, TNode> datatypeCache = new();
        
        /// <summary>
        /// Fired when a new vocabulary is used.
        /// </summary>
        public event Action<VocabularyUri>? VocabularyAdded;

        /// <summary>
        /// Creates a new instance of the cache from a factory function.
        /// </summary>
        /// <param name="nodeFactory">
        /// A function called on the URI of used vocabulary terms,
        /// producing the cached value.
        /// </param>
        public VocabularyCache(Func<Uri, TNode> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        /// <inheritdoc/>
        protected TValue CreateNode<TKey, TValue>(TKey name, ConcurrentDictionary<TKey, TValue> cache, Func<Uri, TValue> factory) where TKey : struct, ITermUri
        {
            if(vocabularies.TryAdd(name.Vocabulary, false))
            {
                VocabularyAdded?.Invoke(name.Vocabulary);
            }
            return cache.GetOrAdd(name, n => {
                return factory(new Uri(name.Value, UriKind.Absolute));
            });
        }
    }
}
