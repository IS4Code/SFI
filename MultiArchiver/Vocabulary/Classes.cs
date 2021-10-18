namespace IS4.MultiArchiver.Vocabulary
{
    public static class Classes
    {
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri Photograph;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri ImageObject;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri AudioObject;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri VideoObject;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri MediaObject;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly ClassUri SoftwareApplication;

        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly ClassUri ContentAsText;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly ClassUri ContentAsBase64;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly ClassUri ContentAsXML;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly ClassUri DoctypeDecl;

        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri ArchiveItem;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Archive;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri FileDataObject;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri FilesystemImage;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Folder;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Executable;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Audio;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Image;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri Video;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri MediaStream;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly ClassUri EmbeddedFileDataObject;
        [Uri(Vocabularies.Uri.Nid3)]
        public static readonly ClassUri ID3Audio;

        [Uri(Vocabularies.Uri.Xis)]
        public static readonly ClassUri Document;
        [Uri(Vocabularies.Uri.Xis)]
        public static readonly ClassUri Element;

        [Uri(Vocabularies.Uri.Sec)]
        public static readonly ClassUri Digest;

        [Uri(Vocabularies.Uri.Cert)]
        public static readonly ClassUri X509Certificate;

        [Uri(Vocabularies.Uri.At)]
        public static readonly ClassUri Root;

        static Classes()
        {
            typeof(Classes).InitializeUris();
        }
    }
}
