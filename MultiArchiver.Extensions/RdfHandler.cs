using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
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

        public ILinkedNode Create(Vocabularies vocabulary, string localName)
        {
            return new UriNode(handler.CreateUriNode(new Uri(cache[vocabulary] + localName, UriKind.Absolute)), handler, cache);
        }

        public ILinkedNode Create<T>(IUriFormatter<T> formatter, T value)
        {
            return new UriNode(handler.CreateUriNode(formatter.FormatUri(value)), handler, cache);
        }

        public ILinkedNode Create<T>(ILinkedNode parent, T entity) where T : class
        {
            if(entity is ILinkedNode node) return node;
            return baseAnalyzer.Analyze(parent, entity, this);
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
                try{
                    handler.HandleTriple(new Triple(subj, pred, obj));
                }catch{

                }
            }

            static readonly Regex unsafeCharacters = new Regex(@"^\p{M}|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x84\x86-\x9F]|(^|[^\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF]($|[^\uDC00-\uDFFF])", RegexOptions.Compiled | RegexOptions.Multiline);

            static bool IsSafeString(string str)
            {
                if(unsafeCharacters.IsMatch(str)) return false;
                var e = StringInfo.GetTextElementEnumerator(str);
                while(e.MoveNext())
                {
                    if(Char.GetUnicodeCategory(str, e.ElementIndex) == UnicodeCategory.OtherNotAssigned)
                    {
                        return false;
                    }
                }
                return true;
            }

            protected override INode CreateNode(Uri uri)
            {
                return handler.CreateUriNode(uri);
            }

            protected override INode CreateNode(string value)
            {
                if(IsSafeString(value)) return handler.CreateLiteralNode(value);
                return handler.CreateLiteralNode($@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)}}}", GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(string value, INode datatype)
            {
                var dt = GetUri(datatype);
                if(IsSafeString(value)) return handler.CreateLiteralNode(value, dt);
                return handler.CreateLiteralNode($@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)},""@type"":{HttpUtility.JavaScriptStringEncode(dt.IsAbsoluteUri ? dt.AbsoluteUri : dt.OriginalString, true)}}}", GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(string value, string language)
            {
                if(IsSafeString(value)) return handler.CreateLiteralNode(value, language);
                return handler.CreateLiteralNode($@"{{""@value"":{HttpUtility.JavaScriptStringEncode(value, true)},""@language"":{HttpUtility.JavaScriptStringEncode(language, true)}}}", GetUri(Cache[Datatypes.Json]));
            }

            protected override INode CreateNode(bool value)
            {
                return LiteralExtensions.ToLiteral(value, handler);
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
