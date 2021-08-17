using IS4.MultiArchiver.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Vocabulary
{
    public sealed class VocabularyCache<TNode> : IVocabularyCache<TNode> where TNode : class
    {
        readonly Func<Uri, TNode> nodeFactory;

        public TNode this[ClassUri name] => CreateNode(name, classCache);
        public TNode this[PropertyUri name] => CreateNode(name, propertyCache);
        public TNode this[IndividualUri name] => CreateNode(name, individualCache);
        public TNode this[DatatypeUri name] => CreateNode(name, datatypeCache);
        public TNode this[VocabularyUri name] => nodeFactory(new Uri(name.Value));

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

        TNode CreateNode<T>(T name, ConcurrentDictionary<T, TNode> cache) where T : struct, ITermUri
        {
            if(vocabularies.Add(name.Vocabulary))
            {
                VocabularyAdded?.Invoke(name.Vocabulary);
            }
            return cache.GetOrAdd(name, n => {
                return nodeFactory(new Uri(name.Value, UriKind.Absolute));
            });
        }
    }
}
