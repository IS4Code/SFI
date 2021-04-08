using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Runtime.Serialization;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class RdfHandler : ILinkedNodeFactory
    {
        readonly IRdfHandler handler;
        readonly IEntityAnalyzer baseAnalyzer;
        readonly VocabularyCache<IUriNode> cache;

        public RdfHandler(IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            this.handler = handler;
            this.baseAnalyzer = baseAnalyzer;
            cache = new VocabularyCache<IUriNode>(handler.CreateUriNode);
        }

        public ILinkedNode Create(Uri uri)
        {
            return new UriNode(this, handler.CreateUriNode(uri));
        }

        public ILinkedNode Create<T>(T entity) where T : class
        {
            return baseAnalyzer.Analyze(entity, this);
        }

        ILiteralNode this[string literal] => handler.CreateLiteralNode(literal);
        ILiteralNode this[string literal, Datatypes datatype] => handler.CreateLiteralNode(literal, cache[datatype].Uri);
        ILiteralNode this[string literal, string language] => handler.CreateLiteralNode(literal, language);

        ILiteralNode this[bool value] => value.ToLiteral(handler);
        ILiteralNode this[sbyte value] => value.ToLiteral(handler);
        ILiteralNode this[byte value] => value.ToLiteral(handler);
        ILiteralNode this[short value] => value.ToLiteral(handler);
        ILiteralNode this[int value] => value.ToLiteral(handler);
        ILiteralNode this[long value] => value.ToLiteral(handler);
        ILiteralNode this[float value] => value.ToLiteral(handler);
        ILiteralNode this[double value] => value.ToLiteral(handler);
        ILiteralNode this[decimal value] => value.ToLiteral(handler);
        ILiteralNode this[TimeSpan value] => value.ToLiteral(handler);
        ILiteralNode this[DateTimeOffset value] => value.ToLiteral(handler);
        ILiteralNode this[DateTime value] => value.ToLiteral(handler, true);

        IUriNode this[Uri name] => handler.CreateUriNode(name);

        class UriNode : ILinkedNode
        {
            readonly RdfHandler parent;
            readonly INode subject;

            VocabularyCache<IUriNode> cache => parent.cache;

            public UriNode(RdfHandler parent, INode subject)
            {
                this.parent = parent;
                this.subject = subject;
            }

            private void HandleTriple(INode subj, INode pred, INode obj)
            {
                parent.handler.HandleTriple(new Triple(subj, pred, obj));
            }

            public void Set(Classes @class)
            {
                HandleTriple(subject, cache[Properties.Type], cache[@class]);
            }

            public void Set(Properties property, Individuals value)
            {
                HandleTriple(subject, cache[property], cache[value]);
            }

            public void Set(Properties property, string value)
            {
                HandleTriple(subject, cache[property], parent[value]);
            }

            public void Set(Properties property, string value, Datatypes datatype)
            {
                HandleTriple(subject, cache[property], parent[value, datatype]);
            }

            public void Set(Properties property, string value, string language)
            {
                HandleTriple(subject, cache[property], parent[value, language]);
            }

            public void Set(Properties property, ILinkedNode entity)
            {
                if(!(entity is UriNode node)) throw new ArgumentException(null, nameof(entity));
                HandleTriple(subject, cache[property], node.subject);
            }

            public void Set(Properties property, Uri value)
            {
                HandleTriple(subject, cache[property], parent[value]);
            }

            public void Set<T>(Properties property, T value) where T : struct, IEquatable<T>, IFormattable, ISerializable
            {
                INode obj;
                try{
                    obj = parent[(dynamic)value];
                }catch(RuntimeBinderException e)
                {
                    throw new ArgumentException(null, nameof(value), e);
                }
                HandleTriple(subject, cache[property], obj);
            }

            public bool Equals(ILinkedNode other)
            {
                return other is UriNode node && subject.Equals(node.subject);
            }

            public override bool Equals(object obj)
            {
                return obj is UriNode node && subject.Equals(node.subject);
            }

            public override int GetHashCode()
            {
                return subject.GetHashCode();
            }
        }
    }
}
