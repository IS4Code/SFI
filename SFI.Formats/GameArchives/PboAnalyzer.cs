using BisUtils.PBO.Entries;
using BisUtils.PBO.Interfaces;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    using IEntryGrouping = IGrouping<string?, DirectoryTools.EntryInfo<PboDataEntry>>;

    /// <summary>
    /// Analyzes PBO archives, as instances of <see cref="IPboFile"/>.
    /// The analysis itself is performed by analyzing an
    /// <see cref="IArchiveFile"/> adapter from the instance.
    /// </summary>
    [Description("Analyzes PBO archives.")]
    public class PboAnalyzer : MediaObjectAnalyzer<IPboFile>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(IPboFile pbo, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var result = await analyzers.Analyze(new Adapter(pbo), context);
            if(result.Node != null)
            {
                if(pbo.GetVersionEntry() is { } version)
                {
                    foreach(var property in version.Metadata)
                    {
                        var value = property.PropertyValue;
                        switch(property.PropertyName)
                        {
                            case "version":
                                result.Node.Set(Properties.Version, value);
                                break;
                        }
                    }
                }
            }
            return result;
        }

        class Adapter : IArchiveFile
        {
            readonly IPboFile pbo;
            readonly string? prefix;

            public Adapter(IPboFile pbo)
            {
                this.pbo = pbo;
                prefix = pbo.GetVersionEntry()?.Metadata.FirstOrDefault(p => p.PropertyName == "prefix").PropertyValue;
            }

            public IEnumerable<IArchiveEntry> Entries {
                get {
                    foreach(var group in DirectoryTools.GroupByDirectories(pbo.GetPboEntries().OfType<PboDataEntry>(), e => e.EntryName.Replace('\\', '/')))
                    {
                        if(group.Key == null)
                        {
                            foreach(var entry in group)
                            {
                                yield return new Entry(this, entry.Entry);
                            }
                        }else{
                            yield return new Directory(this, prefix != null ? $"{prefix}/" : "", group);
                        }
                    }
                }
            }

            public bool IsComplete => true;

            public bool IsSolid => false;

            class Entry : IArchiveEntry, IFileInfo
            {
                readonly PboDataEntry entry;
                readonly Lazy<byte[]> data;

                public Entry(Adapter parent, PboDataEntry entry)
                {
                    this.entry = entry;
                    data = new Lazy<byte[]>(() => parent.pbo.GetEntryData(entry));
                    Path = (parent.prefix != null ? $"{parent.prefix}/{entry.EntryName}" : entry.EntryName)?.Replace('\\', '/');
                }

                public DateTime? ArchivedTime => null;

                public string? Name => System.IO.Path.GetFileName(Path);

                public string? SubName => null;

                public string? Path { get; }

                public int? Revision => null;

                public DateTime? CreationTime => null;

                public DateTime? LastWriteTime => entry.TimeStamp is 0 or 1 ? null : DateTimeOffset.FromUnixTimeSeconds(unchecked((long)entry.TimeStamp)).DateTime;

                public DateTime? LastAccessTime => null;

                public FileKind Kind => FileKind.ArchiveItem;
                
                public FileAttributes Attributes {
                    get {
                        switch(entry.EntryMagic)
                        {
                            case PboEntryMagic.Compressed:
                                return FileAttributes.Compressed;
                            case PboEntryMagic.Encrypted:
                                return FileAttributes.Encrypted;
                            default:
                                return FileAttributes.Normal;
                        }
                    }
                }

                public object? ReferenceKey => entry;

                public object? DataKey => null;

                public long Length => entry.OriginalSize != 0 ? unchecked((long)entry.OriginalSize) : entry.EntryData.Length;

                public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

                public Stream Open()
                {
                    return new MemoryStream(data.Value, false);
                }

                public override string ToString()
                {
                    return "/" + Path;
                }
            }
            
            class Directory : IArchiveEntry, IDirectoryInfo
            {
                readonly Adapter parent;

                readonly IEntryGrouping entries;

                readonly string? path;

                public string? Name => entries.Key;

                public string? Path => path + entries.Key;

                public Directory(Adapter parent, string? path, IEntryGrouping entries)
                {
                    this.parent = parent;
                    this.path = path;
                    this.entries = entries;
                }

                public IEnumerable<IFileNodeInfo> Entries {
                    get {
                        foreach(var group in DirectoryTools.GroupByDirectories(entries, e => e.SubPath, e => e.Entry))
                        {
                            if(group.Key == null)
                            {
                                foreach(var entry in group)
                                {
                                    if(!String.IsNullOrWhiteSpace(entry.SubPath))
                                    {
                                        yield return new Entry(parent, entry.Entry);
                                    }
                                }
                            }else{
                                yield return new Directory(parent, Path + "/", group);
                            }
                        }
                    }
                }

                public FileAttributes Attributes => FileAttributes.Directory;

                public Environment.SpecialFolder? SpecialFolderType => null;

                public override string ToString()
                {
                    return "/" + Path;
                }

                public object? ReferenceKey => parent.pbo;

                public object? DataKey => Path;

                public string? SubName => null;

                public int? Revision => null;

                public DateTime? ArchivedTime => null;

                public DateTime? CreationTime => null;

                public DateTime? LastWriteTime => null;

                public DateTime? LastAccessTime => null;

                public FileKind Kind => FileKind.ArchiveItem;
            }
        }
    }
}
