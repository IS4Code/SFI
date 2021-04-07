using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Linq;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class RdfBase
    {
        private readonly INodeFactory nodeFactory;

        public RdfBase(INodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        private IUriNode CreateNode<T>(T name, bool lowerCase, ConcurrentDictionary<T, IUriNode> cache) where T : struct
        {
            return cache.GetOrAdd(name, n => {
                var enumType = typeof(T);
                var fieldName = enumType.GetEnumName(n);
                var field = enumType.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var uriAttribute = field.GetCustomAttributes(typeof(UriAttribute), false).OfType<UriAttribute>().FirstOrDefault();
                if(uriAttribute == null)
                {
                    throw new ArgumentException(null, nameof(name));
                }
                var localName = uriAttribute.LocalName ?? (lowerCase ? fieldName.Substring(0, 1).ToLowerInvariant() + fieldName.Substring(1) : fieldName);
                return nodeFactory.CreateUriNode(new Uri(uriAttribute.Vocabulary + localName, UriKind.Absolute));
            });
        }

        protected ILiteralNode this[DateTime dateTime] => dateTime.ToLiteral(nodeFactory, true);
        protected ILiteralNode this[string literal] => nodeFactory.CreateLiteralNode(literal);
        protected ILiteralNode this[string literal, Datatypes datatype] => nodeFactory.CreateLiteralNode(literal, this[datatype].Uri);
        protected IUriNode this[Uri name] => nodeFactory.CreateUriNode(name);
        protected IUriNode this[Classes name] => CreateNode(name, false, classCache);
        protected IUriNode this[Properties name] => CreateNode(name, true, propertyCache);
        protected IUriNode this[Individuals name] => CreateNode(name, true, individualCache);
        protected IUriNode this[Datatypes name] => CreateNode(name, true, datatypeCache);

        private readonly ConcurrentDictionary<Classes, IUriNode> classCache = new ConcurrentDictionary<Classes, IUriNode>();
        private readonly ConcurrentDictionary<Properties, IUriNode> propertyCache = new ConcurrentDictionary<Properties, IUriNode>();
        private readonly ConcurrentDictionary<Individuals, IUriNode> individualCache = new ConcurrentDictionary<Individuals, IUriNode>();
        private readonly ConcurrentDictionary<Datatypes, IUriNode> datatypeCache = new ConcurrentDictionary<Datatypes, IUriNode>();
    }
}
