using ICSharpCode.SharpZipLib.Zip;
using IS4.MultiArchiver.Services;
using NPOI.OpenXml4Net.OPC;
using NPOI.OpenXml4Net.OPC.Internal;
using NPOI.OpenXml4Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents a format based on the OOXML package format.
    /// </summary>
    /// <typeparam name="T">The type of the instances created by the format.</typeparam>
    public abstract class OpenXmlDocumentFormat<T> : LegacyPackageFileFormat<IDirectoryInfo, T, OpenXmlDocumentFormat<T>.PackageInfo> where T : class
    {
        /// <inheritdoc/>
        public OpenXmlDocumentFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public sealed override PackageInfo Match(IDirectoryInfo root, MatchContext context)
        {
            if(root.Entries.Any(e => ContentTypeManager.CONTENT_TYPES_PART_NAME.Equals(e.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var entrySource = new EntrySource(root);
                var package = new ZipPackage(entrySource, PackageAccess.READ);
                return new PackageInfo(this, root, package);
            }
            return null;
        }

        protected abstract T Open(OPCPackage package);

        public class PackageInfo : IContainerAnalyzer<IContainerNode, IFileNodeInfo>, IContainerAnalyzer
        {
            readonly OpenXmlDocumentFormat<T> format;
            readonly IDirectoryInfo root;
            public OPCPackage Package { get; }

            public PackageInfo(OpenXmlDocumentFormat<T> format, IDirectoryInfo root, OPCPackage package)
            {
                this.format = format;
                this.root = root;
                Package = package;
                package.GetParts();
            }

            public async ValueTask<AnalysisResult> Analyze(IContainerNode parentNode, IFileNodeInfo entity, AnalysisContext context, AnalyzeInner inner, IEntityAnalyzers analyzers)
            {
                if(entity == root)
                {
                    var obj = format.Open(Package);
                    if(obj != null)
                    {
                        context = context.WithNode(null);
                        await analyzers.Analyze(new FormatObject<T>(format, obj), context);
                    }
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

        class EntrySource : ZipEntrySource
        {
            readonly ConditionalWeakTable<ZipEntry, IFileInfo> fileCache = new ConditionalWeakTable<ZipEntry, IFileInfo>();
            readonly IDirectoryInfo root;

            public EntrySource(IDirectoryInfo root)
            {
                this.root = root;
            }

            public IEnumerator<ZipEntry> Entries => GetEntries(root).GetEnumerator();

            IEnumerator ZipEntrySource.Entries => Entries;

            public bool IsClosed { get; private set; }

            IEnumerable<ZipEntry> GetEntries(IDirectoryInfo parent)
            {
                foreach(var entry in parent.Entries)
                {
                    if(entry is IFileInfo file)
                    {
                        var zipEntry = new ZipEntry(entry.Path);
                        zipEntry.DateTime = file.LastWriteTime ?? zipEntry.DateTime;
                        zipEntry.Size = file.Length;
                        fileCache.Add(zipEntry, file);
                        yield return zipEntry;
                    }
                    if(entry is IDirectoryInfo dir)
                    {
                        foreach(var innerEntry in GetEntries(dir))
                        {
                            yield return innerEntry;
                        }
                    }
                }
            }

            public void Close()
            {
                IsClosed = true;
            }

            public Stream GetInputStream(ZipEntry entry)
            {
                if(!fileCache.TryGetValue(entry, out var info)) return null;
                return info.Open();
            }
        }
    }
}
