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

        public ILinkedNode Analyze(FormatObject entity, ILinkedNodeFactory nodeFactory)
        {
            var node = nodeFactory.TryCreate(entity.Value) ?? nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.Set(Classes.MediaObject);

            if(entity.MediaType is string type)
            {
                node.Set(Properties.EncodingFormat, type);
                node.Set(Properties.EncodingFormat, Vocabularies.MediaTypes, type);
            }

            return node;
        }
    }
}
