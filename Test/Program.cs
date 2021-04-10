using IS4.MultiArchiver.Extensions;
using IS4.MultiArchiver.Services;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var archiver = Archiver.CreateDefault();
            archiver.Archive("DSC00014.zip", "graph.ttl");
            //archiver.Archive("image.png", "graph.ttl");
        }
    }

    class TestAnalyzer : IEntityAnalyzer<object>
    {
        public ILinkedNode Analyze(object entity, ILinkedNodeFactory nodeFactory)
        {
            throw new System.NotImplementedException();
        }
    }
}
