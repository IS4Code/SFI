using IS4.MultiArchiver.Services;
using System;
using System.Buffers;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class FileSignaturesFormat : FileFormat
    {
        public FileSignatures.FileFormat Format { get; }

        protected FileSignaturesFormat(FileSignatures.FileFormat format) : base(Math.Max(format.HeaderLength == Int32.MaxValue ? 0 : format.HeaderLength, format.Offset + format.Signature.Count), format.MediaType, format.Extension)
        {
            Format = format;
        }

        public override bool Match(Span<byte> header)
        {
            using(var stream = new MemoryStream(HeaderLength))
            {
                if(stream.TryGetBuffer(out var buffer))
                {
                    header.CopyTo(buffer);
                }else{
                    if(HeaderLength < 64)
                    {
                        foreach(var b in header)
                        {
                            stream.WriteByte(b);
                        }
                    }else{
                        byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(HeaderLength);
                        try{
                            header.CopyTo(sharedBuffer);
                            stream.Write(sharedBuffer, 0, HeaderLength);
                        }finally{
                            ArrayPool<byte>.Shared.Return(sharedBuffer);
                        }
                    }
                }
                return Format.IsMatch(stream);
            }
        }

        public static FileSignaturesFormat Create(FileSignatures.FileFormat format)
        {
            return format is FileSignatures.IFileFormatReader reader ?
                new ParsingFileSignaturesFormat(reader, format) :
                new FileSignaturesFormat(format);
        }

        class ParsingFileSignaturesFormat : FileSignaturesFormat, IFileLoader
        {
            readonly FileSignatures.IFileFormatReader reader;

            public ParsingFileSignaturesFormat(FileSignatures.IFileFormatReader reader, FileSignatures.FileFormat format) : base(format)
            {
                this.reader = reader;
            }

            public string GetExtension(object value)
            {
                return Extension;
            }

            public string GetMediaType(object value)
            {
                return MediaType;
            }

            public object Match(Stream stream)
            {
                var result = reader.Read(stream);
                bool matches = false;
                try{
                    if(reader.IsMatch(result))
                    {
                        matches = true;
                        return result;
                    }
                    return null;
                }finally{
                    if(!matches) result.Dispose();
                }
            }
        }
    }
}
