using BencodeNET.Objects;
using IS4.MultiArchiver.Extensions;
using IS4.MultiArchiver.Services;
using System;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var archiver = Archiver.CreateDefault();
            archiver.BitTorrentHash.InfoCreated += delegate(BDictionary info, byte[] hash)
            {
                var dict = new BDictionary();
                dict["info"] = info;
                using(var stream = File.Create($@"torrent\{BitConverter.ToString(hash)}.torrent"))
                {
                    dict.EncodeTo(stream);
                }
            };
            //archiver.Archive(@"G:\ISO\Broodwar.iso", "graph.ttl");
            archiver.Archive("image.zip", "graph.ttl");
            //Console.ReadKey(true);
        }
    }

    class TestAnalyzer : IEntityAnalyzer<object>
    {
        public ILinkedNode Analyze(ILinkedNode parent, object entity, ILinkedNodeFactory nodeFactory)
        {
            throw new System.NotImplementedException();
        }
    }
}
