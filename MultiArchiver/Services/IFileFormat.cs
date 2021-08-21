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

        bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector);
        bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector);
        TResult Match<TResult, TArgs>(Stream stream, object source, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IBinaryFileFormat<T> : IFileFormat<T>, IBinaryFileFormat where T : class
    {
        TResult Match<TResult, TArgs>(Stream stream, object source, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IXmlDocumentFormat : IFileFormat
    {
        string GetPublicId(object value);
        string GetSystemId(object value);
        Uri GetNamespace(object value);

        bool CheckDocument(XDocumentType docType, XmlReader rootReader);
        TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, object source, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat where T : class
    {
        string GetPublicId(T value);
        string GetSystemId(T value);
        Uri GetNamespace(T value);
        TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, object source, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IResultFactory<out TResult, in TArgs>
    {
        TResult Invoke<T>(T value, TArgs args) where T : class;
    }

    public delegate TResult ResultFactory<T, out TResult, in TArgs>(T value, TArgs args) where T : class;

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

        public virtual bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            return CheckHeader(header.AsSpan(), isBinary, encodingDetector);
        }

        public abstract bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector);

        public abstract TResult Match<TResult, TArgs>(Stream stream, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);

        public virtual TResult Match<TResult, TArgs>(Stream stream, object source, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(stream, resultFactory, args);
        }

        public TResult Match<TResult, TArgs>(Stream stream, object source, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(stream, source, resultFactory.Invoke, args);
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

        public abstract bool CheckDocument(XDocumentType docType, XmlReader rootReader);

        public abstract TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);

        public virtual TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, object source, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(reader, docType, resultFactory, args);
        }

        public TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, object source, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(reader, docType, source, resultFactory.Invoke, args);
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
