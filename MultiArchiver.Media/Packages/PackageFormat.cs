using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;

namespace IS4.MultiArchiver.Formats
{
    public abstract class PackageFormat<TAnalyzer> : IContainerAnalyzerProvider,
        PackageFormat<TAnalyzer>.IProvider<IDirectoryInfo>,
        PackageFormat<TAnalyzer>.IProvider<IArchiveInfo>
        where TAnalyzer : PackageFormat<TAnalyzer>.Analyzer
    {
        IContainerAnalyzer IContainerAnalyzerProvider.MatchRoot<TRoot>(TRoot root, AnalysisContext context)
        {
            if(this is IProvider<TRoot> provider)
            {
                return provider.MatchRoot(root, context);
            }
            return null;
        }

        protected abstract TAnalyzer MatchRoot(object root, AnalysisContext context);

        IContainerAnalyzer IProvider<IDirectoryInfo>.MatchRoot(object root, AnalysisContext context)
        {
            return MatchRoot(root, context);
        }

        IContainerAnalyzer IProvider<IArchiveInfo>.MatchRoot(object root, AnalysisContext context)
        {
            return MatchRoot(root, context);
        }

        protected interface IProvider<in TRoot> where TRoot : class
        {
            IContainerAnalyzer MatchRoot(object root, AnalysisContext context);
        }

        public abstract class Analyzer : IContainerAnalyzer
        {
            public string Root { get; private set; }

            protected abstract AnalysisResult Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers) where TNode : class where TPath : IContainerNode;

            AnalysisResult IContainerAnalyzer.Analyze<TPath, TNode>(TPath parentPath, TNode node, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
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
