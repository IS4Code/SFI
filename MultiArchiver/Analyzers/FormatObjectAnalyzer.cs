using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Types;
using IS4.MultiArchiver.Vocabulary;
using System;
using VDS.RDF;

namespace IS4.MultiArchiver.Analyzers
{
    public class FormatObjectAnalyzer : RdfBase, IEntityAnalyzer<FormatObject>
    {
        public FormatObjectAnalyzer(INodeFactory nodeFactory) : base(nodeFactory)
        {

        }

        public IUriNode Analyze(FormatObject entity, IRdfHandler handler, IEntityAnalyzer baseAnalyzer)
        {
            var node = baseAnalyzer.TryAnalyze(entity.Value, handler) ?? handler.CreateUriNode(new Uri(Vocabularies.ArchiveId + Guid.NewGuid().ToString("D")));

            handler.HandleTriple(node, this[Properties.Type], this[Classes.MediaObject]);

            handler.HandleTriple(node, this[Properties.EncodingFormat], this[entity.Format.MediaType]);
            handler.HandleTriple(node, this[Properties.EncodingFormat], this[new Uri(Vocabularies.MediaTypes + entity.Format.MediaType)]);

            return node;
        }
    }
}
