﻿using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    /// <summary>
    /// An extension of <see cref="XmlDocument"/> that allows specifying the
    /// <see cref="BaseURI"/> property or assigning it during the lifetime
    /// of the object.
    /// </summary>
    public class BaseXmlDocument : XmlDocument
    {
        string baseUri;

        public override string BaseURI => baseUri;

        /// <summary>
        /// Creates a new instance of the document.
        /// </summary>
        /// <param name="baseUri">The value of <see cref="BaseURI"/>.</param>
        public BaseXmlDocument(string baseUri)
        {
            this.baseUri = baseUri;
        }

        /// <summary>
        /// Creates a new instance of the document.
        /// </summary>
        /// <param name="baseUri">The value of <see cref="BaseURI"/>.</param>
        /// <param name="nameTable">The name table to use for <see cref="XmlDocument.XmlDocument(XmlNameTable)"/>.</param>
        public BaseXmlDocument(string baseUri, XmlNameTable nameTable) : base(nameTable)
        {
            this.baseUri = baseUri;
        }

        /// <summary>
        /// Sets the current base URI.
        /// </summary>
        /// <param name="baseUri">The new value of <see cref="BaseURI"/>.</param>
        public void SetBaseURI(string baseUri)
        {
            this.baseUri = baseUri;
        }
    }
}
