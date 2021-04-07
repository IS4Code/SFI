using VDS.RDF;

namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer
    {
        IUriNode Analyze<T>(T entity, IRdfHandler handler) where T : class;
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        IUriNode Analyze(T entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer);
    }
}
