﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Xml;

namespace IS4.MultiArchiver.Analyzers
{
    public class XmlAnalyzer : ClassRecognizingAnalyzer<XmlReader>
    {
        public XmlAnalyzer() : base(Classes.ContentAsXML)
        {

        }

        public override ILinkedNode Analyze(XmlReader entity, ILinkedNodeFactory nodeFactory)
        {
            var node = base.Analyze(entity, nodeFactory);
            if(node != null)
            {

            }
            return node;
        }
    }
}
