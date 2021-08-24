namespace IS4.MultiArchiver.Services
{
    public interface IPackageFormat<in TInput> : IFileFormat where TInput : class
    {
        IEntityAnalyzer<TInput> Match(TInput entity, MatchContext context);
    }

    public interface IPackageFormat<in TInput, TAnalyzer> : IFileFormat<TAnalyzer>, IPackageFormat<TInput> where TInput : class where TAnalyzer : class, IEntityAnalyzer<TInput>
    {
        new TAnalyzer Match(TInput entity, MatchContext context);
    }

    public abstract class PackageFormat<TInput, TAnalyzer> : FileFormat<TAnalyzer>, IPackageFormat<TInput, TAnalyzer> where TInput : class where TAnalyzer : class, IEntityAnalyzer<TInput>
    {
        public PackageFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public abstract TAnalyzer Match(TInput entity, MatchContext context);

        IEntityAnalyzer<TInput> IPackageFormat<TInput>.Match(TInput entity, MatchContext context)
        {
            return Match(entity, context);
        }
    }
}
