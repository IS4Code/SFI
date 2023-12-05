using BisUtils.PBO;
using BisUtils.PBO.Builders;
using BisUtils.PBO.Entries;
using BisUtils.PBO.Interfaces;
using IS4.SFI.Formats;
using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
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
            return await analyzers.Analyze(new Adapter(pbo), context);
        }

        class Adapter : IArchiveFile
        {
            readonly IPboFile pbo;

            public Adapter(IPboFile pbo)
            {
                this.pbo = pbo;
            }

            public IEnumerable<IArchiveEntry> Entries => pbo.GetPboEntries().OfType<PboDataEntry>().Select(e => new Entry(pbo, e));

            public bool IsComplete => true;

            public bool IsSolid => false;

            class Entry : IArchiveEntry, IFileInfo
            {
                readonly PboDataEntry entry;
                readonly Lazy<byte[]> data;

                public Entry(IPboFile pbo, PboDataEntry entry)
                {
                    this.entry = entry;
                    data = new Lazy<byte[]>(() => pbo.GetEntryData(entry));
                }

                public DateTime? ArchivedTime => null;

                public string? Name => entry.EntryName;

                public string? SubName => null;

                public string? Path => Name;

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
                    return "/" + Name;
                }
            }
        }
    }
}
