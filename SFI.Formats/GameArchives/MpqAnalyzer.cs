using IS4.SFI.Formats;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using War3Net.IO.Mpq;

namespace IS4.SFI.Analyzers
{
    using IEntryGrouping = IGrouping<string?, DirectoryTools.EntryInfo<(MpqEntry entry, int index)>>;

    /// <summary>
    /// Analyzes MPQ archives, as instances of <see cref="MpqArchive"/>.
    /// The analysis itself is performed by analyzing an
    /// <see cref="IArchiveFile"/> adapter from the instance.
    /// </summary>
    [Description("Analyzes MPQ archives.")]
    public class MpqAnalyzer : MediaObjectAnalyzer<MpqArchive>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(MpqArchive wad, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return await analyzers.Analyze(new Adapter(wad), context);
        }

        class Adapter : IArchiveFile
        {
            readonly MpqArchive mpq;

            public Adapter(MpqArchive mpq)
            {
                this.mpq = mpq;
            }
            
            public IEnumerable<IArchiveEntry> Entries {
                get {
                    foreach(var group in DirectoryTools.GroupByDirectories(mpq.Select((e, i) => (e, i)), e => e.e.FileName?.Replace('\\', '/')))
                    {
                        if(group.Key == null)
                        {
                            foreach(var entry in group)
                            {
                                yield return new Entry(mpq, entry.Entry.i, entry.Entry.e);
                            }
                        }else{
                            yield return new Directory(this, "", group);
                        }
                    }
                }
            }

            public bool IsComplete => true;

            public bool IsSolid => false;

            class Entry : IArchiveEntry, IFileInfo
            {
                readonly MpqArchive mpq;
                readonly MpqEntry entry;
                readonly int index;

                public Entry(MpqArchive mpq, int index, MpqEntry entry)
                {
                    this.mpq = mpq;
                    this.index = index;
                    this.entry = entry;
                    Path = entry.FileName?.Replace('\\', '/');
                }

                public DateTime? ArchivedTime => null;

                public string? Name => System.IO.Path.GetFileName(Path);

                public string? SubName => index.ToString();

                public string? Path { get; }

                public int? Revision => null;

                public DateTime? CreationTime => null;

                public DateTime? LastWriteTime => null;

                public DateTime? LastAccessTime => null;

                public FileKind Kind => FileKind.ArchiveItem;

                public FileAttributes Attributes {
                    get {
                        FileAttributes attributes = 0;
                        if(entry.IsCompressed)
                        {
                            attributes |= FileAttributes.Compressed;
                        }
                        if(entry.IsEncrypted)
                        {
                            attributes |= FileAttributes.Encrypted;
                        }
                        if(attributes == 0)
                        {
                            return FileAttributes.Normal;
                        }
                        return attributes;
                    }
                }

                public object? ReferenceKey => entry;

                public object? DataKey => null;

                public long Length => entry.FileSize;

                public StreamFactoryAccess Access => StreamFactoryAccess.Reentrant;

                public Stream Open()
                {
                    return mpq.OpenFile(entry);
                }

                public override string? ToString()
                {
                    return Path == null ? null : "/" + Path;
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
                                        yield return new Entry(parent.mpq, entry.Entry.index, entry.Entry.entry);
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

                public object? ReferenceKey => parent.mpq;

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
