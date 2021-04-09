using IS4.MultiArchiver.Vocabulary;
using System;
using System.Runtime.Serialization;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNode : IEquatable<ILinkedNode>
    {
        void Set(Classes @class);
        void Set<T>(IUriFormatter<T> formatter, T value);
        void Set(Properties property, Individuals value);
        void Set(Properties property, string value);
        void Set(Properties property, string value, Datatypes datatype);
        void Set(Properties property, string value, string language);
        void Set(Properties property, Vocabularies vocabulary, string localName);
        void Set<T>(Properties property, IUriFormatter<T> formatter, T value);
        void Set(Properties property, Uri value);
        void Set(Properties property, ILinkedNode value);
        void Set<T>(Properties property, T value) where T : struct, IEquatable<T>, IFormattable, ISerializable;

        ILinkedNode this[string subName] { get; }
    }

    public abstract class LinkedNode<TNode> : ILinkedNode where TNode : class, IEquatable<TNode>
    {
        readonly IVocabularyCache<TNode> cache;
        protected TNode Subject { get; }

        public LinkedNode(TNode subject, IVocabularyCache<TNode> cache)
        {
            Subject = subject;
            this.cache = cache;
        }

        protected abstract void HandleTriple(TNode subj, TNode pred, TNode obj);

        protected abstract TNode CreateNode(Uri uri);
        protected abstract TNode CreateNode(string value);
        protected abstract TNode CreateNode(string value, TNode datatype);
        protected abstract TNode CreateNode(string value, string language);
        protected abstract TNode CreateNode<T>(T value) where T : struct, IEquatable<T>, IFormattable, ISerializable;

        protected abstract Uri GetUri(TNode node);
        protected abstract LinkedNode<TNode> CreateNew(TNode subject);

        public void Set(Classes @class)
        {
            HandleTriple(Subject, cache[Properties.Type], cache[@class]);
        }

        public void Set<T>(IUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, cache[Properties.Type], CreateNode(formatter.FormatUri(value)));
        }

        public void Set(Properties property, Individuals value)
        {
            HandleTriple(Subject, cache[property], cache[value]);
        }

        public void Set(Properties property, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, cache[property], CreateNode(value));
        }

        public void Set(Properties property, string value, Datatypes datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, cache[property], CreateNode(value, cache[datatype]));
        }

        public void Set(Properties property, string value, string language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, cache[property], CreateNode(value, language));
        }

        public void Set(Properties property, Vocabularies vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, cache[property], CreateNode(new Uri(GetUri(cache[vocabulary]).AbsoluteUri + "/" + localName, UriKind.Absolute)));
        }

        public void Set<T>(Properties property, IUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, cache[property], CreateNode(formatter.FormatUri(value)));
        }

        public void Set(Properties property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, cache[property], node.Subject);
        }

        public void Set(Properties property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyURI);
        }

        public void Set<T>(Properties property, T value) where T : struct, IEquatable<T>, IFormattable, ISerializable
        {
            HandleTriple(Subject, cache[property], CreateNode(value));
        }

        public LinkedNode<TNode> this[string subName] {
            get{
                if(subName == null) throw new ArgumentNullException(nameof(subName));
                return CreateNew(CreateNode(GetUri(Subject).AbsoluteUri + "/" + subName));
            }
        }

        ILinkedNode ILinkedNode.this[string subName] => this[subName];

        public bool Equals(ILinkedNode other)
        {
            return other is LinkedNode<TNode> node && Subject.Equals(node.Subject);
        }

        public override bool Equals(object obj)
        {
            return obj is LinkedNode<TNode> node && Subject.Equals(node.Subject);
        }

        public override int GetHashCode()
        {
            return Subject.GetHashCode();
        }
    }
}
