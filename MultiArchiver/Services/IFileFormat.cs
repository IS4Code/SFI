using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Services
{
    public interface IFileFormat
    {
        string GetMediaType(object value);
        string GetExtension(object value);
    }

    public interface IFileFormat<T> : IFileFormat
    {
        string GetMediaType(T value);
        string GetExtension(T value);
    }

    public interface IBinaryFileFormat : IFileFormat
    {
        int HeaderLength { get; }

        bool Match(ArraySegment<byte> header);
        bool Match(Span<byte> header);
        TResult Match<TResult>(Stream stream, IGenericFunc<TResult> resultFactory) where TResult : class;
    }

    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat
    {
        TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat : IFileFormat
    {
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, IGenericFunc<TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat
    {
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IGenericFunc<TResult>
    {
        TResult Invoke<T>(T value);
    }

    public abstract class FileFormat<T> : IFileFormat<T>
    {
        public string MediaType { get; }
        public string Extension { get; }

        public FileFormat(string mediaType, string extension)
        {
            MediaType = mediaType;
            Extension = extension;
        }

        public virtual string GetExtension(T value)
        {
            return Extension;
        }

        public virtual string GetMediaType(T value)
        {
            return MediaType;
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

    public abstract class BinaryFileFormat<T> : FileFormat<T>, IBinaryFileFormat<T>
    {
        public int HeaderLength { get; }

        public BinaryFileFormat(int headerLength, string mediaType, string extension) : base(mediaType, extension)
        {
            HeaderLength = headerLength;
        }

        public virtual bool Match(ArraySegment<byte> header)
        {
            return Match(header.AsSpan());
        }

        public abstract bool Match(Span<byte> header);

        public abstract TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;

        public TResult Match<TResult>(Stream stream, IGenericFunc<TResult> resultFactory) where TResult : class
        {
            return Match(stream, value => resultFactory.Invoke(value));
        }
    }
}
