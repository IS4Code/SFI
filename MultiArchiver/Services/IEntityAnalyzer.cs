using System;

namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzer
    {
        AnalysisResult Analyze<T>(T entity, AnalysisContext context) where T : class;
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        AnalysisResult Analyze(T entity, AnalysisContext context, IEntityAnalyzer globalAnalyzer);
    }

    public struct AnalysisContext
    {
        public ILinkedNode Parent { get; }
        public ILinkedNode Node { get; }
        public ILinkedNodeFactory NodeFactory { get; }
        public MatchContext MatchContext { get; }

        public AnalysisContext(ILinkedNode parent = null, ILinkedNode node = null, ILinkedNodeFactory nodeFactory = null, MatchContext matchContext = default)
        {
            Parent = parent;
            Node = node;
            NodeFactory = nodeFactory;
            MatchContext = matchContext;
        }

        public AnalysisContext WithParent(ILinkedNode parent)
        {
            return new AnalysisContext(parent, null, NodeFactory, MatchContext);
        }

        public AnalysisContext WithNode(ILinkedNode node)
        {
            return new AnalysisContext(null, node, NodeFactory, MatchContext);
        }

        public AnalysisContext WithMatchContext(MatchContext matchContext)
        {
            return new AnalysisContext(Parent, Node, NodeFactory, matchContext);
        }

        public AnalysisContext WithMatchContext(Func<MatchContext, MatchContext> matchContextTransform)
        {
            return new AnalysisContext(Parent, Node, NodeFactory, matchContextTransform(MatchContext));
        }
    }

    public struct AnalysisResult
    {
        public ILinkedNode Node { get; set; }
        public string Label { get; set; }
        public Exception Exception { get; set; }

        public AnalysisResult(ILinkedNode node, string label = null, Exception exception = null)
        {
            Node = node;
            Label = label;
            Exception = exception;
        }
    }

    public abstract class EntityAnalyzer<T> : IEntityAnalyzer<T> where T : class
    {
        protected ILinkedNode GetNode(AnalysisContext context)
        {
            var node = context.Node ?? context.NodeFactory.NewGuidNode();
            InitNode(node, context);
            return node;
        }

        protected ILinkedNode GetNode(string subName, AnalysisContext context)
        {
            var node = context.Node ?? context.Parent?[subName] ?? context.NodeFactory.NewGuidNode();
            InitNode(node, context);
            return node;
        }

        protected ILinkedNode GetNode(IIndividualUriFormatter<Uri> formatter, AnalysisContext context)
        {
            var node = context.Node ?? context.Parent?[formatter] ?? context.NodeFactory.NewGuidNode();
            InitNode(node, context);
            return node;
        }

        protected virtual void InitNode(ILinkedNode node, AnalysisContext context)
        {

        }

        public abstract AnalysisResult Analyze(T entity, AnalysisContext context, IEntityAnalyzer globalAnalyzer);
    }
}
