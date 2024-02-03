namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF classes, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Classes
    {
        #region schema
        /// <summary>
        /// <c><see cref="Schema"/>:Photograph</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri Photograph;

        /// <summary>
        /// <c><see cref="Schema"/>:ImageObject</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri ImageObject;

        /// <summary>
        /// <c><see cref="Schema"/>:AudioObject</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri AudioObject;

        /// <summary>
        /// <c><see cref="Schema"/>:VideoObject</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri VideoObject;

        /// <summary>
        /// <c><see cref="Schema"/>:MediaObject</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri MediaObject;

        /// <summary>
        /// <c><see cref="Schema"/>:SoftwareApplication</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri SoftwareApplication;

        /// <summary>
        /// <c><see cref="Schema"/>:DigitalDocument</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri DigitalDocument;
        #endregion

        #region cnt
        /// <summary>
        /// <c><see cref="Cnt"/>:ContentAsText</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsText;

        /// <summary>
        /// <c><see cref="Cnt"/>:ContentAsBase64</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsBase64;

        /// <summary>
        /// <c><see cref="Cnt"/>:ContentAsXML</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsXML;

        /// <summary>
        /// <c><see cref="Cnt"/>:DoctypeDecl</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri DoctypeDecl;
        #endregion

        #region nfo
        /// <summary>
        /// <c><see cref="Nfo"/>:ArchiveItem</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri ArchiveItem;

        /// <summary>
        /// <c><see cref="Nfo"/>:Archive</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Archive;

        /// <summary>
        /// <c><see cref="Nfo"/>:FileDataObject</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri FileDataObject;

        /// <summary>
        /// <c><see cref="Nfo"/>:Filesystem</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Filesystem;

        /// <summary>
        /// <c><see cref="Nfo"/>:FilesystemImage</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri FilesystemImage;

        /// <summary>
        /// <c><see cref="Nfo"/>:Folder</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Folder;

        /// <summary>
        /// <c><see cref="Nfo"/>:Executable</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Executable;

        /// <summary>
        /// <c><see cref="Nfo"/>:Audio</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Audio;

        /// <summary>
        /// <c><see cref="Nfo"/>:Image</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Image;

        /// <summary>
        /// <c><see cref="Nfo"/>:Video</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Video;

        /// <summary>
        /// <c><see cref="Nfo"/>:Document</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Document;

        /// <summary>
        /// <c><see cref="Nfo"/>:MediaStream</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri MediaStream;

        /// <summary>
        /// <c><see cref="Nfo"/>:EmbeddedFileDataObject</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri EmbeddedFileDataObject;
        #endregion

        /// <summary>
        /// <c><see cref="Nmo"/>:Message</c>.
        /// </summary>
        [Uri(Nmo)]
        public static readonly ClassUri Message;

        /// <summary>
        /// <c><see cref="Nid3"/>:ID3Audio</c>.
        /// </summary>
        [Uri(Nid3)]
        public static readonly ClassUri ID3Audio;

        /// <summary>
        /// <c><see cref="Exif"/>:IFD</c>.
        /// </summary>
        [Uri(Exif)]
        public static readonly ClassUri IFD;

        #region xis
        /// <summary>
        /// <c><see cref="Xis"/>:Document</c>.
        /// </summary>
        [Uri(Xis, "Document")]
        public static readonly ClassUri XmlDocument;

        /// <summary>
        /// <c><see cref="Xis"/>:Element</c>.
        /// </summary>
        [Uri(Xis, "Element")]
        public static readonly ClassUri XmlElement;

        /// <summary>
        /// <c><see cref="Xis"/>:Attribute</c>.
        /// </summary>
        [Uri(Xis, "Attribute")]
        public static readonly ClassUri XmlAttribute;

        /// <summary>
        /// <c><see cref="Xis"/>:ProcessingInstruction</c>.
        /// </summary>
        [Uri(Xis, "ProcessingInstruction")]
        public static readonly ClassUri XmlProcessingInstruction;

        /// <summary>
        /// <c><see cref="Xis"/>:Character</c>.
        /// </summary>
        [Uri(Xis, "Character")]
        public static readonly ClassUri XmlCharacter;

        /// <summary>
        /// <c><see cref="Xis"/>:UnexpandedEntityReference</c>.
        /// </summary>
        [Uri(Xis, "UnexpandedEntityReference")]
        public static readonly ClassUri XmlUnexpandedEntityReference;

        /// <summary>
        /// <c><see cref="Xis"/>:Comment</c>.
        /// </summary>
        [Uri(Xis, "Comment")]
        public static readonly ClassUri XmlComment;

        /// <summary>
        /// <c><see cref="Xis"/>:DocumentTypeDeclaration</c>.
        /// </summary>
        [Uri(Xis, "DocumentTypeDeclaration")]
        public static readonly ClassUri XmlDocumentTypeDeclaration;

        /// <summary>
        /// <c><see cref="Xis"/>:UnparsedEntity</c>.
        /// </summary>
        [Uri(Xis, "UnparsedEntity")]
        public static readonly ClassUri XmlUnparsedEntity;

        /// <summary>
        /// <c><see cref="Xis"/>:Notation</c>.
        /// </summary>
        [Uri(Xis, "Notation")]
        public static readonly ClassUri XmlNotation;

        /// <summary>
        /// <c><see cref="Xis"/>:Namespace</c>.
        /// </summary>
        [Uri(Xis, "Namespace")]
        public static readonly ClassUri XmlNamespace;
        #endregion

        /// <summary>
        /// <c><see cref="Sec"/>:Digest</c>.
        /// </summary>
        [Uri(Sec)]
        public static readonly ClassUri Digest;

        /// <summary>
        /// <c><see cref="Cert"/>:X509Certificate</c>.
        /// </summary>
        [Uri(Cert)]
        public static readonly ClassUri X509Certificate;

        #region http
        /// <summary>
        /// <c><see cref="Http"/>:Request</c>.
        /// </summary>
        [Uri(Http, "Request")]
        public static readonly ClassUri HttpRequest;

        /// <summary>
        /// <c><see cref="Http"/>:Response</c>.
        /// </summary>
        [Uri(Http, "Response")]
        public static readonly ClassUri HttpResponse;

        /// <summary>
        /// <c><see cref="Http"/>:MessageHeader</c>.
        /// </summary>
        [Uri(Http, "MessageHeader")]
        public static readonly ClassUri HttpMessageHeader;
        #endregion

        #region uriv
        /// <summary>
        /// <c><see cref="Uriv"/>:Mimetype</c>.
        /// </summary>
        [Uri(Uriv, "Mimetype")]
        public static readonly ClassUri MediaType;

        /// <summary>
        /// <c><see cref="Uriv"/>:Mimetype-Implied</c>.
        /// </summary>
        [Uri(Uriv, "Mimetype-Implied")]
        public static readonly ClassUri MediaTypeImplied;

        /// <summary>
        /// <c><see cref="Uriv"/>:Mimetype-Structured</c>.
        /// </summary>
        [Uri(Uriv, "Mimetype-Structured")]
        public static readonly ClassUri MediaTypeStructured;

        /// <summary>
        /// <c><see cref="Uriv"/>:Mimetype-Parametrized</c>.
        /// </summary>
        [Uri(Uriv, "Mimetype-Parametrized")]
        public static readonly ClassUri MediaTypeParametrized;

        /// <summary>
        /// <c><see cref="Uriv"/>:Suffix</c>.
        /// </summary>
        [Uri(Uriv, "Suffix")]
        public static readonly ClassUri Extension;
        #endregion

        static Classes()
        {
            typeof(Classes).InitializeUris();
        }
    }
}
