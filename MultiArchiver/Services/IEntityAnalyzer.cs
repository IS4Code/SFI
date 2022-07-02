using IS4.MultiArchiver.Formats;
using System;
using System.Collections.Generic;
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
        public bool Initialized { get; }
        public ILinkedNodeFactory NodeFactory { get; }
        public MatchContext MatchContext { get; }

        public AnalysisContext(ILinkedNode parent = null, ILinkedNode node = null, bool initialized = false, ILinkedNodeFactory nodeFactory = null, MatchContext matchContext = default)
        {
            Parent = parent;
            Node = node;
            Initialized = initialized;
            NodeFactory = nodeFactory;
            MatchContext = matchContext;
        }

        public AnalysisContext WithParent(ILinkedNode parent)
        {
            return new AnalysisContext(parent, null, false, NodeFactory, MatchContext);
        }

        public AnalysisContext WithNode(ILinkedNode node)
        {
            return new AnalysisContext(Node != null && node != null && !Node.Equals(node) ? null : Parent, node, false, NodeFactory, MatchContext);
        }

        public AnalysisContext WithMatchContext(MatchContext matchContext)
        {
            return new AnalysisContext(Parent, Node, false, NodeFactory, matchContext);
        }

        public AnalysisContext WithMatchContext(Func<MatchContext, MatchContext> matchContextTransform)
        {
            return new AnalysisContext(Parent, Node, false, NodeFactory, matchContextTransform(MatchContext));
        }

        public AnalysisContext AsInitialized()
        {
            return new AnalysisContext(Parent, Node, true, NodeFactory, MatchContext);
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

    public abstract class EntityAnalyzer : IHasFileOutput
    {
        public event OutputFileDelegate OutputFile;

        protected ValueTask OnOutputFile(string name, bool isBinary, IReadOnlyDictionary<string, object> properties, Func<Stream, ValueTask> writer)
        {
            return (OutputFile?.Invoke(name, isBinary, properties, writer)).GetValueOrDefault();
        }

        protected ILinkedNode GetNode(AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.NodeFactory.NewGuidNode(), context);
        }

        protected ILinkedNode GetNode(string subName, AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.Parent?[subName] ?? context.NodeFactory.NewGuidNode(), context);
        }

        protected ILinkedNode GetNode(IIndividualUriFormatter<Uri> formatter, AnalysisContext context)
        {
            return
                InitNewNode(context.Node ?? context.Parent?[formatter] ?? context.NodeFactory.NewGuidNode(), context);
        }

        private ILinkedNode InitNewNode(ILinkedNode node, AnalysisContext context)
        {
            if(!context.Initialized) InitNode(node, context);
            return node;
        }

        protected virtual void InitNode(ILinkedNode node, AnalysisContext context)
        {

        }
    }
}
