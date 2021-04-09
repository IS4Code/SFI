using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using VDS.RDF;

namespace IS4.MultiArchiver
{
    public class RdfHandler : ILinkedNodeFactory
    {
        readonly IRdfHandler handler;
        readonly IEntityAnalyzer baseAnalyzer;
        readonly VocabularyCache<IUriNode> cache;

        public ILinkedNode Root { get; }

        public RdfHandler(Uri root, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            this.handler = handler;
            this.baseAnalyzer = baseAnalyzer;
            cache = new VocabularyCache<IUriNode>(handler.CreateUriNode);
            Root = new UriNode(this, handler.CreateUriNode(root));
        }

        public ILinkedNode Create<T>(IUriFormatter<T> formatter, T value)
        {
            return new UriNode(this, handler.CreateUriNode(formatter.FormatUri(value)));
        }

        public ILinkedNode Create<T>(T entity) where T : class
        {
            return baseAnalyzer.Analyze(entity, this);
        }

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

        class UriNode : LinkedNode<INode>
        {
            readonly RdfHandler parent;

            public UriNode(RdfHandler parent, INode subject) : base(subject, parent.cache)
            {
                this.parent = parent;
            }

            protected override void HandleTriple(INode subj, INode pred, INode obj)
            {
                parent.handler.HandleTriple(new Triple(subj, pred, obj));
            }

            protected override INode CreateNode(Uri uri)
            {
                return parent.handler.CreateUriNode(uri);
            }

            protected override INode CreateNode(string value)
            {
                return parent.handler.CreateLiteralNode(value);
            }

            protected override INode CreateNode(string value, INode datatype)
            {
                return parent.handler.CreateLiteralNode(value, GetUri(datatype));
            }

            protected override INode CreateNode(string value, string language)
            {
                return parent.handler.CreateLiteralNode(value, language);
            }

            protected override INode CreateNode<T>(T value)
            {
                try{
                    return parent[(dynamic)value];
                }catch(RuntimeBinderException e)
                {
                    throw new ArgumentException(null, nameof(value), e);
                }
            }

            protected override Uri GetUri(INode node)
            {
                return ((IUriNode)node).Uri;
            }

            protected override LinkedNode<INode> CreateNew(INode subject)
            {
                return new UriNode(parent, subject);
            }
        }
    }
}
