using IS4.MultiArchiver.Formats;
using IS4.MultiArchiver.Extensions;
using IS4.MultiArchiver.Analyzers;
using IS4.MultiArchiver.Services;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var archiver = new Archiver();
            archiver.DataAnalyzer.Formats.Add(new ImageFormat());
            archiver.Analyzers.Add(new TestAnalyzer());
            archiver.Analyzers.Add(new ImageAnalyzer());
            archiver.Archive("DSC00014.JPG", "graph.ttl");
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
