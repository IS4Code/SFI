using IS4.MultiArchiver.Services;
using NPOI.POIFS.FileSystem;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public abstract class OleDocumentFormat<T> : LegacyPackageFileFormat<IDirectoryInfo, T, OleDocumentFormat<T>.PackageInfo> where T : class
    {
        public OleDocumentFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public sealed override PackageInfo Match(IDirectoryInfo root, MatchContext context)
        {
            if(root.Entries.OfType<IFileInfo>().Any(f => f.Name == "\x05SummaryInformation"))
            {
                return new PackageInfo(this);
            }
            return null;
        }

        protected abstract T Open(NPOIFSFileSystem fileSystem);

        public class PackageInfo : IContainerAnalyzer<IContainerNode, IDirectoryInfo>, IContainerAnalyzer
        {
            readonly OleDocumentFormat<T> format;

            public PackageInfo(OleDocumentFormat<T> format)
            {
                this.format = format;
            }

            public async ValueTask<AnalysisResult> Analyze(IContainerNode parentNode, IDirectoryInfo root, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
            {
                var fs = new NPOIFSFileSystem();

                void Add(DirectoryEntry parent, IDirectoryInfo dir)
                {
                    foreach(var entry in dir.Entries)
                    {
                        if(entry is IFileInfo file)
                        {
                            using(var stream = file.Open())
                            {
                                parent.CreateDocument(file.Name, stream);
                            }
                        }
                        if(entry is IDirectoryInfo subDir)
                        {
                            var node = parent.CreateDirectory(entry.Name);
                            Add(node, subDir);
                        }
                    }
                }
                Add(fs.Root, root);

                var obj = format.Open(fs);
                if(obj != null)
                {
                    context = context.WithNode(null);
                    await analyzers.Analyze(new FormatObject<T>(format, obj), context);
                }
                return await inner(ContainerBehaviour.None);
            }

            ValueTask<AnalysisResult> IContainerAnalyzer.Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzerProvider analyzers)
            {
                if(this is IContainerAnalyzer<TParent, TEntity> analyzer)
                {
                    return analyzer.Analyze(parentNode, entity, context, inner, analyzers);
                }
                return inner(ContainerBehaviour.None);
            }
        }
    }
}
