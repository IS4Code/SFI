using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileFormat
    {
        int HeaderLength { get; }

        string GetMediaType(object value);
        string GetExtension(object value);

        bool Match(ArraySegment<byte> header);
        bool Match(Span<byte> header);
        TResult Match<TResult>(Stream stream, IFileReadingResultFactory<TResult> resultFactory) where TResult : class;
    }

    public interface IFileFormat<T> : IFileFormat
    {
        string GetMediaType(T value);
        string GetExtension(T value);

        TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IFileReadingResultFactory<TResult>
    {
        TResult Read<T>(T value);
    }

    public abstract class FileFormat<T> : IFileFormat<T>
    {
        public int HeaderLength { get; }
        public string MediaType { get; }
        public string Extension { get; }

        public FileFormat(int headerLength, string mediaType, string extension)
        {
            HeaderLength = headerLength;
            MediaType = mediaType;
            Extension = extension;
        }

        public virtual bool Match(ArraySegment<byte> header)
        {
            return Match(header.AsSpan());
        }

        public abstract bool Match(Span<byte> header);

        public virtual string GetExtension(T value)
        {
            return Extension;
        }

        public virtual string GetMediaType(T value)
        {
            return MediaType;
        }

        public abstract TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;

        public TResult Match<TResult>(Stream stream, IFileReadingResultFactory<TResult> resultFactory) where TResult : class
        {
            return Match(stream, value => resultFactory.Read(value));
        }

        string IFileFormat.GetExtension(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetExtension(obj);
        }

        string IFileFormat.GetMediaType(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetMediaType(obj);
        }
    }
}
