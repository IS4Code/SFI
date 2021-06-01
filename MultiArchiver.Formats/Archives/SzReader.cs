using IS4.MultiArchiver.Tools;
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

        public EntryStream OpenEntryStream()
        {
            var stream = new EnumeratorStream<EnumeratorStream<Exception>.NoException>(ReadBytes(), Entry.Size);
            return SharpCompressExtensions.CreateEntryStream(this, stream);
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
            throw new NotSupportedException();
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

    }
}
