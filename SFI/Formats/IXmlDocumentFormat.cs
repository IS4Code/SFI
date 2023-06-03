using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents an instance of <see cref="IFileFormat"/> for a format
    /// based on XML.
    /// </summary>
    public interface IXmlDocumentFormat : IFileFormat
    {
        /// <summary>
        /// Returns the <c>PUBLIC</c> identifier of a document describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>A <c>PUBLIC</c> identifier based on <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the argument is not compatible with the format.
        /// </exception>
        string? GetPublicId(object value);

        /// <summary>
        /// Returns the <c>SYSTEM</c> identifier of a document describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>A <c>SYSTEM</c> identifier based on <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the argument is not compatible with the format.
        /// </exception>
        string? GetSystemId(object value);

        /// <summary>
        /// Returns the root element's namespace URI in a document describing an instance of this formats.
        /// </summary>
        /// <param name="value">An object compatible with this format.</param>
        /// <returns>The namespace URI based on <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the argument is not compatible with the format.
        /// </exception>
        Uri? GetNamespace(object value);

        /// <summary>
        /// Determines whether an XML document identified using a
        /// particular DTD and root element could
        /// be in a format represented by this instance.
        /// </summary>
        /// <param name="docType">The DTD of the document.</param>
        /// <param name="rootReader">
        /// An instance of a <see cref="XmlReader"/>
        /// pointing at the root element. This instance
        /// is not expected to be modified.
        /// </param>
        /// <returns><see langword="false"/> if the document cannot possibly be in this format, <see langword="true"/> otherwise.</returns>
        bool CheckDocument(XDocumentType? docType, XmlReader rootReader);

        /// <summary>
        /// Attempts to match this format from an XML document, producing
        /// an object that describes the media object stored in the file.
        /// The object is obtained using <paramref name="resultFactory"/>.
        /// </summary>
        /// <typeparam name="TResult">User-specified result type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <typeparam name="TArgs">User-specified arguments type passed to <paramref name="resultFactory"/>.</typeparam>
        /// <param name="reader">
        /// An instance of <see cref="XmlReader"/> that can read the whole document.
        /// The reader is pointed right before the root element.
        /// </param>
        /// <param name="docType">The DTD of the document.</param>
        /// <param name="context">Additional information relevant to the match.</param>
        /// <param name="resultFactory">A receiver object that is provided the result of the match, if any.</param>
        /// <param name="args">User-specified arguments passed to <paramref name="resultFactory"/>.</param>
        /// <returns>
        /// The result of invoking <paramref name="resultFactory"/> when given the produced object,
        /// or the default value of <typeparamref name="TResult"/> when the match isn't successful.
        /// </returns>
        /// <exception cref="Exception">
        /// Any exception may be caused during the internal parsing of the format.
        /// </exception>
        ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args);
    }

    /// <summary>
    /// Represents an instance of <see cref="IFileFormat{T}"/> for a format
    /// based on XML, producing instances of <typeparamref name="T"/>
    /// to describe the media object.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="IFileFormat{T}" path="/typeparam[@name='T']"/></typeparam>
    public interface IXmlDocumentFormat<T> : IFileFormat<T>, IXmlDocumentFormat where T : class
    {
        /// <inheritdoc cref="IXmlDocumentFormat.GetPublicId(object)"/>
        string? GetPublicId(T value);

        /// <inheritdoc cref="IXmlDocumentFormat.GetSystemId(object)"/>
        string? GetSystemId(T value);

        /// <inheritdoc cref="IXmlDocumentFormat.GetNamespace(object)"/>
        Uri? GetNamespace(T value);

        /// <inheritdoc cref="IXmlDocumentFormat.Match{TResult, TArgs}(XmlReader, XDocumentType?, MatchContext, IResultFactory{TResult, TArgs}, TArgs)"/>
        ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);
    }

    /// <summary>
    /// Provides a base implementation of <see cref="IXmlDocumentFormat{T}"/>.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="IFileFormat{T}" path="/typeparam[@name='T']"/></typeparam>
    public abstract class XmlDocumentFormat<T> : FileFormat<T>, IXmlDocumentFormat<T> where T : class
    {
        /// <summary>
        /// The common <c>PUBLIC</c> identifier, used if there is no other implementation
        /// of <see cref="GetPublicId(T)"/>.
        /// </summary>
        public string? PublicId { get; }

        /// <summary>
        /// The common <c>SYSTEM</c> identifier, used if there is no other implementation
        /// of <see cref="GetSystemId(T)"/>.
        /// </summary>
        public string? SystemId { get; }

        /// <summary>
        /// The common <c>SYSTEM</c> identifier, used if there is no other implementation
        /// of <see cref="GetSystemId(T)"/>.
        /// </summary>
        public Uri? Namespace { get; }

        /// <param name="publicId">The value of <see cref="PublicId"/>.</param>
        /// <param name="systemId">The value of <see cref="SystemId"/>.</param>
        /// <param name="namespace">The value of <see cref="Namespace"/>.</param>
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        /// <param name="extension"><inheritdoc cref="FileFormat{T}.FileFormat(string, string)" path="/param[@name='extension']"/></param>
        /// <param name="mediaType"><inheritdoc cref="FileFormat{T}.FileFormat(string, string)" path="/param[@name='mediaType']"/></param>
        public XmlDocumentFormat(string? publicId, string? systemId, Uri? @namespace, string? mediaType, string? extension) : base(mediaType, extension)
        {
            PublicId = publicId;
            SystemId = systemId;
            Namespace = @namespace;
        }

        /// <inheritdoc/>
        public virtual bool CheckDocument(XDocumentType? docType, XmlReader rootReader)
        {
            if(PublicId == null && Namespace == null)
            {
                return false;
            }
            return (PublicId != null && docType?.PublicId == PublicId) || (Namespace != null && rootReader.NamespaceURI == Namespace.AbsoluteUri);
        }

        /// <inheritdoc/>
        public abstract ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, ResultFactory<T, TResult, TArgs> resultFactory, TArgs args);

        /// <inheritdoc/>
        public ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, IResultFactory<TResult, TArgs> resultFactory, TArgs args)
        {
            return Match(reader, docType, context, resultFactory.Invoke, args);
        }

        /// <inheritdoc/>
        public virtual string? GetPublicId(T value)
        {
            return PublicId;
        }

        /// <inheritdoc/>
        public virtual string? GetSystemId(T value)
        {
            return SystemId;
        }

        /// <inheritdoc/>
        public virtual Uri? GetNamespace(T value)
        {
            return Namespace;
        }

        string? IXmlDocumentFormat.GetPublicId(object value)
        {
            if(value is not T obj) throw new ArgumentException($"The object must derive from {typeof(T)}.", nameof(value));
            return GetPublicId(obj);
        }

        string? IXmlDocumentFormat.GetSystemId(object value)
        {
            if(value is not T obj) throw new ArgumentException($"The object must derive from {typeof(T)}.", nameof(value));
            return GetSystemId(obj);
        }

        Uri? IXmlDocumentFormat.GetNamespace(object value)
        {
            if(value is not T obj) throw new ArgumentException($"The object must derive from {typeof(T)}.", nameof(value));
            return GetNamespace(obj);
        }
    }
}
