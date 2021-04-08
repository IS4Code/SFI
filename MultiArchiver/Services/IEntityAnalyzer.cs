namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer<in T> where T : class
    {
        ILinkedNode Analyze(T entity, ILinkedNodeFactory analyzer);
    }
}
