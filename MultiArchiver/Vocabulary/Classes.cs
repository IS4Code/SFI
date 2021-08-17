namespace IS4.MultiArchiver.Vocabulary
{
    public static class Classes
    {
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri Photograph;
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri ImageObject;
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri AudioObject;
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri VideoObject;
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri MediaObject;
        [Uri(Vocabularies.Schema)]
        public static readonly ClassUri SoftwareApplication;

        [Uri(Vocabularies.Cnt)]
        public static readonly ClassUri ContentAsText;
        [Uri(Vocabularies.Cnt)]
        public static readonly ClassUri ContentAsBase64;
        [Uri(Vocabularies.Cnt)]
        public static readonly ClassUri ContentAsXML;
        [Uri(Vocabularies.Cnt)]
        public static readonly ClassUri DoctypeDecl;

        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri ArchiveItem;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Archive;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri FileDataObject;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri FilesystemImage;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Folder;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Executable;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Audio;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Image;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri Video;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri MediaStream;
        [Uri(Vocabularies.Nfo)]
        public static readonly ClassUri EmbeddedFileDataObject;
        [Uri(Vocabularies.Nid3)]
        public static readonly ClassUri ID3Audio;

        [Uri(Vocabularies.Xis)]
        public static readonly ClassUri Document;
        [Uri(Vocabularies.Xis)]
        public static readonly ClassUri Element;

        [Uri(Vocabularies.Sec)]
        public static readonly ClassUri Digest;

        [Uri(Vocabularies.At)]
        public static readonly ClassUri Root;

        static Classes()
        {
            typeof(Classes).InitializeUris();
        }
    }
}
