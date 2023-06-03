namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF classes, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Classes
    {
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

        /// <summary>
        /// <c><see cref="Xis"/>:Document</c>.
        /// </summary>
        [Uri(Xis, "Document")]
        public static readonly ClassUri XmlDocument;

        /// <summary>
        /// <c><see cref="Xis"/>:Element</c>.
        /// </summary>
        [Uri(Xis)]
        public static readonly ClassUri Element;

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

        static Classes()
        {
            typeof(Classes).InitializeUris();
        }
    }
}
