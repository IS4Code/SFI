using IS4.MultiArchiver.Vocabulary;
using System;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNodeFactory
    {
        ILinkedNode Root { get; }
        ILinkedNode Create(Vocabularies vocabulary, string localName);
        ILinkedNode Create<T>(IUriFormatter<T> formatter, T value);
        ILinkedNode Create<T>(ILinkedNode parent, T entity) where T : class;
        bool IsSafeString(string str);
    }

    public static class LinkedNodeFactoryExtensions
    {
        public static ILinkedNode NewGuidNode(this ILinkedNodeFactory factory)
        {
            var node = factory.Root[Guid.NewGuid().ToString("D")];
            if(node != null)
            {
                node.SetClass(Classes.Root);
            }
            return node;
        }
    }
}
