namespace IS4.MultiArchiver.Vocabulary
{
    public static class Properties
    {
        [Uri(Vocabularies.Rdf)]
        public static readonly PropertyUri Type;
        [Uri(Vocabularies.Rdf)]
        public static readonly PropertyUri Value;

        [Uri(Vocabularies.Rdfs)]
        public static readonly PropertyUri Label;
        [Uri(Vocabularies.Rdfs)]
        public static readonly PropertyUri SeeAlso;

        [Uri(Vocabularies.Owl)]
        public static readonly PropertyUri SameAs;

        [Uri(Vocabularies.Dcterms)]
        public static readonly PropertyUri Description;
        [Uri(Vocabularies.Dcterms)]
        public static readonly PropertyUri HasFormat;
        [Uri(Vocabularies.Dcterms)]
        public static readonly PropertyUri Extent;
        [Uri(Vocabularies.Dcterms)]
        public static readonly PropertyUri Creator;
        [Uri(Vocabularies.Dcterms)]
        public static readonly PropertyUri Modified;

        [Uri(Vocabularies.Dbo)]
        public static readonly PropertyUri OriginalName;

        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri Bytes;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri Chars;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri CharacterEncoding;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri Rest;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri DoctypeName;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri PublicId;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri SystemId;
        [Uri(Vocabularies.Cnt, "version")]
        public static readonly PropertyUri XmlVersion;
        [Uri(Vocabularies.Cnt, "declaredEncoding")]
        public static readonly PropertyUri XmlEncoding;
        [Uri(Vocabularies.Cnt, "standalone")]
        public static readonly PropertyUri XmlStandalone;
        [Uri(Vocabularies.Cnt)]
        public static readonly PropertyUri DtDecl;

        [Uri(Vocabularies.Foaf)]
        public static readonly PropertyUri Depicts;

        [Uri(Vocabularies.Sec)]
        public static readonly PropertyUri CanonicalizationAlgorithm;
        [Uri(Vocabularies.Sec)]
        public static readonly PropertyUri DigestAlgorithm;
        [Uri(Vocabularies.Sec)]
        public static readonly PropertyUri DigestValue;

        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri Name;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri DownloadUrl;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri Encoding;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri EncodingFormat;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri Version;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri SoftwareVersion;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri Thumbnail;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri Image;
        [Uri(Vocabularies.Schema)]
        public static readonly PropertyUri CopyrightNotice;

        [Uri(Vocabularies.Nie)]
        public static readonly PropertyUri IsStoredAs;
        [Uri(Vocabularies.Nie)]
        public static readonly PropertyUri Links;
        [Uri(Vocabularies.Nie)]
        public static readonly PropertyUri HasPart;

        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FileName;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri BelongsToContainer;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FileCreated;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FileLastAccessed;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FileLastModified;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FileSize;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FreeSpace;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri OccupiedSpace;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri TotalSpace;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri FilesystemType;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri Width;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri Height;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri BitDepth;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri ColorDepth;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri HorizontalResolution;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri VerticalResolution;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri PaletteSize;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri BitsPerSample;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri Channels;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri SampleCount;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri SampleRate;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri Duration;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri HasMediaStream;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri AverageBitrate;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri CompressionType;
        [Uri(Vocabularies.Nfo)]
        public static readonly PropertyUri EncryptionStatus;

        [Uri(Vocabularies.Skos)]
        public static readonly PropertyUri Broader;
        [Uri(Vocabularies.Skos)]
        public static readonly PropertyUri ExactMatch;
        [Uri(Vocabularies.Skos)]
        public static readonly PropertyUri CloseMatch;
        [Uri(Vocabularies.Skos)]
        public static readonly PropertyUri PrefLabel;
        [Uri(Vocabularies.Skos)]
        public static readonly PropertyUri AltLabel;

        [Uri(Vocabularies.Xis)]
        public static readonly PropertyUri DocumentElement;
        [Uri(Vocabularies.Xis)]
        public static readonly PropertyUri LocalName;
        [Uri(Vocabularies.Xis, "name")]
        public static readonly PropertyUri XmlName;
        [Uri(Vocabularies.Xis, "prefix")]
        public static readonly PropertyUri XmlPrefix;
        [Uri(Vocabularies.Xis)]
        public static readonly PropertyUri NamespaceName;

        [Uri(Vocabularies.At)]
        public static readonly PropertyUri Digest;
        [Uri(Vocabularies.At)]
        public static readonly PropertyUri Source;
        [Uri(Vocabularies.At, "prefLabel")]
        public static readonly PropertyUri AtPrefLabel;
        [Uri(Vocabularies.At, "altLabel")]
        public static readonly PropertyUri AtAltLabel;
        [Uri(Vocabularies.At)]
        public static readonly PropertyUri PathObject;
        [Uri(Vocabularies.At)]
        public static readonly PropertyUri ExtensionObject;
        [Uri(Vocabularies.At)]
        public static readonly PropertyUri VolumeLabel;
        [Uri(Vocabularies.At)]
        public static readonly PropertyUri Visited;

        static Properties()
        {
            typeof(Properties).InitializeUris();
        }
    }
}
