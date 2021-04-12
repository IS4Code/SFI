using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class FormatAnalyzer<T> : IEntityAnalyzer<IFormatObject<T>> where T : class
    {
        readonly IEnumerable<Classes> recognizedClasses;

        public FormatAnalyzer(IEnumerable<Classes> recognizedClasses)
        {
            this.recognizedClasses = recognizedClasses;
        }

        public FormatAnalyzer(params Classes[] recognizedClasses) : this((IEnumerable<Classes>)recognizedClasses)
        {

        }

        protected virtual ILinkedNode CreateNode(ILinkedNode parent, IFormatObject<T> format)
        {
            return parent[format.MediaType];
        }

        public virtual void Analyze(ILinkedNode node, T entity, ILinkedNodeFactory nodeFactory)
        {

        }

        public ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T> entity, ILinkedNodeFactory nodeFactory)
        {
            var node = CreateNode(parent, entity);

            if(node != null)
            {
                Analyze(node, entity.Value, nodeFactory);
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
