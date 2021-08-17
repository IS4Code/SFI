using IS4.MultiArchiver.Tools;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Globalization;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNode : IEquatable<ILinkedNode>
    {
        string Scheme { get; }

        void SetClass(ClassUri @class);
        void SetClass<TClass>(IClassUriFormatter<TClass> formatter, TClass value);

        void Set(PropertyUri property, IndividualUri value);
        void Set(PropertyUri property, string value);
        void Set(PropertyUri property, string value, DatatypeUri datatype);
        void Set<TData>(PropertyUri property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);
        void Set<TValue>(PropertyUri property, TValue value, DatatypeUri datatype) where TValue : IFormattable;
        void Set<TValue, TData>(PropertyUri property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;
        void Set(PropertyUri property, string value, string language);
        void Set(PropertyUri property, VocabularyUri vocabulary, string localName);
        void Set<TValue>(PropertyUri property, IIndividualUriFormatter<TValue> formatter, TValue value);
        void Set(PropertyUri property, Uri value);
        void Set(PropertyUri property, ILinkedNode value);
        void Set(PropertyUri property, bool value);
        void Set<TValue>(PropertyUri property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IndividualUri value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, DatatypeUri datatype);
        void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, DatatypeUri datatype) where TValue : IFormattable;
        void Set<TProp, TValue, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable;
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, string language);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, VocabularyUri vocabulary, string localName);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, Uri value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value);
        void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, bool value);
        void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable;

        ILinkedNode this[string subName] { get; }
        ILinkedNode this[IIndividualUriFormatter<Uri> subFormatter] { get; }

        ILinkedNode In(GraphUri graph);
        ILinkedNode In<TGraph>(IGraphUriFormatter<TGraph> graphFormatter, TGraph value);
    }

    public abstract class LinkedNode<TNode, TGraphNode> : ILinkedNode, IEquatable<LinkedNode<TNode, TGraphNode>> where TNode : class, IEquatable<TNode> where TGraphNode : class
    {
        protected TNode Subject { get; }
        protected TGraphNode Graph { get; }
        protected IVocabularyCache<TNode, TGraphNode> Cache { get; }

        public LinkedNode(TNode subject, TGraphNode graph, IVocabularyCache<TNode, TGraphNode> cache)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Graph = graph;
            Cache = cache;
        }

        protected abstract void HandleTriple(TNode subj, TNode pred, TNode obj);

        protected abstract TNode CreateNode(Uri uri);
        protected abstract TNode CreateNode(string value);
        protected abstract TNode CreateNode(string value, TNode datatype);
        protected abstract TNode CreateNode(string value, string language);
        protected abstract TNode CreateNode(bool value);
        protected abstract TNode CreateNode<T>(T value) where T : struct, IEquatable<T>, IFormattable;

        protected abstract TGraphNode CreateGraphNode(Uri uri);

        protected abstract Uri GetUri(TNode node);
        protected abstract LinkedNode<TNode, TGraphNode> CreateNew(TNode subject, TGraphNode graph);

        private TNode CreateNode<T>(IUriFormatter<T> formatter, T value)
        {
            var uri = formatter.FormatUri(value);
            return uri != null ? CreateNode(uri) : null;
        }

        private TGraphNode CreateGraphNode<T>(IGraphUriFormatter<T> formatter, T value)
        {
            var uri = formatter.FormatUri(value);
            return uri != null ? CreateGraphNode(uri) : null;
        }

        public string Scheme => GetUri(Subject).Scheme;

        public void SetClass(ClassUri @class)
        {
            HandleTriple(Subject, Cache[Properties.Type], Cache[@class]);
        }

        public void SetClass<T>(IClassUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[Properties.Type], CreateNode(formatter, value));
        }

        public void Set(PropertyUri property, IndividualUri value)
        {
            HandleTriple(Subject, Cache[property], Cache[value]);
        }

        public void Set(PropertyUri property, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set(PropertyUri property, string value, DatatypeUri datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, Cache[datatype]));
        }

        public void Set<TData>(PropertyUri property, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value, CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set<TValue>(PropertyUri property, TValue value, DatatypeUri datatype) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), Cache[datatype]));
        }

        public void Set<TValue, TData>(PropertyUri property, TValue value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue) where TValue : IFormattable
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, Cache[property], CreateNode(value.ToString(null, CultureInfo.InvariantCulture), CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set(PropertyUri property, string value, string language)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(language == null) throw new ArgumentNullException(nameof(language));
            HandleTriple(Subject, Cache[property], CreateNode(value, language));
        }

        public void Set(PropertyUri property, VocabularyUri vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, Cache[property], CreateNode((Uri)new EncodedUri(vocabulary.Value + localName, UriKind.Absolute)));
        }

        public void Set<T>(PropertyUri property, IIndividualUriFormatter<T> formatter, T value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(formatter, value));
        }

        public void Set(PropertyUri property, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode, TGraphNode> node)) throw new ArgumentException(null, nameof(value));
            HandleTriple(Subject, Cache[property], node.Subject);
        }

        public void Set(PropertyUri property, Uri value)
        {
            Set(property, value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString, Datatypes.AnyUri);
        }

        public void Set(PropertyUri property, bool value)
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set<TValue>(PropertyUri property, TValue value) where TValue : struct, IEquatable<TValue>, IFormattable
        {
            HandleTriple(Subject, Cache[property], CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IndividualUri value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), Cache[value]);
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, DatatypeUri datatype)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, Cache[datatype]));
        }

        public void Set<TProp, TData>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, string value, IDatatypeUriFormatter<TData> datatypeFormatter, TData datatypeValue)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(value, CreateNode(datatypeFormatter, datatypeValue)));
        }

        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, TValue value, DatatypeUri datatype) where TValue : IFormattable
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

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, VocabularyUri vocabulary, string localName)
        {
            if(localName == null) throw new ArgumentNullException(nameof(localName));
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode((Uri)new EncodedUri(vocabulary.Value + localName, UriKind.Absolute)));
        }

        public void Set<TProp, TValue>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, IIndividualUriFormatter<TValue> formatter, TValue value)
        {
            HandleTriple(Subject, CreateNode(propertyFormatter, propertyValue), CreateNode(formatter, value));
        }

        public void Set<TProp>(IPropertyUriFormatter<TProp> propertyFormatter, TProp propertyValue, ILinkedNode value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!(value is LinkedNode<TNode, TGraphNode> node)) throw new ArgumentException(null, nameof(value));
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

        public LinkedNode<TNode, TGraphNode> this[string subName] {
            get{
                if(subName == null) throw new ArgumentNullException(nameof(subName));
                var uri = GetUri(Subject);
                return CreateNew(CreateNode((Uri)new EncodedUri(uri.AbsoluteUri + (subName.StartsWith("#") ? "" : String.IsNullOrEmpty(uri.Authority + uri.Fragment) ? "#" : "/") + subName)), Graph);
            }
        }

        ILinkedNode ILinkedNode.this[string subName] => this[subName];

        public LinkedNode<TNode, TGraphNode> this[IIndividualUriFormatter<Uri> subFormatter] {
            get{
                if(subFormatter == null) throw new ArgumentNullException(nameof(subFormatter));
                var node = CreateNode(subFormatter, GetUri(Subject));
                return node != null ? CreateNew(node, Graph) : null;
            }
        }

        ILinkedNode ILinkedNode.this[IIndividualUriFormatter<Uri> subFormatter] => this[subFormatter];

        public ILinkedNode In(GraphUri graph)
        {
            return CreateNew(Subject, Cache[graph]);
        }

        public ILinkedNode In<TGraph>(IGraphUriFormatter<TGraph> graphFormatter, TGraph value)
        {
            if(graphFormatter == null) throw new ArgumentNullException(nameof(graphFormatter));
            var node = CreateGraphNode(graphFormatter, value);
            return CreateNew(Subject, node ?? Graph);
        }

        public bool Equals(LinkedNode<TNode, TGraphNode> node)
        {
            return Subject.Equals(node.Subject) && Object.Equals(Graph, node.Graph);
        }

        public bool Equals(ILinkedNode other)
        {
            return other is LinkedNode<TNode, TGraphNode> node && Equals(node);
        }

        public override bool Equals(object obj)
        {
            return obj is LinkedNode<TNode, TGraphNode> node && Equals(node);
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
