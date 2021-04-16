using IS4.MultiArchiver.Services;
using System;
using System.Buffers;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class FileSignaturesFormat : BinaryFileFormat<IDisposable>
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

        public override TResult Match<TResult>(Stream stream, Func<IDisposable, TResult> resultFactory)
        {
            return resultFactory(null);
        }

        public static FileSignaturesFormat Create(FileSignatures.FileFormat format)
        {
            return format is FileSignatures.IFileFormatReader reader ?
                new ParsingFileSignaturesFormat(reader, format) :
                new FileSignaturesFormat(format);
        }

        class ParsingFileSignaturesFormat : FileSignaturesFormat
        {
            readonly FileSignatures.IFileFormatReader reader;

            public ParsingFileSignaturesFormat(FileSignatures.IFileFormatReader reader, FileSignatures.FileFormat format) : base(format)
            {
                this.reader = reader;
            }

            public override TResult Match<TResult>(Stream stream, Func<IDisposable, TResult> resultFactory)
            {
                using(var result = reader.Read(stream))
                {
                    return resultFactory(result);
                }
            }
        }
    }
}
