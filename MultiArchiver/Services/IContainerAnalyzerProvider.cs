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

    /// <typeparam name="TRoot">The root object type recognized by the provider.</typeparam>
    /// <inheritdoc cref="IContainerAnalyzerProvider"/>
    public interface IContainerAnalyzerProvider<in TRoot> : IContainerAnalyzerProvider where TRoot : class
    {
        /// <inheritdoc cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        IContainerAnalyzer MatchRoot(TRoot root, AnalysisContext context);
    }

    /// <summary>
    /// The delegate provided to <see cref="IContainerAnalyzer.Analyze{TParent, TEntity}(TParent, TEntity, AnalysisContext, AnalyzeInner, IEntityAnalyzers)"/>
    /// and <see cref="IContainerAnalyzer{TParent, TEntity}.Analyze(TParent, TEntity, AnalysisContext, AnalyzeInner, IEntityAnalyzers)"/>
    /// that is called to execute the inner analyzers for the entity and instruct
    /// the provider how to process other nodes or analyzers.
    /// </summary>
    /// <param name="behaviour">The requested behaviour of the analysis in relation to other analyzers or nodes.</param>
    /// <returns>The result of the inner analysis.</returns>
    public delegate ValueTask<AnalysisResult> AnalyzeInner(ContainerBehaviour behaviour);

    /// <summary>
    /// Specifies the flags provided via <see cref="AnalyzeInner"/>
    /// </summary>
    [Flags]
    public enum ContainerBehaviour
    {
        /// <summary>
        /// No specific behaviour of the analysis.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that children nodes of the current node should be followed
        /// by the current analyzer.
        /// </summary>
        FollowChildren = 1,

        /// <summary>
        /// Indicates that multiple instances of this analyzer type should not
        /// enter this node, i.e. that the instance of <see cref="IContainerAnalyzerProvider"/>
        /// that was used to obtain this analyzer should not be used when matching this node
        /// as a root.
        /// </summary>
        BlockOther = 2
    }

    /// <summary>
    /// Represents an object returned from <see cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
    /// capable of analyzing a node in a hierarchy and traversing its path to the root.
    /// </summary>
    public interface IContainerAnalyzer
    {
        /// <summary>
        /// Analyzes an entity in a hierarchy, similarly to <see cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)"/>.
        /// </summary>
        /// <typeparam name="TParent">The type of the parent node, implementing <see cref="IContainerNode"/>.</typeparam>
        /// <typeparam name="TEntity">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="parentNode">The parent node of <paramref name="entity"/>.</param>
        /// <param name="entity">The entity to analyze.</param>
        /// <param name="inner">
        /// A delegate which should be called by the implementation to execute other analyzers of perform the base analysis.
        /// </param>
        /// <inheritdoc cref="IEntityAnalyzer{T}.Analyze(T, AnalysisContext, IEntityAnalyzers)"/>
        ValueTask<AnalysisResult> Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers) where TEntity : class where TParent : IContainerNode;
    }

    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <inheritdoc cref="IContainerAnalyzer"/>
    public interface IContainerAnalyzer<in TParent, in TEntity> where TEntity : class where TParent : IContainerNode
    {
        /// <inheritdoc cref="IContainerAnalyzer.Analyze{TParent, TEntity}(TParent, TEntity, AnalysisContext, AnalyzeInner, IEntityAnalyzers)"/>
        ValueTask<AnalysisResult> Analyze(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers);
    }

    /// <summary>
    /// Represents a node in a container hierarchy.
    /// </summary>
    public interface IContainerNode
    {
        /// <summary>
        /// The parent of the current node.
        /// </summary>
        IContainerNode ParentNode { get; }

        /// <summary>
        /// The value of the current node.
        /// </summary>
        object Value { get; }
    }

    /// <typeparam name="TValue">The type of <see cref="Value"/>.</typeparam>
    /// <typeparam name="TParent">The type of <see cref="ParentNode"/>, implementing <see cref="IContainerNode"/>.</typeparam>
    /// <inheritdoc cref="IContainerNode"/>
    public interface IContainerNode<out TValue, out TParent> : IContainerNode where TValue : class where TParent : IContainerNode
    {
        /// <inheritdoc cref="IContainerNode.ParentNode"/>
        new TParent ParentNode { get; }

        /// <inheritdoc cref="IContainerNode.Value"/>
        new TValue Value { get; }
    }
}
