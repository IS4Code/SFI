using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace IS4.MultiArchiver.Services
{
    public interface IEntityAnalyzerProvider
    {
        IEnumerable<IEntityAnalyzer<T>> GetAnalyzers<T>() where T : class;
    }

    public static class EntityAnalyzerExtensions
    {
        public static AnalysisResult Analyze<T>(this IEntityAnalyzerProvider analyzers, T entity, AnalysisContext context, IEntityAnalyzerProvider customAnalyzer = null) where T : class
        {
            if(entity == null) return default;
            foreach(var analyzer in analyzers.GetAnalyzers<T>())
            {
                try{
                    if(typeof(T).Equals(typeof(IStreamFactory)))
                    {
                        Console.Error.WriteLine($"Data ({((IStreamFactory)entity).Length} B)");
                    }else{
                        Console.Error.WriteLine(entity);
                    }
                    var result = analyzer.Analyze(entity, context, customAnalyzer ?? analyzers);
                    if(result.Node != null)
                    {
                        return result;
                    }
                }catch(InternalArchiverException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    throw;
                }catch(Exception e)
                {
                    Console.Error.WriteLine("Error in analyzer " + analyzer);
                    Console.Error.WriteLine(e);
                }
            }
            return default;
        }
    }

    public interface IEntityAnalyzer<in T> where T : class
    {
        AnalysisResult Analyze(T entity, AnalysisContext context, IEntityAnalyzerProvider analyzers);
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
