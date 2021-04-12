namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer
    {
        ILinkedNode Analyze<T>(ILinkedNode parent, T entity, ILinkedNodeFactory nodeFactory) where T : class;
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        ILinkedNode Analyze(ILinkedNode parent, T entity, ILinkedNodeFactory nodeFactory);
    }
}
