namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer<in T> where T : class
    {
        IRdfEntity Analyze(T entity, IRdfAnalyzer analyzer);
    }
}
