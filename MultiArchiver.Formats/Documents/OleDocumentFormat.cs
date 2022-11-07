using IS4.MultiArchiver.Services;
using NPOI.POIFS.FileSystem;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents a container format based on the OLE file structure.
    /// </summary>
    /// <typeparam name="T">The entity type produced by the format.</typeparam>
    public abstract class OleDocumentFormat<T> : ContainerFileFormat<IDirectoryInfo, T> where T : class
    {
        /// <inheritdoc/>
        public OleDocumentFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        protected sealed override IContainerAnalyzer MatchRoot(IDirectoryInfo root, AnalysisContext context)
        {
            if(root.Entries.OfType<IFileInfo>().Any(f => f.Name == "\x05SummaryInformation"))
            {
                return new PackageInfo(this);
            }
            return null;
        }

        /// <summary>
        /// Opens an instance of <see cref="NPOIFSFileSystem"/> and interprets
        /// it as the package-specific type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="fileSystem">The OLE file system.</param>
        /// <returns>The media object representing the package.</returns>
        protected abstract T Open(NPOIFSFileSystem fileSystem);
        
        private T TryOpen(NPOIFSFileSystem fileSystem)
        {
            try{
                return Open(fileSystem);
            }catch(InternalApplicationException)
            {
                throw;
            }catch{
                return null;
            }
        }

        class PackageInfo : IContainerAnalyzer<IContainerNode, IDirectoryInfo>, IContainerAnalyzer
        {
            readonly OleDocumentFormat<T> format;

            public PackageInfo(OleDocumentFormat<T> format)
            {
                this.format = format;
            }

            public async ValueTask<AnalysisResult> Analyze(IContainerNode parentNode, IDirectoryInfo root, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
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

                var obj = format.TryOpen(fs);
                if(obj != null)
                {
                    context = context.WithNode(null);
                    await analyzers.Analyze(new FormatObject<T>(format, obj), context);
                }
                return await inner(ContainerBehaviour.None);
            }

            ValueTask<AnalysisResult> IContainerAnalyzer.Analyze<TParent, TEntity>(TParent parentNode, TEntity entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
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
