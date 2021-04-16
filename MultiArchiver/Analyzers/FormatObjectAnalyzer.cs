﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;
using System.Text;

namespace IS4.MultiArchiver.Analyzers
{
    public abstract class FormatObjectAnalyzer<T, TFormat> : IEntityAnalyzer<IFormatObject<T, TFormat>> where TFormat : IFileFormat
    {
        readonly IEnumerable<Classes> recognizedClasses;

        public FormatObjectAnalyzer(IEnumerable<Classes> recognizedClasses)
        {
            this.recognizedClasses = recognizedClasses;
        }

        public FormatObjectAnalyzer(params Classes[] recognizedClasses) : this((IEnumerable<Classes>)recognizedClasses)
        {

        }

        protected virtual ILinkedNode CreateNode(ILinkedNode parent, IFormatObject<T, TFormat> format, ILinkedNodeFactory nodeFactory)
        {
            if(format.MediaType == null)
            {
                return nodeFactory.Root[Guid.NewGuid().ToString("D")];
            }
            var split = format.MediaType.Split('/');
            if(split.Length < 2)
            {
                return nodeFactory.Root[Guid.NewGuid().ToString("D")];
            }
            return parent[split[1]];
        }

        public virtual bool Analyze(ILinkedNode node, T entity, ILinkedNodeFactory nodeFactory)
        {
            return false;
        }

        public virtual ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T, TFormat> entity, ILinkedNodeFactory nodeFactory)
        {
            var node = CreateNode(parent, entity, nodeFactory);

            if(node != null)
            {
                if(!Analyze(node, entity.Value, nodeFactory))
                {
                    if(entity.Extension != null)
                    {
                        node.Set(Properties.PrefLabel, $"{entity.Extension.ToUpperInvariant()} object", "en");
                    }
                }
                node.SetClass(Classes.MediaObject);
                foreach(var cls in recognizedClasses)
                {
                    node.SetClass(cls);
                }
                if(entity.MediaType != null)
                {
                    node.Set(Properties.EncodingFormat, Vocabularies.Mime, Uri.EscapeUriString(entity.MediaType));
                }
            }

            return node;
        }
    }
}