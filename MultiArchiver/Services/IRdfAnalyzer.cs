using System;

namespace IS4.MultiArchiver.Services
{
    public interface IRdfAnalyzer
    {
        IRdfEntity CreateUriNode(Uri uri);
        IRdfEntity Analyze<T>(T entity) where T : class;
    }
}
