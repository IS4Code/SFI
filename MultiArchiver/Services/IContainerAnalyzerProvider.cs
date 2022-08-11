using System;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Services
{
    /// <summary>
    /// Produces an instance of <see cref="IContainerAnalyzer"/> from the root
    /// of a container encountered during analysis.
    /// </summary>
    public interface IContainerAnalyzerProvider
    {
        /// <summary>
        /// Attempts to match an object as a root of the container hierarchy.
        /// </summary>
        /// <typeparam name="TRoot">The type of <paramref name="root"/>.</typeparam>
        /// <param name="root">The root of the hierarchy, of an arbitrary type.</param>
        /// <param name="context">Additional context of the analysis.</param>
        /// <returns>
        /// An instance of <see cref="IContainerAnalyzer"/> if the root is
        /// recognized, null otherwise.
        /// </returns>
        IContainerAnalyzer MatchRoot<TRoot>(TRoot root, AnalysisContext context) where TRoot : class;
    }

    public delegate ValueTask<AnalysisResult> AnalyzeInner(ContainerBehaviour behaviour);

    [Flags]
    public enum ContainerBehaviour
    {
        None = 0,
        FollowChildren = 1,
        BlockOther = 2
    }

    public interface IContainerAnalyzer
    {
        ValueTask<AnalysisResult> Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers) where TEntity : class where TParent : IContainerNode;
    }

    public interface IContainerAnalyzer<in TParent, in TEntity> where TEntity : class where TParent : IContainerNode
    {
        ValueTask<AnalysisResult> Analyze(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers);
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
