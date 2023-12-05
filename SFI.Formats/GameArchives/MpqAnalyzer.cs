using IS4.SFI.Formats;
using IS4.SFI.Services;
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
                                yield return new Entry(mpq, entry.Entry);
                            }
                        }else{
                            yield return new Directory(this, "", group);
                        }
                    }
                }
            }

            public bool IsComplete => true;

            public bool IsSolid => false;

            class Entry : ArchiveFileWrapper<(MpqEntry entry, int index)>
            {
                readonly MpqArchive mpq;

                MpqEntry entry => Entry.entry;

                public Entry(MpqArchive mpq, (MpqEntry entry, int index) entry) : base(entry)
                {
                    this.mpq = mpq;
                    Path = this.entry.FileName?.Replace('\\', '/');
                }

                public override string? SubName => Entry.index.ToString();

                public override string? Path { get; }

                public override FileAttributes Attributes {
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

                protected override object? ReferenceKey => entry;

                protected override object? DataKey => null;

                public override long Length => entry.FileSize;

                public override StreamFactoryAccess Access => StreamFactoryAccess.Reentrant;

                public override Stream Open()
                {
                    return mpq.OpenFile(entry);
                }
            }
            
            class Directory : ArchiveDirectoryWrapper<Adapter, (MpqEntry entry, int index)>
            {
                public Directory(Adapter parent, string? path, IEntryGrouping entries) : base(parent, default, path, entries)
                {

                }

                protected override bool IsValidFile((MpqEntry? entry, int index) entry)
                {
                    return entry.entry != null;
                }

                protected override ArchiveFileWrapper<(MpqEntry entry, int index)> CreateFileWrapper((MpqEntry entry, int index) entry)
                {
                    return new Entry(Archive.mpq, entry);
                }

                protected override ArchiveDirectoryWrapper<Adapter, (MpqEntry entry, int index)> CreateDirectoryWrapper(string path, IEntryGrouping entries)
                {
                    return new Directory(Archive, path, entries);
                }
            }
        }
    }
}
