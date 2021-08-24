using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IBinaryFileFormat : IFileFormat
    {
        int HeaderLength { get; }

        bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector);
        bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector);
        TResult Match<TResult, TArgs>(Stream stream, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat where T : class
    {
        TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
    }

    public abstract class BinaryFileFormat<T> : FileFormat<T>, IBinaryFileFormat<T> where T : class
    {
        public int HeaderLength { get; }

        public BinaryFileFormat(int headerLength, string mediaType, string extension) : base(mediaType, extension)
        {
            HeaderLength = headerLength;
        }

        public virtual bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return CheckHeader(header.AsSpan(), isBinary, encodingDetector);
        }

        public abstract bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector);

        public abstract TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);

        public TResult Match<TResult, TArgs>(Stream stream, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(stream, context, resultFactory.Invoke, args);
        }
    }
}
