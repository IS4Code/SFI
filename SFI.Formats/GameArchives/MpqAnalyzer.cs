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

            public IEnumerable<IArchiveEntry> Entries => mpq.Select((e, i) => new Entry(mpq, i, e));

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
                }

                public DateTime? ArchivedTime => null;

                public string? Name => entry.FileName;

                public string? SubName => index.ToString();

                public string? Path => Name;

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
        }
    }
}
