using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public class ClassRecognizingAnalyzer<T> : IEntityAnalyzer<T> where T : class
    {
        readonly IEnumerable<Classes> recognizedClasses;

        public ClassRecognizingAnalyzer(IEnumerable<Classes> recognizedClasses)
        {
            this.recognizedClasses = recognizedClasses;
        }

        public ClassRecognizingAnalyzer(params Classes[] recognizedClasses) : this((IEnumerable<Classes>)recognizedClasses)
        {

        }

        public virtual ILinkedNode Analyze(T entity, ILinkedNodeFactory nodeFactory)
        {
            var node = nodeFactory.Root[Guid.NewGuid().ToString("D")];

            foreach(var cls in recognizedClasses)
            {
                node.Set(cls);
            }

            return node;
        }
    }
}
