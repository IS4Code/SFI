using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// A mock implementation of <see cref="ILinkedNode"/> and <see cref="ILinkedNodeFactory"/>,
    /// storing all assigned properties in <see cref="Properties"/> and exposing their values
    /// via <see cref="this[PropertyUri]"/>.
    /// </summary>
    public class StorageLinkedNode : LinkedNode<EquatableUri, EquatableUri, UriCache>, ILinkedNodeFactory
    {
        /// <summary>
        /// <see langword="true"/> if <see cref="SetAsBase"/> has been called.
        /// </summary>
        public bool IsBase { get; private set; }

        /// <inheritdoc cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Subject"/>
        public new Uri Subject => base.Subject;

        /// <inheritdoc cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Graph"/>
        public new Uri Graph => base.Graph;

        /// <summary>
        /// A collection of all properties assigned via methods like <see cref="ILinkedNode.Set{TValue}(PropertyUri, TValue)"/>.
        /// </summary>
        public ConcurrentDictionary<EquatableUri, HashSet<EquatableUri>> Properties { get; } = new ConcurrentDictionary<EquatableUri, HashSet<EquatableUri>>();

        /// <summary>
        /// Obtains a value or collection of values from <see cref="Properties"/>
        /// based on a vocabulary property.
        /// </summary>
        /// <param name="property">
        /// The property to obtain, turned to a URI via
        /// the local <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Cache"/>.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Uri"/> if the value is a single node,
        /// or a previously assigned object of arbitrary type, <see langword="null"/> if there is
        /// no value of the property, or <see cref="IEnumerable{T}"/> of <see cref="object"/>
        /// if there are multiple distinct values.
        /// </returns>
        public object? this[PropertyUri property] {
            get {
                var key = Cache[property];
                if(Properties.TryGetValue(key, out var set))
                {
                    if(set.Count > 1)
                    {
                        return set.Select(GetValue);
                    }
                    return GetValue(set.FirstOrDefault());
                }
                return null;

                static object? GetValue(EquatableUri? uri)
                {
                    if(uri is ValueUri valueUri)
                    {
                        return valueUri.Value;
                    }
                    return uri;
                }
            }
        }

        IIndividualUriFormatter<string> ILinkedNodeFactory.Root { get; } = new UriTools.PrefixFormatter<string>("x.blank:");

        /// <summary>
        /// Creates a new node, from a supplied subject URI, graph URI, and the vocabulary cache instance to use.
        /// </summary>
        /// <param name="subject">The value of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Subject"/>.</param>
        /// <param name="graph">The value of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Graph"/>.</param>
        /// <param name="cache">The value of <see cref="LinkedNode{TNode, TGraphNode, TVocabularyCache}.Cache"/>.</param>
        public StorageLinkedNode(EquatableUri subject, EquatableUri graph, UriCache cache) : base(subject, graph, cache)
        {

        }

        /// <inheritdoc/>
        public override void SetAsBase()
        {
            IsBase = true;
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateGraphNode(Uri uri)
        {
            return EquatableUri.Create(uri);
        }

        /// <inheritdoc/>
        protected override LinkedNode<EquatableUri, EquatableUri, UriCache>? CreateInGraph(EquatableUri? graph)
        {
            if(graph == null) return null;
            return new StorageLinkedNode(base.Subject, graph, Cache);
        }

        /// <inheritdoc/>
        protected override LinkedNode<EquatableUri, EquatableUri, UriCache> CreateNew(EquatableUri subject)
        {
            return new StorageLinkedNode(subject, base.Graph, Cache);
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode(Uri uri)
        {
            return EquatableUri.Create(uri);
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode(string value)
        {
            return new ValueUri(value);
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode(string value, EquatableUri datatype)
        {
            return new ValueUri((value, datatype));
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode(string value, string language)
        {
            return new ValueUri((value, language));
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode(bool value)
        {
            return new ValueUri(value);
        }

        /// <inheritdoc/>
        protected override EquatableUri CreateNode<T>(T value)
        {
            return new ValueUri(value);
        }

        /// <inheritdoc/>
        protected override Uri GetUri(EquatableUri node)
        {
            return node;
        }

        /// <inheritdoc/>
        protected override void HandleTriple(EquatableUri? subj, EquatableUri? pred, EquatableUri? obj)
        {
            if(subj == null || pred == null || obj == null)
            {
                return;
            }
            if(base.Subject.Equals(subj))
            {
                if(!Properties.TryGetValue(pred, out var set))
                {
                    Properties[pred] = set = new HashSet<EquatableUri>(EquatableUri.Comparer);
                }
                set.Add(obj);
            }
        }

        /// <inheritdoc/>
        public override void Describe(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
        {

        }

        /// <inheritdoc/>
        public override void Describe(XmlDocument rdfXmlDocument, IReadOnlyCollection<Uri>? subjectUris = null)
        {

        }

        /// <inheritdoc/>
        public override ValueTask DescribeAsync(XmlReader rdfXmlReader, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            return default;
        }

        /// <inheritdoc/>
        public override void Describe(Func<Uri, XmlReader?> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null)
        {

        }

        /// <inheritdoc/>
        public override ValueTask DescribeAsync(Func<Uri, ValueTask<XmlReader?>> rdfXmlReaderFactory, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            return default;
        }

        /// <inheritdoc/>
        public override void Describe(Func<Uri, XmlDocument?> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null)
        {

        }

        /// <inheritdoc/>
        public override ValueTask DescribeAsync(Func<Uri, ValueTask<XmlDocument?>> rdfXmlDocumentFactory, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            return default;
        }

        /// <inheritdoc/>
        public override bool TryDescribe(object loader, Func<Uri, object?> dataSource, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            return false;
        }

        /// <inheritdoc/>
        public override ValueTask<bool> TryDescribeAsync(object loader, Func<Uri, ValueTask<object?>> dataSource, IReadOnlyCollection<Uri>? subjectUris = null)
        {
            return new ValueTask<bool>(false);
        }

        /// <inheritdoc/>
        public override IEnumerable<INodeMatchProperties> Match()
        {
            return Array.Empty<INodeMatchProperties>();
        }

        ILinkedNode ILinkedNodeFactory.Create<T>(IIndividualUriFormatter<T> formatter, T value)
        {
            return new StorageLinkedNode(EquatableUri.Create(formatter[value] ?? throw new NotImplementedException()), base.Graph, Cache);
        }

        bool ILinkedNodeFactory.IsSafeLiteral(string str)
        {
            return true;
        }

        bool ILinkedNodeFactory.IsSafePredicate(Uri uri)
        {
            return true;
        }
    }
}
