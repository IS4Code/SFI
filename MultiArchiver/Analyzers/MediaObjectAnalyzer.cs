using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Vocabulary;
using System.Collections.Generic;

namespace IS4.MultiArchiver.Analyzers
{
    public abstract class MediaObjectAnalyzer<T> : EntityAnalyzer, IEntityAnalyzer<T> where T : class
    {
        readonly IEnumerable<ClassUri> classes;

        public MediaObjectAnalyzer(IEnumerable<ClassUri> classes)
        {
            this.classes = classes;
        }

        public MediaObjectAnalyzer(params ClassUri[] classes) : this((IEnumerable<ClassUri>)classes)
        {

        }

        public abstract AnalysisResult Analyze(T entity, AnalysisContext context, IEntityAnalyzerProvider analyzers);

        protected override void InitNode(ILinkedNode node, AnalysisContext context)
        {
            base.InitNode(node, context);

            node.SetClass(Classes.MediaObject);
            foreach(var cls in classes)
            {
                node.SetClass(cls);
            }
        }
    }
}
