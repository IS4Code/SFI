using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// An implementation of <see cref="IContainerAnalyzerProvider"/> for packages,
    /// able to analyze file systems as instances of <see cref="IDirectoryInfo"/>
    /// or <see cref="IArchiveInfo"/> by default.
    /// </summary>
    /// <typeparam name="TAnalyzer">The concrete analyzer type used by the implementation.</typeparam>
    public abstract class PackageProvider<TAnalyzer> : IContainerAnalyzerProvider,
        PackageProvider<TAnalyzer>.IProvider<IDirectoryInfo>,
        PackageProvider<TAnalyzer>.IProvider<IArchiveInfo>
        where TAnalyzer : PackageProvider<TAnalyzer>.Analyzer
    {
        IContainerAnalyzer IContainerAnalyzerProvider.MatchRoot<TRoot>(TRoot root, AnalysisContext context)
        {
            if(this is IProvider<TRoot> provider)
            {
                return provider.MatchRoot(root, context);
            }
            return null;
        }

        /// <summary>
        /// When implemented, creates an instance of <typeparamref name="TAnalyzer"/> to analyze
        /// the entities logically contained in a hierarchy starting at <paramref name="root"/>.
        /// </summary>
        /// <param name="root">The root of the hierarchy, used e.g. as <see cref="IPersistentKey.ReferenceKey"/> if needed.</param>
        /// <param name="context">The context of the analysis.</param>
        /// <returns>
        /// An instance of <typeparamref name="TAnalyzer"/> for <paramref name="root"/>.
        /// This analyzer will be used again for the root itself and all nodes under it,
        /// based on the specified <see cref="ContainerBehaviour"/>.
        /// </returns>
        protected abstract TAnalyzer MatchRoot(object root, AnalysisContext context);

        IContainerAnalyzer IProvider<IDirectoryInfo>.MatchRoot(IDirectoryInfo root, AnalysisContext context)
        {
            return MatchRoot(root, context);
        }

        IContainerAnalyzer IProvider<IArchiveInfo>.MatchRoot(IArchiveInfo root, AnalysisContext context)
        {
            return MatchRoot(root, context);
        }

        /// <summary>
        /// Implementing this interface ensures that <see cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        /// succeeds when the argument is compatible with type <typeparamref name="TRoot"/>.
        /// </summary>
        /// <typeparam name="TRoot">The root type recognized by the provider.</typeparam>
        protected interface IProvider<in TRoot> where TRoot : class
        {
            /// <inheritdoc cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
            IContainerAnalyzer MatchRoot(TRoot root, AnalysisContext context);
        }

        /// <summary>
        /// Provides an implementation of <see cref="IContainerAnalyzer"/>
        /// which stores the root of the file hierarchy in <see cref="Root"/>.
        /// </summary>
        public abstract class Analyzer : IContainerAnalyzer
        {
            /// <summary>
            /// If the root of the hierarchy is a directory, stores its path.
            /// </summary>
            public string Root { get; private set; }

            /// <inheritdoc cref="IContainerAnalyzer.Analyze{TParent, TEntity}(TParent, TEntity, AnalysisContext, AnalyzeInner, IEntityAnalyzers)"/>
            protected abstract ValueTask<AnalysisResult> Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers) where TNode : class where TPath : IContainerNode;

            ValueTask<AnalysisResult> IContainerAnalyzer.Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
            {
                if(node is IDirectoryInfo dir)
                {
                    if(Root == null || Root.StartsWith(dir.Path))
                    {
                        Root = dir.Path;
                    }
                }
                return Analyze(parentPath, node, context, inner, analyzers);
            }
        }
    }
}
