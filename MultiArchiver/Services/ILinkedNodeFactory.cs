using System;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNodeFactory
    {
        IIndividualUriFormatter<string> Root { get; }
        ILinkedNode Create<T>(IIndividualUriFormatter<T> formatter, T value);
        bool IsSafeString(string str);
    }

    public static class LinkedNodeFactoryExtensions
    {
        public static ILinkedNode NewGuidNode(this ILinkedNodeFactory factory)
        {
            return factory.Create(factory.Root, Guid.NewGuid().ToString("D"));
        }
    }
}
