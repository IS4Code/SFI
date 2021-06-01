using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;

namespace IS4.MultiArchiver.Analyzers
{
    public class CabinetAnalyzer : BinaryFormatAnalyzer<CabinetFile>
    {
        public CabinetAnalyzer()
        {

        }

        public override string Analyze(ILinkedNode parent, ILinkedNode node, CabinetFile file, object source, ILinkedNodeFactory nodeFactory)
        {
            var obj = new LinkedObject<IReader>(node, source, new CabinetAdapter(file));
            if(nodeFactory.Create(parent, obj) != null)
            {
                return obj.Label;
            }
            return null;
        }

        class CabinetAdapter : IReader
        {
            readonly CabinetFile cabinet;

            public CabinetAdapter(CabinetFile cabinet)
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
                return SharpCompressExtensions.CreateEntryStream(this, ((CabinetEntry)Entry).Stream);
            }

            public void WriteEntryTo(Stream writableStream)
            {
                throw new NotImplementedException();
            }
            
            protected virtual void Dispose(bool disposing)
            {
                if(disposing)
                {
                    cabinet?.Dispose();
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
                readonly CabinetFile.FileInfo info;

                public Stream Stream => info.Stream;

                public CabinetEntry(CabinetFile.FileInfo info)
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
