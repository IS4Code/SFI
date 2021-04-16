﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class BinaryFormatAnalyzer<T> : FormatObjectAnalyzer<T, IBinaryFileFormat>
    {
        public BinaryFormatAnalyzer(IEnumerable<Classes> recognizedClasses) : base(recognizedClasses)
        {

        }

        public BinaryFormatAnalyzer(params Classes[] recognizedClasses) : base(recognizedClasses)
        {

        }

        public sealed override ILinkedNode Analyze(ILinkedNode parent, IFormatObject<T, IBinaryFileFormat> entity, ILinkedNodeFactory nodeFactory)
        {
            return base.Analyze(parent, entity, nodeFactory);
        }
    }
}