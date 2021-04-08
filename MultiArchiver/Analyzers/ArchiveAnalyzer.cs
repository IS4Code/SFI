using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.IO.Compression;
using VDS.RDF;

namespace IS4.MultiArchiver.Analyzers
{
    public class ArchiveAnalyzer : RdfBase, IEntityAnalyzer<ZipArchive>
    {
        public ArchiveAnalyzer(INodeFactory nodeFactory) : base(nodeFactory)
        {

        }

        public IUriNode Analyze(ZipArchive entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            var node = handler.CreateUriNode(new Uri(Vocabularies.ArchiveId + Guid.NewGuid().ToString("D")));

            handler.HandleTriple(node, this[Properties.Type], this[Classes.Archive]);

            return node;
        }
    }
}
