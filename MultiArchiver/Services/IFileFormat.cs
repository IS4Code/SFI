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

        bool CheckEncoding(bool isBinary, IEncodingDetector detectedEncoding);
        bool CheckHeader(ArraySegment<byte> header);
        bool CheckHeader(Span<byte> header);
        TResult Match<TResult>(Stream stream, object source, IGenericFunc<TResult> resultFactory) where TResult : class;
    }

    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat where T : class
    {
        TResult Match<TResult>(Stream stream, object source, Func<T, TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat : IFileFormat
    {
        string GetPublicId(object value);
        string GetSystemId(object value);
        Uri GetNamespace(object value);
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, object source, IGenericFunc<TResult> resultFactory) where TResult : class;
    }

    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat where T : class
    {
        string GetPublicId(T value);
        string GetSystemId(T value);
        Uri GetNamespace(T value);
        TResult Match<TResult>(XmlReader reader, XDocumentType docType, object source, Func<T, TResult> resultFactory) where TResult : class;
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

        public virtual bool CheckEncoding(bool isBinary, IEncodingDetector encodingDetector)
        {
            return true;
        }

        public virtual bool CheckHeader(ArraySegment<byte> header)
        {
            return CheckHeader(header.AsSpan());
        }

        public abstract bool CheckHeader(Span<byte> header);

        public abstract TResult Match<TResult>(Stream stream, Func<T, TResult> resultFactory) where TResult : class;

        public virtual TResult Match<TResult>(Stream stream, object source, Func<T, TResult> resultFactory) where TResult : class
        {
            return Match(stream, resultFactory);
        }

        public TResult Match<TResult>(Stream stream, object source, IGenericFunc<TResult> resultFactory) where TResult : class
        {
            return Match(stream, source, value => resultFactory.Invoke(value));
        }
    }

    public abstract class XmlDocumentFormat<T> : FileFormat<T>, IXmlDocumentFormat<T> where T : class
    {
        public string PublicId { get; }
        public string SystemId { get; }
        public Uri Namespace { get; }

        public XmlDocumentFormat(string publicId, string systemId, Uri @namespace, string mediaType, string extension) : base(mediaType, extension)
        {
            PublicId = publicId;
            SystemId = systemId;
            Namespace = @namespace;
        }

        public abstract TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<T, TResult> resultFactory) where TResult : class;

        public virtual TResult Match<TResult>(XmlReader reader, XDocumentType docType, object source, Func<T, TResult> resultFactory) where TResult : class
        {
            return Match(reader, docType, resultFactory);
        }

        public TResult Match<TResult>(XmlReader reader, XDocumentType docType, object source, IGenericFunc<TResult> resultFactory) where TResult : class
        {
            return Match(reader, docType, source, value => resultFactory.Invoke(value));
        }

        public virtual string GetPublicId(T value)
        {
            return PublicId;
        }

        public virtual string GetSystemId(T value)
        {
            return SystemId;
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

        string IXmlDocumentFormat.GetSystemId(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetSystemId(obj);
        }

        Uri IXmlDocumentFormat.GetNamespace(object value)
        {
            if(!(value is T obj)) throw new ArgumentException(null, nameof(value));
            return GetNamespace(obj);
        }
    }
}
