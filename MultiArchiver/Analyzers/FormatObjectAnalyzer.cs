using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Types;
using IS4.MultiArchiver.Vocabulary;
using System;

namespace IS4.MultiArchiver.Analyzers
{
    public sealed class FormatObjectAnalyzer : IEntityAnalyzer<FormatObject>
    {
        public FormatObjectAnalyzer()
        {

        }

        public IRdfEntity Analyze(FormatObject entity, IRdfAnalyzer analyzer)
        {
            var node = analyzer.TryAnalyze(entity.Value) ?? analyzer.CreateUriNode(new Uri(Vocabularies.ArchiveId + Guid.NewGuid().ToString("D")));

            node.Set(Classes.MediaObject);

            node.Set(Properties.EncodingFormat, entity.Format.MediaType);
            node.Set(Properties.EncodingFormat, new Uri(Vocabularies.MediaTypes + entity.Format.MediaType));

            return node;
        }
    }
}
