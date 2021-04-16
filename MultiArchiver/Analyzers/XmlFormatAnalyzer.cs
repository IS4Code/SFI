using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlFormatAnalyzer<T> : FormatObjectAnalyzer<T, IXmlDocumentFormat>
    {
        public XmlFormatAnalyzer(IEnumerable<Classes> recognizedClasses) : base(recognizedClasses)
        {

        }

        public XmlFormatAnalyzer(params Classes[] recognizedClasses) : base(recognizedClasses)
        {

        }

        public sealed override ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T, IXmlDocumentFormat> entity, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(parent, entity, nodeFactory);

            if(node != null)
            {
                string pubId;
                Uri ns;
                if(entity.Format is IXmlDocumentFormat<T> fmt)
                {
                    pubId = fmt.GetPublicId(entity.Value);
                    ns = fmt.GetNamespace(entity.Value);
                }else{
                    pubId = entity.Format.GetPublicId(entity.Value);
                    ns = entity.Format.GetNamespace(entity.Value);
                }

                if(pubId != null)
                {
                    node.Set(Properties.EncodingFormat, UriTools.PublicIdFormatter.Instance, pubId);
                }
            }

            return node;
        }
    }
}
