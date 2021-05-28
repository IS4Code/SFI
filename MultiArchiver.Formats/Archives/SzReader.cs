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
            var stream = new EnumeratorStream<NoException>(ReadBytes(), Entry.Size);
            return (EntryStream)ctor.Invoke(new object[] { this, stream });
        }

        abstract class NoException : Exception
        {
            private NoException()
            {

            }
        }

        const int windowSize = 4096;

        static readonly byte[] initialWindow = Enumerable.Repeat((byte)0x20, windowSize).ToArray();

        IEnumerator<ArraySegment<byte>> ReadBytes()
        {
            var stream = reader.BaseStream;
            var window = (byte[])initialWindow.Clone();
            const int invert = 0;

            int pos = windowSize - (qbFormat ? 18 : 16);

            int initialPos = pos;

            ArraySegment<byte> GetSegment(int? newPos = null)
            {
                var result = new ArraySegment<byte>(window, initialPos, pos - initialPos);
                initialPos = pos = newPos ?? pos;
                return result;
            }

            while(true)
            {
                int control = stream.ReadByte();
                if(control == -1)
                {
                    yield return GetSegment();
                    yield break;
                }
                control ^= invert;
                for(int cbit = 0x01; (cbit & 0xFF) != 0; cbit <<= 1)
                {
                    if((control & cbit) != 0)
                    {
                        int b = stream.ReadByte();
                        if(b == -1)
                        {
                            yield return GetSegment();
                            yield break;
                        }
                        window[pos++] = unchecked((byte)b);
                        if(pos >= windowSize)
                        {
                            yield return GetSegment(0);
                        }
                    }else{
                        int matchpos = stream.ReadByte();
                        int matchlen = stream.ReadByte();
                        if(matchpos == -1 || matchlen == -1)
                        {
                            yield return GetSegment();
                            yield break;
                        }
                        matchpos |= (matchlen & 0xF0) << 4;
                        matchlen = (matchlen & 0x0F) + 3;
                        while(matchlen-- != 0)
                        {
                            window[pos++] = window[matchpos++];
                            matchpos &= windowSize - 1;
                            if(pos >= windowSize)
                            {
                                yield return GetSegment(0);
                            }
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
            readonly IEnumerator<ArraySegment<byte>> enumerator;

            public EnumeratorStream(IEnumerator<ArraySegment<byte>> enumerator, long length)
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

            ArraySegment<byte> remaining;

            public override int Read(byte[] buffer, int offset, int count)
            {
                int total = 0;
                while(count > 0)
                {
                    while(remaining.Count == 0)
                    {
                        if(!MoveNext()) return total;
                    }
                    int read = Math.Min(remaining.Count, count);
                    Array.Copy(remaining.Array, remaining.Offset, buffer, offset, read);
                    remaining = new ArraySegment<byte>(remaining.Array, remaining.Offset + read, remaining.Count - read);
                    total += read;
                    offset += read;
                    count -= read;
                }
                return total;
            }

            public override int ReadByte()
            {
                while(remaining.Count == 0)
                {
                    if(!MoveNext()) return -1;
                }
                var result = remaining.Array[remaining.Offset];
                remaining = new ArraySegment<byte>(remaining.Array, remaining.Offset + 1, remaining.Count - 1);
                return result;
            }

            bool MoveNext()
            {
                try{
                    if(enumerator.MoveNext())
                    {
                        remaining = enumerator.Current;
                        return true;
                    }
                    return false;
                }catch(TException)
                {
                    return false;
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
