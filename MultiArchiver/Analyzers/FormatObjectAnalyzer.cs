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
            if(!entity.Successful) return null;
            var node = nodeFactory.TryCreate(entity.Value) ?? nodeFactory.Root[Guid.NewGuid().ToString("D")];

            node.SetClass(Classes.MediaObject);

            if(entity.MediaType is string type)
            {
                node.Set(Properties.EncodingFormat, Vocabularies.Mime, Uri.EscapeUriString(type));
            }

            return node;
        }
    }
}
