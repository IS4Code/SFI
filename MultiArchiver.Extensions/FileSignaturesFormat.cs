using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// A format storing an instance of <see cref="FileSignatures.FileFormat"/>.
    /// </summary>
    public class FileSignaturesFormat : BinaryFileFormat<IDisposable>
    {
        public FileSignatures.FileFormat Format { get; }

        /// <param name="format">The value of <see cref="Format"/>.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        protected FileSignaturesFormat(FileSignatures.FileFormat format) : base(Math.Max(format.HeaderLength == Int32.MaxValue ? 0 : format.HeaderLength, format.Offset + format.Signature.Count), format.MediaType, format.Extension)
        {
            Format = format;
        }

        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            using(var stream = header.AsStream(false))
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

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(null, args);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSignaturesFormat"/>.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <returns>A new instance enclosing <paramref name="format"/>.</returns>
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

            public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
            {
                using(var result = reader.Read(stream))
                {
                    return await resultFactory(result, args);
                }
            }
        }
    }
}
