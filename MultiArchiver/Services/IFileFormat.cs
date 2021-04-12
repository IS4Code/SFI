using System;
using System.IO;

namespace IS4.MultiArchiver.Services
{
    public interface IFileFormat
    {
        int HeaderLength { get; }

        string MediaType { get; }
        string Extension { get; }

        bool Match(ArraySegment<byte> header);
        bool Match(Span<byte> header);
    }

    public interface IFileLoader : IFileFormat
    {
        string GetMediaType(object value);
        string GetExtension(object value);

        object Match(Stream stream);
    }

    public interface IFileReader : IFileFormat
    {
        ILinkedNode Match(Stream stream, ILinkedNode parent, ILinkedNodeFactory nodeFactory);
    }

    public abstract class FileFormat : IFileFormat
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
    }

    public abstract class FileLoader<T> : FileFormat, IFileLoader
    {
        public FileLoader(int headerLength, string mediaType, string extension) : base(headerLength, mediaType, extension)
        {

        }

        public virtual string GetExtension(T value)
        {
            return Extension;
        }

        public virtual string GetMediaType(T value)
        {
            return MediaType;
        }

        public abstract T Match(Stream stream);

        string IFileLoader.GetExtension(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetExtension(obj);
        }

        string IFileLoader.GetMediaType(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetMediaType(obj);
        }

        object IFileLoader.Match(Stream stream)
        {
            return Match(stream);
        }
    }
}
