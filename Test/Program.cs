using MultiArchiverExtensions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var archive = new Archiver();
            archive.Archive("xml2rdf.csproj", "graph.ttl");
        }
    }
}
