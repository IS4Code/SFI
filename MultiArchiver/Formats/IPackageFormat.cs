using IS4.MultiArchiver.Services;

namespace IS4.MultiArchiver.Formats
{
    public interface IPackageFormat<in TEntity> : IFileFormat where TEntity : class
    {
        IEntityAnalyzerProvider Match(TEntity entity, MatchContext context);
    }

    public abstract class PackageFormat<TEntity, TValue, TAnalyzer> : FileFormat<TValue>, IPackageFormat<TEntity> where TEntity : class where TValue : class where TAnalyzer : IEntityAnalyzerProvider
    {
        public PackageFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public abstract TAnalyzer Match(TEntity entity, MatchContext context);

        IEntityAnalyzerProvider IPackageFormat<TEntity>.Match(TEntity entity, MatchContext context)
        {
            return Match(entity, context);
        }
    }
}
