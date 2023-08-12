using Hst.Compression.Lha;
using IS4.SFI.Formats.Archives;
using IS4.SFI.Services;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the LHA archive format.
    /// </summary>
    [Description("Represents the LHA archive format.")]
    public class LhaFormat : SignatureFormat<IArchiveReader>
    {
        readonly LhaOptions options = new();

        /// <summary>
        /// Contains the encoding used for reading file names.
        /// </summary>
        [Description("Contains the encoding used for reading file names.")]
        public Encoding DefaultEncoding { get => options.Encoding; set => options.Encoding = value; }

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public LhaFormat() : base(headerLength, "application/x-lha", "lha")
        {

        }

        const int headerLength = 8;

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(header.Length < headerLength) return false;
            if(header[2] != (byte)'-' || header[6] != (byte)'-') return false;
            return true;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var header = new Header(options);
            if(await header.GetHeader(stream) == null)
            {
                return default;
            }
            return await resultFactory(new ArchiveReaderAdapter(new Adapter(header, stream)), args);
        }

        class Adapter : IReader
        {
            readonly Header header;
            readonly Stream stream;

            ArchiveEntry? entry;

            public Adapter(Header header, Stream stream)
            {
                this.header = header;
                this.stream = stream;
            }

            public ArchiveType ArchiveType => (ArchiveType)(-1);

            public IEntry Entry => entry ?? throw new InvalidOperationException("There is no remaining entry in the archive.");

            public bool Cancelled { get; private set; }

            public void Cancel()
            {
                Cancelled = true;
            }

            event EventHandler<ReaderExtractionEventArgs<IEntry>> IReader.EntryExtractionProgress {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            event EventHandler<CompressedBytesReadEventArgs> IReader.CompressedBytesRead {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            event EventHandler<FilePartExtractionBeginEventArgs> IReader.FilePartExtractionBegin {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public void Dispose()
            {
                stream.Dispose();
            }

            public bool MoveToNextEntry()
            {
                if(entry != null)
                {
                    stream.Position = entry.header.HeaderOffset + entry.header.HeaderSize + entry.header.PackedSize;
                }else{
                    stream.Position = 0;
                }
                var lzheader = ReadHeader();
                if(lzheader == null)
                {
                    entry = null;
                    return false;
                }
                entry = new ArchiveEntry(lzheader);
                return true;
            }

            public EntryStream OpenEntryStream()
            {
                var buffer = new MemoryStream();
                LhExt.ExtractOne(stream, buffer, entry?.header);
                return this.CreateEntryStream(buffer);
            }

            public void WriteEntryTo(Stream writableStream)
            {
                throw new NotImplementedException();
            }

            LzHeader? ReadHeader()
            {
                try{
                    var task = header.GetHeader(stream);
                    if(task.Status == TaskStatus.RanToCompletion)
                    {
                        return task.Result;
                    }else if(task.IsFaulted)
                    {
                        if(task.Exception.InnerExceptions.Count == 1)
                        {
                            ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                        }
                        throw task.Exception;
                    }else{
                        return task.Result;
                    }
                }catch(AggregateException agg) when(agg.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(agg.InnerException).Throw();
                    throw;
                }
            }

            class ArchiveEntry : IEntry
            {
                public readonly LzHeader header;

                public ArchiveEntry(LzHeader header)
                {
                    this.header = header;
                }

                public CompressionType CompressionType => header.Method == Constants.LZHUFF0_METHOD ? CompressionType.None : CompressionType.Unknown;

                public DateTime? ArchivedTime => null;

                public long CompressedSize => header.PackedSize;

                public long Crc => header.Crc;

                public DateTime? CreatedTime => null;

                public string Key => header.Name;

                public string? LinkTarget => null;

                public bool IsDirectory => false;

                public bool IsEncrypted => false;

                public bool IsSplitAfter => false;

                public bool IsSolid => false;

                public int VolumeIndexFirst => 0;

                public int VolumeIndexLast => 0;

                public DateTime? LastAccessedTime => null;

                public DateTime? LastModifiedTime => header.UnixLastModifiedStamp;

                public long Size => header.OriginalSize;

                public int? Attrib => header.Attribute;
            }
        }
    }
}
