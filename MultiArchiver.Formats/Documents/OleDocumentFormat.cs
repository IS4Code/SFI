using IS4.MultiArchiver.Services;
using NPOI.POIFS.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.MultiArchiver.Formats
{
    public abstract class OleDocumentFormat<T> : PackageFormat<IDirectoryInfo, T, OleDocumentFormat<T>.PackageInfo> where T : class
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

        public class PackageInfo : IEntityAnalyzer<IDirectoryInfo>, IEntityAnalyzerProvider
        {
            readonly OleDocumentFormat<T> format;

            public PackageInfo(OleDocumentFormat<T> format)
            {
                this.format = format;
            }

            public AnalysisResult Analyze(IDirectoryInfo root, AnalysisContext context, IEntityAnalyzerProvider analyzers)
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

                try{
                    var obj = format.Open(fs);
                    if(obj != null)
                    {
                        context = context.WithNode(null);
                        return analyzers.Analyze(new FormatObject<T>(format, obj), context);
                    }
                }catch(Exception e)
                {
                    return new AnalysisResult(null, exception: e);
                }
                return default;
            }

            IEnumerable<IEntityAnalyzer<T1>> IEntityAnalyzerProvider.GetAnalyzers<T1>()
            {
                if(this is IEntityAnalyzer<T1> analyzer)
                {
                    yield return analyzer;
                }
            }
        }
    }
}
