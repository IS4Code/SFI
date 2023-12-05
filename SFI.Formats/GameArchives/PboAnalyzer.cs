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
            var result = await analyzers.Analyze<IArchiveFile>(new Adapter(pbo), context);
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

        class Adapter : IArchiveFile, IFileNodeInfo
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

            string? IFileNodeInfo.Name => prefix;

            string? IFileNodeInfo.SubName => null;

            string? IFileNodeInfo.Path => prefix;

            int? IFileNodeInfo.Revision => null;

            DateTime? IFileNodeInfo.CreationTime => null;

            DateTime? IFileNodeInfo.LastWriteTime => null;

            DateTime? IFileNodeInfo.LastAccessTime => null;

            FileKind IFileNodeInfo.Kind => FileKind.None;

            FileAttributes IFileNodeInfo.Attributes => FileAttributes.Directory;

            object? IIdentityKey.ReferenceKey => pbo;

            object? IIdentityKey.DataKey => null;

            class Entry : ArchiveFileWrapper<PboDataEntry>
            {
                readonly Lazy<byte[]> data;

                public Entry(Adapter parent, PboDataEntry entry) : base(entry)
                {
                    data = new Lazy<byte[]>(() => parent.pbo.GetEntryData(entry));
                    Path = (parent.prefix != null ? $"{parent.prefix}/{entry.EntryName}" : entry.EntryName)?.Replace('\\', '/');
                }

                public override string? Path { get; }

                public override DateTime? LastWriteTime => Entry.TimeStamp is 0 or 1 ? null : DateTimeOffset.FromUnixTimeSeconds(unchecked((long)Entry.TimeStamp)).DateTime;

                public override FileAttributes Attributes {
                    get {
                        switch(Entry.EntryMagic)
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

                protected override object? ReferenceKey => Entry;

                protected override object? DataKey => null;

                public override long Length => Entry.OriginalSize != 0 ? unchecked((long)Entry.OriginalSize) : Entry.EntryData.Length;

                public override StreamFactoryAccess Access => StreamFactoryAccess.Parallel;

                public override Stream Open()
                {
                    return new MemoryStream(data.Value, false);
                }
            }
            
            class Directory : ArchiveDirectoryWrapper<Adapter, PboDataEntry>
            {
                public Directory(Adapter parent, string? path, IEntryGrouping entries) : base(parent, null, path, entries)
                {

                }

                protected override bool IsValidFile(PboDataEntry? entry)
                {
                    return entry != null;
                }

                protected override ArchiveFileWrapper<PboDataEntry> CreateFileWrapper(PboDataEntry entry)
                {
                    return new Entry(Archive, entry);
                }

                protected override ArchiveDirectoryWrapper<Adapter, PboDataEntry> CreateDirectoryWrapper(string path, IEntryGrouping entries)
                {
                    return new Directory(Archive, path, entries);
                }
            }
        }
    }
}
