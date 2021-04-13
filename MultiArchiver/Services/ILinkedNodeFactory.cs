using IS4.MultiArchiver.Vocabulary;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNodeFactory
    {
        ILinkedNode Root { get; }
        ILinkedNode Create(Vocabularies vocabulary, string localName);
        ILinkedNode Create<T>(IUriFormatter<T> formatter, T value);
        ILinkedNode Create<T>(ILinkedNode parent, T entity) where T : class;
    }
}
