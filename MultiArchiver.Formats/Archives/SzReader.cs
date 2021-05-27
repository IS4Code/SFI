using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IS4.MultiArchiver.Formats.Archives
{
    public class SzReader : IReader
    {
        readonly BinaryReader reader;

        readonly bool qbFormat;

        public SzReader(Stream stream)
        {
            reader = new BinaryReader(stream, Encoding.ASCII, true);

            var sig1 = reader.ReadUInt32();
            if(sig1 == 0x44445A53)
            {
                var sig2 = reader.ReadUInt32();
                if(sig2 == 0x3327F088)
                {
                    if(reader.ReadByte() == 0x41)
                    {
                        return;
                    }
                }
            }else if(sig1 == 0x88205A53)
            {
                var sig2 = reader.ReadUInt32();
                if(sig2 == 0xD13327F0)
                {
                    qbFormat = true;
                    return;
                }
            }
            throw new ArgumentException(null, nameof(stream));
        }

        public ArchiveType ArchiveType => (ArchiveType)(-1);

        public IEntry Entry { get; private set; }

        public bool Cancelled { get; private set; }

        public event EventHandler<ReaderExtractionEventArgs<IEntry>> EntryExtractionProgress;
        public event EventHandler<CompressedBytesReadEventArgs> CompressedBytesRead;
        public event EventHandler<FilePartExtractionBeginEventArgs> FilePartExtractionBegin;

        public void Cancel()
        {
            Cancelled = true;
        }

        public void Dispose()
        {
            Cancel();
        }

        public bool MoveToNextEntry()
        {
            if(Entry == null)
            {
                string key = null;
                if(!qbFormat)
                {
                    var c = reader.ReadChar();
                    if(c != '\0') key = c.ToString();
                }
                long length = reader.ReadUInt32();
                Entry = new SzEntry(key, length);
                return true;
            }
            return false;
        }

        ConstructorInfo ctor = typeof(EntryStream).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IReader), typeof(Stream) }, null);

        public EntryStream OpenEntryStream()
        {
            var stream = new EnumeratorStream<EndOfStreamException>(ReadBytes(), Entry.Size);
            return (EntryStream)ctor.Invoke(new object[] { this, stream });
        }

        const int windowSize = 4096;

        static readonly byte[] initialWindow = Enumerable.Repeat((byte)0x20, windowSize).ToArray();

        IEnumerator<byte> ReadBytes()
        {
            var window = (byte[])initialWindow.Clone();
            const int invert = 0;

            int pos = windowSize - (qbFormat ? 18 : 16);
            while(true)
            {
                int control = reader.BaseStream.ReadByte();
                if(control == -1) yield break;
                control ^= invert;
                for(int cbit = 0x01; (cbit & 0xFF) != 0; cbit <<= 1)
                {
                    if((control & cbit) != 0)
                    {
                        yield return window[pos++] = reader.ReadByte();
                        pos &= windowSize - 1;
                    }else{
                        int matchpos = reader.ReadByte();
                        int matchlen = reader.ReadByte();
                        matchpos |= (matchlen & 0xF0) << 4;
                        matchlen = (matchlen & 0x0F) + 3;
                        while(matchlen-- != 0)
                        {
                            yield return window[pos++] = window[matchpos++];
                            pos &= windowSize - 1; matchpos &= windowSize - 1;
                        }
                    }
                }
            }
        }

        public void WriteEntryTo(Stream writableStream)
        {
            throw new NotImplementedException();
        }

        class SzEntry : IEntry
        {
            public SzEntry(string key, long size)
            {
                Key = key;
                Size = size;
            }

            public CompressionType CompressionType => CompressionType.Unknown;

            public DateTime? ArchivedTime => null;

            public long CompressedSize => 0;

            public long Crc => 0;

            public DateTime? CreatedTime => null;

            public string Key { get; }

            public string LinkTarget => null;

            public bool IsDirectory => false;

            public bool IsEncrypted => false;

            public bool IsSplitAfter => false;

            public DateTime? LastAccessedTime => null;

            public DateTime? LastModifiedTime => null;

            public long Size { get; }

            public int? Attrib => null;
        }

        class EnumeratorStream<TException> : Stream where TException : Exception
        {
            readonly IEnumerator<byte> enumerator;

            public EnumeratorStream(IEnumerator<byte> enumerator, long length)
            {
                this.enumerator = enumerator;
                Length = length;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length { get; }

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
                
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                for(int i = 0; i < count; i++)
                {
                    int read = ReadByte();
                    if(read == -1) return i;
                    buffer[offset + i] = (byte)read;
                }
                return 0;
            }

            public override int ReadByte()
            {
                try{
                    if(!enumerator.MoveNext()) return -1;
                    return enumerator.Current;
                }catch(TException)
                {
                    return -1;
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
