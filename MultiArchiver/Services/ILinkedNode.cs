using IS4.MultiArchiver.Vocabulary;
using System;
using System.Runtime.Serialization;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNode : IEquatable<ILinkedNode>
    {
        void SetClass(Classes @class);
        void SetClass<T>(IUriFormatter<T> formatter, T value);

        void Set(Properties property, Individuals value);
        void Set(Properties property, string value);
        void Set(Properties property, string value, Datatypes datatype);
        void Set(Properties property, string value, string language);
        void Set(Properties property, Vocabularies vocabulary, string localName);
        void Set<T>(Properties property, IUriFormatter<T> formatter, T value);
        void Set(Properties property, Uri value);
        void Set(Properties property, ILinkedNode value);
        void Set<T>(Properties property, T value) where T : struct, IEquatable<T>, IFormattable, ISerializable;

        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Individuals value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, Datatypes datatype);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, string language);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Vocabularies vocabulary, string localName);
        void Set<TProp, T>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IUriFormatter<T> formatter, T value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value);
        void Set<TProp, T>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, T value) where T : struct, IEquatable<T>, IFormattable;

        ILinkedNode this[string subName] { get; }
    }

    public abstract class LinkedNode<TNode> : ILinkedNode where TNode : class, IEquatable<TNode>
    {
        protected IVocabularyCache<TNode> Cache { get; }
        protected TNode Subject { get; }

        public LinkedNode(TNode subject, IVocabularyCache<TNode> cache)
        {
            Subject = subject;
            Cache = cache;
        }

        protected abstract void HandleTriple(TNode subj, TNode pred, TNode obj);

        protected abstract TNode CreateNode(Uri uri);
        protected abstract TNode CreateNode(string value);
        protected abstract TNode CreateNode(string value, TNode datatype);
        protected abstract TNode CreateNode(string value, string language);
        protected abstract TNode CreateNode<T>(T value) where T : struct, IEquatable<T>, IFormattable;

        protected abstract Uri GetUri(TNode node);
        protected abstract LinkedNode<TNode> CreateNew(TNode subject);

        public void SetClass(Classes @class)
        {
            HandleTriple(Subject, Cache[Properties.Type], Cache[@class]);
        }

        public void SetClass<T>(IUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[Properties.Type], CreateNode(formatter.FormatUri(value)));
        }

        public void Set(Properties property, Individuals value)
        {
            HandleTriple(Subject, Cache[property], Cache[value]);
        }

        public void Set(Properties property, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set(Properties property, string value, Datatypes datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, Cache[datatype]));
        }

        public void Set(Properties property, string value, string language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, Cache[property], CreateNode(value, language));
        }

        public void Set(Properties property, Vocabularies vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, Cache[property], CreateNode(new Uri(GetUri(Cache[vocabulary]).AbsoluteUri + "/" + localName, UriKind.Absolute)));
        }

        public void Set<T>(Properties property, IUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(formatter.FormatUri(value)));
        }

        public void Set(Properties property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, Cache[property], node.Subject);
        }

        public void Set(Properties property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyURI);
        }

        public void Set<T>(Properties property, T value) where T : struct, IEquatable<T>, IFormattable, ISerializable
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Individuals value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), Cache[value]);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, Datatypes datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(value, Cache[datatype]));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, string language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(value, language));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Vocabularies vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(new Uri(GetUri(Cache[vocabulary]).AbsoluteUri + "/" + localName, UriKind.Absolute)));
        }

        public void Set<TProp, T>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(formatter.FormatUri(value)));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), node.Subject);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value)
        {
            Set(propertyFormatter, propertyValue, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyURI);
        }

        public void Set<TProp, T>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, T value) where T : struct, IEquatable<T>, IFormattable
        {
            HandleTriple(Subject, CreateNode(propertyFormatter.FormatUri(propertyValue)), CreateNode(value));
        }

        public LinkedNode<TNode> this[string subName] {
            get{
                if(subName == null) throw new ArgumentNullException(nameof(subName));
                return CreateNew(CreateNode(new Uri(GetUri(Subject).AbsoluteUri + "/" + subName)));
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
