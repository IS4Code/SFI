using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Collections.Concurrent;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// A mock implementation of <see cref="VocabularyCache{TNode}"/>,
    /// storing the cached URIs directly as nodes.
    /// </summary>
    public class UriCache : VocabularyCache<EquatableUri>, IVocabularyCache<GraphUri, EquatableUri>
    {
        readonly ConcurrentDictionary<GraphUri, EquatableUri> graphCache = new();

        /// <summary>
        /// Creates a new instance of the cache.
        /// </summary>
        public UriCache() : base(EquatableUri.Create)
        {

        }

        /// <inheritdoc/>
        public EquatableUri this[GraphUri term] => CreateNode(term, graphCache, EquatableUri.Create);
    }
}
