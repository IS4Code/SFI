using System;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Formats
{
    public interface IXmlDocumentFormat : IFileFormat
    {
        string GetPublicId(object value);
        string GetSystemId(object value);
        Uri GetNamespace(object value);

        bool CheckDocument(XDocumentType docType, XmlReader rootReader);
        TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat where T : class
    {
        string GetPublicId(T value);
        string GetSystemId(T value);
        Uri GetNamespace(T value);
        TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
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

        public virtual bool CheckDocument(XDocumentType docType, XmlReader rootReader)
        {
            if(PublicId == null && Namespace == null)
            {
                return false;
            }
            return (PublicId != null && docType.PublicId == PublicId) || (Namespace != null && rootReader.NamespaceURI == Namespace.AbsoluteUri);
        }

        public abstract TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
        
        public TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(reader, docType, context, resultFactory.Invoke, args);
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
