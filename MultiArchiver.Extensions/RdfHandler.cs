using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using VDS.RDF;

namespace IS4.MultiArchiver.Extensions
{
    public class RdfHandler : ILinkedNodeFactory
    {
        readonly IRdfHandler handler;
        readonly IEntityAnalyzer baseAnalyzer;
        readonly VocabularyCache<IUriNode> cache;

        public IReadOnlyDictionary<Vocabularies, string> Vocabularies => cache.Vocabularies;

        public ILinkedNode Root { get; }

        public RdfHandler(Uri root, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            this.handler = handler;
            this.baseAnalyzer = baseAnalyzer;
            cache = new VocabularyCache<IUriNode>(handler.CreateUriNode);
            Root = Create(IdentityUriFormatter.Instance, root);
        }

        public ILinkedNode Create<T>(IUriFormatter<T> formatter, T value)
        {
            return new UriNode(handler.CreateUriNode(formatter.FormatUri(value)), handler, cache);
        }

        public ILinkedNode Create<T>(T entity) where T : class
        {
            return baseAnalyzer.Analyze(entity, this);
        }

        class UriNode : LinkedNode<INode>
        {
            readonly IRdfHandler handler;

            public UriNode(INode subject, IRdfHandler handler, IVocabularyCache<INode> cache) : base(subject, cache)
            {
                if(!(subject is IUriNode)) throw new ArgumentException(null, nameof(subject));
                this.handler = handler;
            }

            protected override void HandleTriple(INode subj, INode pred, INode obj)
            {
                handler.HandleTriple(new Triple(subj, pred, obj));
            }

            protected override INode CreateNode(Uri uri)
            {
                return handler.CreateUriNode(uri);
            }

            protected override INode CreateNode(string value)
            {
                return handler.CreateLiteralNode(value);
            }

            protected override INode CreateNode(string value, INode datatype)
            {
                return handler.CreateLiteralNode(value, GetUri(datatype));
            }

            protected override INode CreateNode(string value, string language)
            {
                return handler.CreateLiteralNode(value, language);
            }

            protected override INode CreateNode<T>(T value)
            {
                try{
                    return LiteralExtensions.ToLiteral((dynamic)value, handler);
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
                return new UriNode(subject, handler, Cache);
            }
        }
    }
}
