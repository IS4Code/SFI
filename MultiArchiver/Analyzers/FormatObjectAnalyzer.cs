using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public abstract class FormatObjectAnalyzer<T, TFormat> : IEntityAnalyzer<IFormatObject<T, TFormat>> where T : class where TFormat : class, IFileFormat
    {
        readonly IEnumerable<Classes> recognizedClasses;

        public FormatObjectAnalyzer(IEnumerable<Classes> recognizedClasses)
        {
            this.recognizedClasses = recognizedClasses;
        }

        public FormatObjectAnalyzer(params Classes[] recognizedClasses) : this((IEnumerable<Classes>)recognizedClasses)
        {

        }

        public virtual string Analyze(ILinkedNode node, T entity, ILinkedNodeFactory nodeFactory)
        {
            return null;
        }

        public virtual string Analyze(ILinkedNode node, T entity, object source, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(node, entity, nodeFactory);
        }

        public virtual string Analyze(ILinkedNode parent, ILinkedNode node, T entity, object source, ILinkedNodeFactory nodeFactory)
        {
            return Analyze(node, entity, source, nodeFactory);
        }

        public virtual ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T, TFormat> format, ILinkedNodeFactory nodeFactory)
        {
            var node = parent[format];

            if(node != null)
            {
                format.Label = Analyze(parent, node, format.Value, format.Source, nodeFactory);
                node.SetClass(Classes.MediaObject);
                foreach(var cls in recognizedClasses)
                {
                    node.SetClass(cls);
                }
                var type = format.MediaType?.ToLowerInvariant();
                if(type != null)
                {
                    if(type.StartsWith("audio/", StringComparison.Ordinal))
                    {
                        node.SetClass(Classes.AudioObject);
                        node.SetClass(Classes.Audio);
                    }else if(type.StartsWith("video/", StringComparison.Ordinal))
                    {
                        node.SetClass(Classes.VideoObject);
                        node.SetClass(Classes.Video);
                    }else if(type.StartsWith("image/", StringComparison.Ordinal))
                    {
                        node.SetClass(Classes.ImageObject);
                        node.SetClass(Classes.Image);
                    }
                    node.Set(Properties.EncodingFormat, Vocabularies.Mime, Uri.EscapeUriString(type));
                }
            }

            return node;
        }
    }
}
