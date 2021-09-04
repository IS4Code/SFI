using IS4.MultiArchiver.Services;

namespace IS4.MultiArchiver.Formats
{
    public abstract class LegacyPackageFileFormat<TRoot, TEntity, TAnalyzer> : FileFormat<TEntity>,
        IContainerAnalyzerProvider,
        LegacyPackageFileFormat<TRoot, TEntity, TAnalyzer>.IMatcher<TRoot>
        where TRoot : class
        where TEntity : class
        where TAnalyzer : IContainerAnalyzer
    {
        public LegacyPackageFileFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public abstract TAnalyzer Match(TRoot root, MatchContext context);

        IContainerAnalyzer IContainerAnalyzerProvider.MatchRoot<TRoot2>(TRoot2 root, AnalysisContext context)
        {
            if(this is IMatcher<TRoot2> matcher)
            {
                return matcher.Match(root, context.MatchContext);
            }
            return null;
        }

        interface IMatcher<in TRoot2>
        {
            TAnalyzer Match(TRoot2 root, MatchContext context);
        }
    }
}
