using IS4.MultiArchiver.Vocabulary;

namespace IS4.MultiArchiver.Services
{
    public interface IVocabularyCache<out TNode> where TNode : class
    {
        TNode this[Classes name] { get; }
        TNode this[Properties name] { get; }
        TNode this[Individuals name] { get; }
        TNode this[Datatypes name] { get; }
        TNode this[Vocabularies name] { get; }
    }
}
