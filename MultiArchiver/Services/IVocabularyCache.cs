using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Services
{
    public interface IVocabularyCache<out TNode, out TGraphNode> where TNode : class where TGraphNode : class
    {
        TNode this[ClassUri name] { get; }
        TNode this[PropertyUri name] { get; }
        TNode this[IndividualUri name] { get; }
        TNode this[DatatypeUri name] { get; }
        TGraphNode this[GraphUri name] { get; }
        IReadOnlyCollection<VocabularyUri> Vocabularies { get; }
    }
}
