using IS4.SFI.Formats;
using IS4.SFI.Services;
using nz.doom.WadParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// Analyzes WAD archives, as instances of <see cref="Wad"/>.
    /// The analysis itself is performed by analyzing an
    /// <see cref="IArchiveFile"/> adapter from the instance.
    /// </summary>
    [Description("Analyzes WAD archives.")]
    public class WadAnalyzer : MediaObjectAnalyzer<Wad>
    {
        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(Wad wad, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return await analyzers.Analyze(new Adapter(wad), context);
        }

        class Adapter : IArchiveFile
        {
            readonly Wad wad;

            public Adapter(Wad wad)
            {
                this.wad = wad;
            }

            public IEnumerable<IArchiveEntry> Entries => wad.Lumps.Select(l => new Entry(l));

            public bool IsComplete => true;

            public bool IsSolid => false;

            class Entry : IArchiveEntry, IFileInfo
            {
                readonly Lump lump;

                public Entry(Lump lump)
                {
                    this.lump = lump;
                }

                public DateTime? ArchivedTime => null;

                public string? Name => lump.Name;

                public string? SubName => null;

                public string? Path => Name;

                public int? Revision => null;

                public DateTime? CreationTime => null;

                public DateTime? LastWriteTime => null;

                public DateTime? LastAccessTime => null;

                public FileKind Kind => FileKind.ArchiveItem;

                public FileAttributes Attributes => lump.IsCompressed ? FileAttributes.Compressed : FileAttributes.Normal;

                public object? ReferenceKey => lump;

                public object? DataKey => null;

                public long Length => lump.Size;

                public StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

                public Stream Open()
                {
                    return new MemoryStream(lump.Bytes, false);
                }

                public override string ToString()
                {
                    return "/" + Name;
                }
            }
        }
    }
}
