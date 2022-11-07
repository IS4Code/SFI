using IS4.MultiArchiver.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
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

        public TNode this[ClassUri name] => CreateNode(name, classCache, nodeFactory);
        public TNode this[PropertyUri name] => CreateNode(name, propertyCache, nodeFactory);
        public TNode this[IndividualUri name] => CreateNode(name, individualCache, nodeFactory);
        public TNode this[DatatypeUri name] => CreateNode(name, datatypeCache, nodeFactory);
        
        readonly ConcurrentDictionary<VocabularyUri, bool> vocabularies = new ConcurrentDictionary<VocabularyUri, bool>();

        public ICollection<VocabularyUri> Vocabularies => vocabularies.Keys;

        readonly ConcurrentDictionary<ClassUri, TNode> classCache = new ConcurrentDictionary<ClassUri, TNode>();
        readonly ConcurrentDictionary<PropertyUri, TNode> propertyCache = new ConcurrentDictionary<PropertyUri, TNode>();
        readonly ConcurrentDictionary<IndividualUri, TNode> individualCache = new ConcurrentDictionary<IndividualUri, TNode>();
        readonly ConcurrentDictionary<DatatypeUri, TNode> datatypeCache = new ConcurrentDictionary<DatatypeUri, TNode>();
        
        /// <summary>
        /// Fired when a new vocabulary is used.
        /// </summary>
        public event Action<VocabularyUri> VocabularyAdded;

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

        TValue CreateNode<TKey, TValue>(TKey name, ConcurrentDictionary<TKey, TValue> cache, Func<Uri, TValue> factory) where TKey : struct, ITermUri
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
