using IS4.MultiArchiver.Services;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// An implementation of <see cref="IContainerAnalyzerProvider"/> as a file format, producing entities
    /// of type <typeparamref name="TEntity"/> representing hierarchies
    /// starting with instances of <typeparamref name="TRoot"/>.
    /// </summary>
    /// <typeparam name="TRoot">The root type of the package.</typeparam>
    /// <typeparam name="TEntity">The type of of the produced entities.</typeparam>
    public abstract class ContainerFileFormat<TRoot, TEntity> : FileFormat<TEntity>,
        IContainerAnalyzerProvider<TRoot>
        where TRoot : class
        where TEntity : class
    {
        /// <inheritdoc/>
        public ContainerFileFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        /// <summary>
        /// Called from the implementation of <see cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        /// when the type parameter is compatible with <typeparamref name="TRoot"/>.
        /// </summary>
        /// <inheritdoc cref="IContainerAnalyzerProvider.MatchRoot{TRoot}(TRoot, AnalysisContext)"/>
        protected abstract IContainerAnalyzer Match(TRoot root, MatchContext context);

        IContainerAnalyzer IContainerAnalyzerProvider<TRoot>.MatchRoot(TRoot root, AnalysisContext context)
        {
            return Match(root, context.MatchContext);
        }

        IContainerAnalyzer IContainerAnalyzerProvider.MatchRoot<TRoot2>(TRoot2 root, AnalysisContext context)
        {
            if(this is IContainerAnalyzerProvider<TRoot2> provider)
            {
                return provider.MatchRoot(root, context);
            }
            return null;
        }
    }
}
