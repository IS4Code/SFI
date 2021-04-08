using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Linq;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class RdfHandler : ILinkedNodeFactory
    {
        readonly IRdfHandler handler;
        readonly IEntityAnalyzer baseAnalyzer;

        public RdfHandler(Graph graph, IEntityAnalyzer baseAnalyzer) : this(new VDS.RDF.Parsing.Handlers.GraphHandler(graph), baseAnalyzer)
        {

        }

        public RdfHandler(IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            this.handler = handler;
            this.baseAnalyzer = baseAnalyzer;
        }

        public ILinkedNode Create(Uri uri)
        {
            return new UriNode(this, handler.CreateUriNode(uri));
        }

        public ILinkedNode Create<T>(T entity) where T : class
        {
            return baseAnalyzer.Analyze(entity, this);
        }

        ILiteralNode this[int value] => value.ToLiteral(handler);
        ILiteralNode this[DateTime value] => value.ToLiteral(handler, true);
        ILiteralNode this[string literal] => handler.CreateLiteralNode(literal);
        ILiteralNode this[string literal, Datatypes datatype] => handler.CreateLiteralNode(literal, this[datatype].Uri);

        IUriNode this[Uri name] => handler.CreateUriNode(name);
        IUriNode this[Classes name] => CreateNode(name, false, classCache);
        IUriNode this[Properties name] => CreateNode(name, true, propertyCache);
        IUriNode this[Individuals name] => CreateNode(name, true, individualCache);
        IUriNode this[Datatypes name] => CreateNode(name, true, datatypeCache);

        readonly ConcurrentDictionary<Classes, IUriNode> classCache = new ConcurrentDictionary<Classes, IUriNode>();
        readonly ConcurrentDictionary<Properties, IUriNode> propertyCache = new ConcurrentDictionary<Properties, IUriNode>();
        readonly ConcurrentDictionary<Individuals, IUriNode> individualCache = new ConcurrentDictionary<Individuals, IUriNode>();
        readonly ConcurrentDictionary<Datatypes, IUriNode> datatypeCache = new ConcurrentDictionary<Datatypes, IUriNode>();
            
        IUriNode CreateNode<T>(T name, bool lowerCase, ConcurrentDictionary<T, IUriNode> cache) where T : struct
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
                return handler.CreateUriNode(new Uri(uriAttribute.Vocabulary + localName, UriKind.Absolute));
            });
        }

        class UriNode : ILinkedNode
        {
            readonly RdfHandler parent;
            readonly INode subject;

            public UriNode(RdfHandler parent, INode subject)
            {
                this.parent = parent;
                this.subject = subject;
            }

            public void Set(Classes @class)
            {
                parent.handler.HandleTriple(subject, parent[Properties.Type], parent[@class]);
            }

            public void Set(Properties property, Individuals value)
            {
                parent.handler.HandleTriple(subject, parent[property], parent[value]);
            }

            public void Set(Properties property, string value)
            {
                parent.handler.HandleTriple(subject, parent[property], parent[value]);
            }

            public void Set(Properties property, string value, Datatypes datatype)
            {
                parent.handler.HandleTriple(subject, parent[property], parent[value, datatype]);
            }

            public void Set(Properties property, Services.ILinkedNode entity)
            {
                if(!(entity is UriNode node)) throw new ArgumentException(null, nameof(entity));
                parent.handler.HandleTriple(subject, parent[property], node.subject);
            }

            public void Set(Properties property, Uri value)
            {
                parent.handler.HandleTriple(subject, parent[property], parent[value]);
            }

            public void Set<T>(Properties property, T value) where T : struct
            {
                parent.handler.HandleTriple(subject, parent[property], (INode)parent[(dynamic)value]);
            }
        }
    }
}
