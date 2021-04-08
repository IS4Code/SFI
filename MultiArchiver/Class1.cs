using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class Class1
    {
        readonly IReadOnlyCollection<object> analyzers;

        public Class1()
        {
            var hash = new BuiltInHash(MD5.Create, Individuals.MD5, "urn:md5:");

            var formats = new List<IFileFormat>
            {
                new XmlFileFormat()
            };

            var graph = new Graph();
            analyzers = new ConcurrentBag<object>()
            {
                new FileAnalyzer(),
                new DataAnalyzer(hash, formats),
                new FormatObjectAnalyzer(),
                new XmlAnalyzer()
            };

            //string path = "image.zip";
            string path = "graph.ttl";

            var handler = new VDS.RDF.Parsing.Handlers.GraphHandler(graph);
            handler.StartRdf();
            try{
                new FileAnalyzer().Analyze(new FileInfo(path), new TripleHandler(this, handler));
            }finally{
                handler.EndRdf(true);
            }
            graph.SaveToFile("graph.ttl");
        }

        public IRdfEntity CreateUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        IRdfEntity Analyze<T>(T entity, TripleHandler handler) where T : class
        {
            if(entity == null) return null;
            foreach(var obj in analyzers)
            {
                if(obj is IEntityAnalyzer<T> analyzer)
                {
                    var result = analyzer.Analyze(entity, handler);
                    if(result != null) return result;
                }
            }
            return null;
        }

        class TripleHandler : IRdfAnalyzer
        {
            readonly Class1 parent;
            readonly IRdfHandler handler;

            public TripleHandler(Class1 parent, IRdfHandler handler)
            {
                this.parent = parent;
                this.handler = handler;
            }

            public IRdfEntity CreateUriNode(Uri uri)
            {
                return new UriNode(this, handler.CreateUriNode(uri));
            }

            public IRdfEntity Analyze<T>(T entity) where T : class
            {
                return parent.Analyze(entity, this);
            }

            ILiteralNode this[DateTime dateTime] => dateTime.ToLiteral(handler, true);
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

            class UriNode : IRdfEntity
            {
                readonly TripleHandler parent;
                readonly INode subject;

                public UriNode(TripleHandler parent, INode subject)
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

                public void Set(Properties property, IRdfEntity entity)
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
}
