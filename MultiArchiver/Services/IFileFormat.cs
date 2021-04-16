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

    public interface IFileFormat<T> : IFileFormat where T : class
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

    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat where T : class
    {
        TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat : IFileFormat
    {
        string GetPublicId(object value);
        Uri GetNamespace(object value);
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, IGenericFunc<TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat where T : class
    {
        string GetPublicId(T value);
        Uri GetNamespace(T value);
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IGenericFunc<TResult>
    {
        TResult Invoke<T>(T value) where T : class;
    }

    public abstract class FileFormat<T> : IFileFormat<T> where T : class
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

    public abstract class BinaryFileFormat<T> : FileFormat<T>, IBinaryFileFormat<T> where T : class
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

    public abstract class XmlDocumentFormat<T> : FileFormat<T>, IXmlDocumentFormat<T> where T : class
    {
        public string PublicId { get; }
        public Uri Namespace { get; }

        public XmlDocumentFormat(string publicId, Uri @namespace, string mediaType, string extension) : base(mediaType, extension)
        {
            PublicId = publicId;
            Namespace = @namespace;
        }

        public abstract TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<T, TResult> resultFactory) where TResult : class;

        public TResult Match<TResult>(XmlReader reader, XDocumentType docType, IGenericFunc<TResult> resultFactory) where TResult : class
        {
            return Match(reader, docType, value => resultFactory.Invoke(value));
        }

        public virtual string GetPublicId(T value)
        {
            return PublicId;
        }

        public virtual Uri GetNamespace(T value)
        {
            return Namespace;
        }

        string IXmlDocumentFormat.GetPublicId(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetPublicId(obj);
        }

        Uri IXmlDocumentFormat.GetNamespace(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetNamespace(obj);
        }
    }
}
