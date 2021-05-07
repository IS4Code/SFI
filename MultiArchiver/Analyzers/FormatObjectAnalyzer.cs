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

        public virtual bool Analyze(ILinkedNode node, T entity, ILinkedNodeFactory nodeFactory)
        {
            return false;
        }

        public virtual ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T, TFormat> format, ILinkedNodeFactory nodeFactory)
        {
            var node = parent[format];

            if(node != null)
            {
                if(!Analyze(node, format.Value, nodeFactory))
                {
                    if(format.Extension != null)
                    {
                        node.Set(Properties.Description, $"{format.Extension.ToUpperInvariant()} object", "en");
                    }
                }
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
