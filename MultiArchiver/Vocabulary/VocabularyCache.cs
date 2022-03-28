using IS4.MultiArchiver.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
{
    public class VocabularyCache<TNode> :
        IVocabularyCache<ClassUri, TNode>, IVocabularyCache<PropertyUri, TNode>,
        IVocabularyCache<IndividualUri, TNode>, IVocabularyCache<DatatypeUri, TNode>
    {
        readonly Func<Uri, TNode> nodeFactory;

        public TNode this[ClassUri name] => CreateNode(name, classCache, nodeFactory);
        public TNode this[PropertyUri name] => CreateNode(name, propertyCache, nodeFactory);
        public TNode this[IndividualUri name] => CreateNode(name, individualCache, nodeFactory);
        public TNode this[DatatypeUri name] => CreateNode(name, datatypeCache, nodeFactory);
        
        readonly HashSet<VocabularyUri> vocabularies = new HashSet<VocabularyUri>();

        public IReadOnlyCollection<VocabularyUri> Vocabularies => vocabularies;

        readonly ConcurrentDictionary<ClassUri, TNode> classCache = new ConcurrentDictionary<ClassUri, TNode>();
        readonly ConcurrentDictionary<PropertyUri, TNode> propertyCache = new ConcurrentDictionary<PropertyUri, TNode>();
        readonly ConcurrentDictionary<IndividualUri, TNode> individualCache = new ConcurrentDictionary<IndividualUri, TNode>();
        readonly ConcurrentDictionary<DatatypeUri, TNode> datatypeCache = new ConcurrentDictionary<DatatypeUri, TNode>();
        
        public event Action<VocabularyUri> VocabularyAdded;

        public VocabularyCache(Func<Uri, TNode> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        TValue CreateNode<TKey, TValue>(TKey name, ConcurrentDictionary<TKey, TValue> cache, Func<Uri, TValue> factory) where TKey : struct, ITermUri
        {
            if(vocabularies.Add(name.Vocabulary))
            {
                VocabularyAdded?.Invoke(name.Vocabulary);
            }
            return cache.GetOrAdd(name, n => {
                return factory(new Uri(name.Value, UriKind.Absolute));
            });
        }
    }
}
