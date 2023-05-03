﻿namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF classes, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Classes
    {
        /// <summary>
        /// <see cref="Schema"/>:Photograph.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri Photograph;

        /// <summary>
        /// <see cref="Schema"/>:ImageObject.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri ImageObject;

        /// <summary>
        /// <see cref="Schema"/>:AudioObject.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri AudioObject;

        /// <summary>
        /// <see cref="Schema"/>:VideoObject.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri VideoObject;

        /// <summary>
        /// <see cref="Schema"/>:MediaObject.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri MediaObject;

        /// <summary>
        /// <see cref="Schema"/>:SoftwareApplication.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri SoftwareApplication;

        /// <summary>
        /// <see cref="Schema"/>:DigitalDocument.
        /// </summary>
        [Uri(Schema)]
        public static readonly ClassUri DigitalDocument;

        /// <summary>
        /// <see cref="Cnt"/>:ContentAsText.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsText;

        /// <summary>
        /// <see cref="Cnt"/>:ContentAsBase64.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsBase64;

        /// <summary>
        /// <see cref="Cnt"/>:ContentAsXML.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri ContentAsXML;

        /// <summary>
        /// <see cref="Cnt"/>:DoctypeDecl.
        /// </summary>
        [Uri(Cnt)]
        public static readonly ClassUri DoctypeDecl;

        /// <summary>
        /// <see cref="Nfo"/>:ArchiveItem.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri ArchiveItem;

        /// <summary>
        /// <see cref="Nfo"/>:Archive.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Archive;

        /// <summary>
        /// <see cref="Nfo"/>:FileDataObject.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri FileDataObject;

        /// <summary>
        /// <see cref="Nfo"/>:Filesystem.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Filesystem;

        /// <summary>
        /// <see cref="Nfo"/>:FilesystemImage.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri FilesystemImage;

        /// <summary>
        /// <see cref="Nfo"/>:Folder.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Folder;

        /// <summary>
        /// <see cref="Nfo"/>:Executable.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Executable;

        /// <summary>
        /// <see cref="Nfo"/>:Audio.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Audio;

        /// <summary>
        /// <see cref="Nfo"/>:Image.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Image;

        /// <summary>
        /// <see cref="Nfo"/>:Video.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Video;

        /// <summary>
        /// <see cref="Nfo"/>:Document.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri Document;

        /// <summary>
        /// <see cref="Nfo"/>:MediaStream.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri MediaStream;

        /// <summary>
        /// <see cref="Nfo"/>:EmbeddedFileDataObject.
        /// </summary>
        [Uri(Nfo)]
        public static readonly ClassUri EmbeddedFileDataObject;

        /// <summary>
        /// <see cref="Nmo"/>:Message.
        /// </summary>
        [Uri(Nmo)]
        public static readonly ClassUri Message;

        /// <summary>
        /// <see cref="Nid3"/>:ID3Audio.
        /// </summary>
        [Uri(Nid3)]
        public static readonly ClassUri ID3Audio;

        /// <summary>
        /// <see cref="Exif"/>:IFD.
        /// </summary>
        [Uri(Exif)]
        public static readonly ClassUri IFD;

        /// <summary>
        /// <see cref="Xis"/>:Document.
        /// </summary>
        [Uri(Xis, "Document")]
        public static readonly ClassUri XmlDocument;

        /// <summary>
        /// <see cref="Xis"/>:Element.
        /// </summary>
        [Uri(Xis)]
        public static readonly ClassUri Element;

        /// <summary>
        /// <see cref="Sec"/>:Digest.
        /// </summary>
        [Uri(Sec)]
        public static readonly ClassUri Digest;

        /// <summary>
        /// <see cref="Cert"/>:X509Certificate.
        /// </summary>
        [Uri(Cert)]
        public static readonly ClassUri X509Certificate;

        /// <summary>
        /// <see cref="Http"/>:Request.
        /// </summary>
        [Uri(Http, "Request")]
        public static readonly ClassUri HttpRequest;

        /// <summary>
        /// <see cref="Http"/>:Response.
        /// </summary>
        [Uri(Http, "Response")]
        public static readonly ClassUri HttpResponse;

        /// <summary>
        /// <see cref="Http"/>:MessageHeader.
        /// </summary>
        [Uri(Http, "MessageHeader")]
        public static readonly ClassUri HttpMessageHeader;

        /// <summary>
        /// <see cref="Uriv"/>:Mimetype.
        /// </summary>
        [Uri(Uriv, "Mimetype")]
        public static readonly ClassUri MediaType;

        /// <summary>
        /// <see cref="Uriv"/>:Mimetype-Implied.
        /// </summary>
        [Uri(Uriv, "Mimetype-Implied")]
        public static readonly ClassUri MediaTypeImplied;

        /// <summary>
        /// <see cref="Uriv"/>:Mimetype-Structured.
        /// </summary>
        [Uri(Uriv, "Mimetype-Structured")]
        public static readonly ClassUri MediaTypeStructured;

        /// <summary>
        /// <see cref="Uriv"/>:Mimetype-Parametrized.
        /// </summary>
        [Uri(Uriv, "Mimetype-Parametrized")]
        public static readonly ClassUri MediaTypeParametrized;

        static Classes()
        {
            typeof(Classes).InitializeUris();
        }
    }
}
