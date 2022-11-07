using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    /// <summary>
    /// Provides caching of vocabulary terms.
    /// </summary>
    public interface IVocabularyCache
    {
        /// <summary>
        /// Stores a collection of used vocabularies
        /// as instances of <see cref="VocabularyUri"/>.
        /// </summary>
        ICollection<VocabularyUri> Vocabularies { get; }
    }

    /// <summary>
    /// Provides caching of objects of type <typeparamref name="TNode"/>
    /// based on specific vocabulary terms represented by <typeparamref name="TTerm"/>.
    /// </summary>
    /// <typeparam name="TTerm">
    /// The type of terms this cache supports, implementing <see cref="ITermUri"/>.
    /// </typeparam>
    /// <typeparam name="TNode">
    /// The node type cached by the object.
    /// </typeparam>
    public interface IVocabularyCache<in TTerm, out TNode> : IVocabularyCache
        where TTerm : ITermUri, IEquatable<TTerm>
    {
        /// <summary>
        /// Obtains (or produces) the cached node for the term identified
        /// by <paramref name="term"/>.
        /// </summary>
        /// <param name="term">The term to obtain the node from.</param>
        /// <returns>A node that represents an instance of the term.</returns>
        TNode this[TTerm term] { get; }
    }
}
