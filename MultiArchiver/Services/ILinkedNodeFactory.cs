using System;

namespace IS4.MultiArchiver.Services
{
    public interface ILinkedNodeFactory
    {
        ILinkedNode Root { get; }
        ILinkedNode Create(Uri uri);
        ILinkedNode Create<T>(T entity) where T : class;
    }
}
