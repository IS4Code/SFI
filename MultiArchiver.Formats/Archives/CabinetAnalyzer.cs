using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Analyzers
{
    /// <summary>
    /// Analyzes Cabinet archives, as instances of <see cref="ICabinetArchive"/>.
    /// The analysis itself is performed by analyzing <see cref="IArchiveReader"/>
    /// on an adapted instance of the archive.
    /// </summary>
    public class CabinetAnalyzer : MediaObjectAnalyzer<ICabinetArchive>
    {
        public override ValueTask<AnalysisResult> Analyze(ICabinetArchive file, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            return analyzers.Analyze(new ArchiveReaderAdapter(new CabinetAdapter(file)), context);
        }

        class CabinetAdapter : IReader
        {
            readonly ICabinetArchive cabinet;

            public CabinetAdapter(ICabinetArchive cabinet)
            {
                this.cabinet = cabinet;
            }

            public ArchiveType ArchiveType => (ArchiveType)(-1);

            public IEntry Entry { get; private set; }

            public bool Cancelled { get; private set; }

            public void Cancel()
            {
                Cancelled = true;
            }

            public bool MoveToNextEntry()
            {
                var file = cabinet.GetNextFile();
                if(file != null)
                {
                    Entry = new CabinetEntry(file);
                    return true;
                }
                Entry = null;
                return false;
            }

            public EntryStream OpenEntryStream()
            {
                return this.CreateEntryStream(((CabinetEntry)Entry).Stream);
            }

            public void WriteEntryTo(Stream writableStream)
            {
                throw new NotImplementedException();
            }
            
            protected virtual void Dispose(bool disposing)
            {
                if(disposing && cabinet is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            event EventHandler<ReaderExtractionEventArgs<IEntry>> IReader.EntryExtractionProgress {
                add {
                    throw new NotSupportedException();
                }

                remove {
                    throw new NotSupportedException();
                }
            }

            event EventHandler<CompressedBytesReadEventArgs> IReader.CompressedBytesRead {
                add {
                    throw new NotSupportedException();
                }

                remove {
                    throw new NotSupportedException();
                }
            }

            event EventHandler<FilePartExtractionBeginEventArgs> IReader.FilePartExtractionBegin {
                add {
                    throw new NotSupportedException();
                }

                remove {
                    throw new NotSupportedException();
                }
            }

            class CabinetEntry : IEntry
            {
                readonly ICabinetArchiveFile info;

                public Stream Stream => info.Stream;

                public CabinetEntry(ICabinetArchiveFile info)
                {
                    this.info = info;
                }

                public CompressionType CompressionType => CompressionType.Unknown;

                public DateTime? ArchivedTime => null;

                public long CompressedSize => 0;

                public long Crc => 0;

                public DateTime? CreatedTime => null;

                public string Key => info.Name;

                public string LinkTarget => null;

                public bool IsDirectory => false;

                public bool IsEncrypted => false;

                public bool IsSplitAfter => false;

                public DateTime? LastAccessedTime => null;

                public DateTime? LastModifiedTime => info.Date;

                public long Size => info.Size;

                public int? Attrib => (int)info.Attributes;
            }
        }
    }
}
