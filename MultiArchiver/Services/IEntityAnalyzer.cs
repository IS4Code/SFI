using System;
using System.IO;

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
        public object Source { get; }
        public Stream Stream { get; }

        public AnalysisContext(ILinkedNode parent = null, ILinkedNode node = null, ILinkedNodeFactory nodeFactory = null, object source = null, Stream stream = null)
        {
            Parent = parent;
            Node = node;
            NodeFactory = nodeFactory;
            Source = source;
            Stream = stream;
        }

        public AnalysisContext WithParent(ILinkedNode parent)
        {
            return new AnalysisContext(parent, null, NodeFactory, Source, Stream);
        }

        public AnalysisContext WithNode(ILinkedNode node)
        {
            return new AnalysisContext(null, node, NodeFactory, Source, Stream);
        }

        public AnalysisContext WithSource(object source)
        {
            return new AnalysisContext(Parent, Node, NodeFactory, source, Stream);
        }

        public AnalysisContext WithStream(Stream stream)
        {
            return new AnalysisContext(Parent, Node, NodeFactory, Source, stream);
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
