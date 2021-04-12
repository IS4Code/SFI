using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class FormatAnalyzer<T> : IEntityAnalyzer<T>, IEntityAnalyzer<IFormatObject<T>> where T : class
    {
        readonly IEnumerable<Classes> recognizedClasses;

        public FormatAnalyzer(IEnumerable<Classes> recognizedClasses)
        {
            this.recognizedClasses = recognizedClasses;
        }

        public FormatAnalyzer(params Classes[] recognizedClasses) : this((IEnumerable<Classes>)recognizedClasses)
        {

        }

        public virtual ILinkedNode Analyze(ILinkedNode parent, T entity, ILinkedNodeFactory nodeFactory)
        {
            return parent;
        }

        public ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T> entity, ILinkedNodeFactory nodeFactory)
        {
            var node = Analyze(parent[entity.MediaType], entity.Value, nodeFactory);

            if(node != null)
            {
                node.SetClass(Classes.MediaObject);
                foreach(var cls in recognizedClasses)
                {
                    node.SetClass(cls);
                }
                node.Set(Properties.EncodingFormat, Vocabularies.Mime, Uri.EscapeUriString(entity.MediaType));
            }

            return node;
        }
    }
}
