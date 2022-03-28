using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    public interface IVocabularyCache
    {
        IReadOnlyCollection<VocabularyUri> Vocabularies { get; }
    }

    public interface IVocabularyCache<in TTerm, out TNode> : IVocabularyCache
        where TTerm : ITermUri, IEquatable<TTerm>
    {
        TNode this[TTerm name] { get; }
    }
}
