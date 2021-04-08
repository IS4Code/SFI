using System;
using System.Collections.Concurrent;
using System.Linq;

namespace IS4.MultiArchiver.Vocabulary
{
    public sealed class VocabularyCache<TNode> where TNode : class
    {
        readonly Func<Uri, TNode> nodeFactory;

        public TNode this[Classes name] => CreateNode(name, false, classCache);
        public TNode this[Properties name] => CreateNode(name, true, propertyCache);
        public TNode this[Individuals name] => CreateNode(name, true, individualCache);
        public TNode this[Datatypes name] => CreateNode(name, true, datatypeCache);

        readonly ConcurrentDictionary<Classes, TNode> classCache = new ConcurrentDictionary<Classes, TNode>();
        readonly ConcurrentDictionary<Properties, TNode> propertyCache = new ConcurrentDictionary<Properties, TNode>();
        readonly ConcurrentDictionary<Individuals, TNode> individualCache = new ConcurrentDictionary<Individuals, TNode>();
        readonly ConcurrentDictionary<Datatypes, TNode> datatypeCache = new ConcurrentDictionary<Datatypes, TNode>();

        public VocabularyCache(Func<Uri, TNode> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        TNode CreateNode<T>(T name, bool lowerCase, ConcurrentDictionary<T, TNode> cache) where T : struct
        {
            return cache.GetOrAdd(name, n => {
                var enumType = typeof(T);
                var fieldName = enumType.GetEnumName(n);
                var field = enumType.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if(field == null)
                {
                    throw new ArgumentException(null, nameof(name));
                }
                var uriAttribute = field.GetCustomAttributes(typeof(UriAttribute), false).OfType<UriAttribute>().FirstOrDefault();
                if(uriAttribute == null)
                {
                    throw new ArgumentException(null, nameof(name));
                }
                var localName = uriAttribute.LocalName ?? (lowerCase ? fieldName.Substring(0, 1).ToLowerInvariant() + fieldName.Substring(1) : fieldName);
                return nodeFactory(new Uri(uriAttribute.Vocabulary + localName, UriKind.Absolute));
            });
        }
    }
}
