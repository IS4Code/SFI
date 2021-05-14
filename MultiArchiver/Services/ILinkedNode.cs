using IS4.MultiArchiver.Vocabulary;
using System;
using System.Globalization;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNode : IEquatable<ILinkedNode>
    {
        void SetClass(Classes @class);
        void SetClass<T>(IClassUriFormatter<T> formatter, T value);

        void Set(Properties property, Individuals value);
        void Set(Properties property, string value);
        void Set(Properties property, string value, Datatypes datatype);
        void Set<TData>(Properties property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);
        void Set<TValue>(Properties property, TValue value, Datatypes datatype) where TValue : IFormattable;
        void Set<TValue, TData>(Properties property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;
        void Set(Properties property, string value, string language);
        void Set(Properties property, Vocabularies vocabulary, string localName);
        void Set<TValue>(Properties property, IIndividualUriFormatter<TValue> formatter, TValue value);
        void Set(Properties property, Uri value);
        void Set(Properties property, ILinkedNode value);
        void Set(Properties property, bool value);
        void Set<TValue>(Properties property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Individuals value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, Datatypes datatype);
        void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, Datatypes datatype) where TValue : IFormattable;
        void Set<TProp, TValue, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, string language);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Vocabularies vocabulary, string localName);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, bool value);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        ILinkedNode this[string subName] { get; }
        ILinkedNode this[IIndividualUriFormatter<Uri> subFormatter] { get; }
    }

    public abstract class LinkedNode<TNode> : ILinkedNode where TNode : class, IEquatable<TNode>
    {
        protected IVocabularyCache<TNode> Cache { get; }
        protected TNode Subject { get; }

        public LinkedNode(TNode subject, IVocabularyCache<TNode> cache)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Cache = cache;
        }

        protected abstract void HandleTriple(TNode subj, TNode pred, TNode obj);

        protected abstract TNode CreateNode(Uri uri);
        protected abstract TNode CreateNode(string value);
        protected abstract TNode CreateNode(string value, TNode datatype);
        protected abstract TNode CreateNode(string value, string language);
        protected abstract TNode CreateNode(bool value);
        protected abstract TNode CreateNode<T>(T value) where T : struct, IEquatable<T>, IFormattable;

        protected abstract Uri GetUri(TNode node);
        protected abstract LinkedNode<TNode> CreateNew(TNode subject);

        private TNode CreateNode<T>(IUriFormatter<T> formatter, T value)
        {
            return CreateNode(formatter.FormatUri(value));
        }

        public void SetClass(Classes @class)
        {
            HandleTriple(Subject, Cache[Properties.Type], Cache[@class]);
        }

        public void SetClass<T>(IClassUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[Properties.Type], CreateNode(formatter, value));
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

        public void Set<TData>(Properties property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set<TValue>(Properties property, TValue value, Datatypes datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]));
        }

        public void Set<TValue, TData>(Properties property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), CreateNode(datatypeFormatter, datatypeValue)));
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
            HandleTriple(Subject, Cache[property], CreateNode(new Uri(GetUri(Cache[vocabulary]).AbsoluteUri + localName, UriKind.Absolute)));
        }

        public void Set<T>(Properties property, IIndividualUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(formatter, value));
        }

        public void Set(Properties property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, Cache[property], node.Subject);
        }

        public void Set(Properties property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        public void Set(Properties property, bool value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set<TValue>(Properties property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Individuals value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), Cache[value]);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, Datatypes datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, Cache[datatype]));
        }

        public void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, Datatypes datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]));
        }

        public void Set<TProp, TValue, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value.ToString(null, CultureInfo.InvariantCulture), CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, string language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, language));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Vocabularies vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(new Uri(GetUri(Cache[vocabulary]).AbsoluteUri + localName, UriKind.Absolute)));
        }

        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(formatter, value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), node.Subject);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value)
        {
            Set(propertyFormatter, propertyValue, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, bool value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        public LinkedNode<TNode> this[string subName] {
            get{
                if(subName == null) throw new ArgumentNullException(nameof(subName));
                var uri = GetUri(Subject);
                return CreateNew(CreateNode(new Uri(uri.AbsoluteUri + (subName.StartsWith("#") ? "" : String.IsNullOrEmpty(uri.Authority + uri.Fragment) ? "#" : "/") + subName)));
            }
        }

        ILinkedNode ILinkedNode.this[string subName] => this[subName];

        public LinkedNode<TNode> this[IIndividualUriFormatter<Uri> subFormatter] {
            get{
                if(subFormatter == null) throw new ArgumentNullException(nameof(subFormatter));
                return CreateNew(CreateNode(subFormatter, GetUri(Subject)));
            }
        }

        ILinkedNode ILinkedNode.this[IIndividualUriFormatter<Uri> subFormatter] => this[subFormatter];

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

        public override string ToString()
        {
            return Subject.ToString();
        }
    }
}
