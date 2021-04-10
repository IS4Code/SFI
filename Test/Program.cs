using IS4.MultiArchiver.Extensions;
using IS4.MultiArchiver.Services;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var archiver = Archiver.CreateDefault();
            archiver.Archive(@"G:\ISO\Broodwar.iso", "graph.ttl");
            //archiver.Archive("image.png", "graph.ttl");
            //Console.ReadKey(true);
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
