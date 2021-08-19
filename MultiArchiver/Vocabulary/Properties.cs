namespace IS4.MultiArchiver.Vocabulary
{
    public static class Properties
    {
        [Uri(Vocabularies.Uri.Rdf)]
        public static readonly PropertyUri Type;
        [Uri(Vocabularies.Uri.Rdf)]
        public static readonly PropertyUri Value;

        [Uri(Vocabularies.Uri.Rdfs)]
        public static readonly PropertyUri Label;
        [Uri(Vocabularies.Uri.Rdfs)]
        public static readonly PropertyUri SeeAlso;

        [Uri(Vocabularies.Uri.Owl)]
        public static readonly PropertyUri SameAs;

        [Uri(Vocabularies.Uri.Dcterms)]
        public static readonly PropertyUri Description;
        [Uri(Vocabularies.Uri.Dcterms)]
        public static readonly PropertyUri HasFormat;
        [Uri(Vocabularies.Uri.Dcterms)]
        public static readonly PropertyUri Extent;
        [Uri(Vocabularies.Uri.Dcterms)]
        public static readonly PropertyUri Creator;
        [Uri(Vocabularies.Uri.Dcterms)]
        public static readonly PropertyUri Modified;

        [Uri(Vocabularies.Uri.Dbo)]
        public static readonly PropertyUri OriginalName;

        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri Bytes;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri Chars;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri CharacterEncoding;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri Rest;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri DoctypeName;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri PublicId;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri SystemId;
        [Uri(Vocabularies.Uri.Cnt, "version")]
        public static readonly PropertyUri XmlVersion;
        [Uri(Vocabularies.Uri.Cnt, "declaredEncoding")]
        public static readonly PropertyUri XmlEncoding;
        [Uri(Vocabularies.Uri.Cnt, "standalone")]
        public static readonly PropertyUri XmlStandalone;
        [Uri(Vocabularies.Uri.Cnt)]
        public static readonly PropertyUri DtDecl;

        [Uri(Vocabularies.Uri.Foaf)]
        public static readonly PropertyUri Depicts;

        [Uri(Vocabularies.Uri.Sec)]
        public static readonly PropertyUri CanonicalizationAlgorithm;
        [Uri(Vocabularies.Uri.Sec)]
        public static readonly PropertyUri DigestAlgorithm;
        [Uri(Vocabularies.Uri.Sec)]
        public static readonly PropertyUri DigestValue;

        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri Name;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri DownloadUrl;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri Encoding;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri EncodingFormat;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri Version;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri SoftwareVersion;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri Thumbnail;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri Image;
        [Uri(Vocabularies.Uri.Schema)]
        public static readonly PropertyUri CopyrightNotice;

        [Uri(Vocabularies.Uri.Nie)]
        public static readonly PropertyUri IsStoredAs;
        [Uri(Vocabularies.Uri.Nie)]
        public static readonly PropertyUri Links;
        [Uri(Vocabularies.Uri.Nie)]
        public static readonly PropertyUri HasPart;

        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FileName;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri BelongsToContainer;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FileCreated;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FileLastAccessed;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FileLastModified;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FileSize;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FreeSpace;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri OccupiedSpace;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri TotalSpace;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri FilesystemType;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri Width;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri Height;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri BitDepth;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri ColorDepth;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri HorizontalResolution;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri VerticalResolution;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri PaletteSize;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri BitsPerSample;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri Channels;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri SampleCount;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri SampleRate;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri Duration;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri HasMediaStream;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri AverageBitrate;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri CompressionType;
        [Uri(Vocabularies.Uri.Nfo)]
        public static readonly PropertyUri EncryptionStatus;

        [Uri(Vocabularies.Uri.Skos)]
        public static readonly PropertyUri Broader;
        [Uri(Vocabularies.Uri.Skos)]
        public static readonly PropertyUri ExactMatch;
        [Uri(Vocabularies.Uri.Skos)]
        public static readonly PropertyUri CloseMatch;
        [Uri(Vocabularies.Uri.Skos)]
        public static readonly PropertyUri PrefLabel;
        [Uri(Vocabularies.Uri.Skos)]
        public static readonly PropertyUri AltLabel;

        [Uri(Vocabularies.Uri.Xis)]
        public static readonly PropertyUri DocumentElement;
        [Uri(Vocabularies.Uri.Xis)]
        public static readonly PropertyUri LocalName;
        [Uri(Vocabularies.Uri.Xis, "name")]
        public static readonly PropertyUri XmlName;
        [Uri(Vocabularies.Uri.Xis, "prefix")]
        public static readonly PropertyUri XmlPrefix;
        [Uri(Vocabularies.Uri.Xis)]
        public static readonly PropertyUri NamespaceName;

        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri Digest;
        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri Source;
        [Uri(Vocabularies.Uri.At, "prefLabel")]
        public static readonly PropertyUri AtPrefLabel;
        [Uri(Vocabularies.Uri.At, "altLabel")]
        public static readonly PropertyUri AtAltLabel;
        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri PathObject;
        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri ExtensionObject;
        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri VolumeLabel;
        [Uri(Vocabularies.Uri.At)]
        public static readonly PropertyUri Visited;

        static Properties()
        {
            typeof(Properties).InitializeUris();
        }
    }
}
