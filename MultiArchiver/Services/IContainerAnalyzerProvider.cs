using System;

namespace IS4.MultiArchiver.Services
{
    public interface IContainerAnalyzerProvider
    {
        IContainerAnalyzer MatchRoot<TRoot>(TRoot root, AnalysisContext context) where TRoot : class;
    }

    public delegate AnalysisResult AnalyzeInner(ContainerBehaviour behaviour);

    [Flags]
    public enum ContainerBehaviour
    {
        None = 0,
        FollowChildren = 1,
        BlockOther = 2
    }

    public interface IContainerAnalyzer
    {
        AnalysisResult Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers) where TEntity : class where TParent : IContainerNode;
    }

    public interface IContainerAnalyzer<in TParent, in TEntity> where TEntity : class where TParent : IContainerNode
    {
        AnalysisResult Analyze(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers);
    }

    public interface IContainerNode
    {
        IContainerNode ParentNode { get; }
        object Value { get; }
    }

    public interface IContainerNode<out TValue, out TParent> : IContainerNode where TValue : class where TParent : IContainerNode
    {
        new TParent ParentNode { get; }
        new TValue Value { get; }
    }
}
