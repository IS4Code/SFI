using IS4.MultiArchiver.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzerProvider
    {
        ValueTask<AnalysisResult> Analyze<T>(T entity, AnalysisContext context) where T : class;
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        ValueTask<AnalysisResult> Analyze(T entity, AnalysisContext context, IEntityAnalyzerProvider analyzers);
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
            return new AnalysisContext(Node != null && node != null && !Node.Equals(node) ? null : Parent, node, NodeFactory, MatchContext);
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

    public delegate ValueTask AnalysisOutputFile(string name, Func<Stream, ValueTask> writer);

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

    public abstract class EntityAnalyzer
    {
        public event AnalysisOutputFile OutputFile;

        protected ValueTask OnOutputFile(string name, Func<Stream, ValueTask> writer)
        {
            return (OutputFile?.Invoke(name, writer)).GetValueOrDefault();
        }

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
    }
}
