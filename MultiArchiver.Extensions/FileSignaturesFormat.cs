using IS4.MultiArchiver.Services;
using System;
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

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            using(var stream = new MemoryStream(header.Array, header.Offset, header.Count, false))
            {
                return Format.IsMatch(stream);
            }
        }

        public override unsafe bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            fixed(byte* ptr = header)
            {
                using(var stream = new UnmanagedMemoryStream(ptr, header.Length, header.Length, FileAccess.Read))
                {
                    return Format.IsMatch(stream);
                }
            }
        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
        {
            return resultFactory(null, args);
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

            public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
            {
                using(var result = reader.Read(stream))
                {
                    return resultFactory(result, args);
                }
            }
        }
    }
}
