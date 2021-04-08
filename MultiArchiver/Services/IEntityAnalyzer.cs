namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer
    {
        ILinkedNode Analyze<T>(T entity, ILinkedNodeFactory nodeFactory) where T : class;
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        ILinkedNode Analyze(T entity, ILinkedNodeFactory nodeFactory);
    }
}
