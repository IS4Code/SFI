﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(null, args);
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

            public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IDisposable, TResult, TArgs> resultFactory, TArgs args)
            {
                using(var result = reader.Read(stream))
                {
                    return await resultFactory(result, args);
                }
            }
        }
    }
}
