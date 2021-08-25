﻿using ICSharpCode.SharpZipLib.Zip;
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

namespace IS4.MultiArchiver.Formats
{
    public abstract class OpenXmlDocumentFormat<T> : PackageFormat<IDirectoryInfo, T, OpenXmlDocumentFormat<T>.PackageInfo> where T : class
    {
        public OpenXmlDocumentFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        public sealed override PackageInfo Match(IDirectoryInfo root, MatchContext context)
        {
            if(root.Entries.Any(e => e.Name.Equals(ContentTypeManager.CONTENT_TYPES_PART_NAME, StringComparison.OrdinalIgnoreCase)))
            {
                var entrySource = new EntrySource(root);
                var package = new ZipPackage(entrySource, PackageAccess.READ);
                return new PackageInfo(this, package);
            }
            return null;
        }

        protected abstract T Open(OPCPackage package);

        public class PackageInfo : IEntityAnalyzer<IDirectoryInfo>
        {
            readonly OpenXmlDocumentFormat<T> format;
            public OPCPackage Package { get; }

            public PackageInfo(OpenXmlDocumentFormat<T> format, OPCPackage package)
            {
                this.format = format;
                Package = package;
                package.GetParts();
            }

            public AnalysisResult Analyze(IDirectoryInfo entity, AnalysisContext context, IEntityAnalyzer globalAnalyzer)
            {
                try{
                    var obj = format.Open(Package);
                    if(obj != null)
                    {
                        context = context.WithNode(null);
                        return globalAnalyzer.Analyze(new FormatObject<T>(format, obj), context);
                    }
                }catch(Exception e)
                {
                    return new AnalysisResult(null, exception: e);
                }
                return default;
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
