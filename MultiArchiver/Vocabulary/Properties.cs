namespace IS4.MultiArchiver.Vocabulary
{
    public enum Properties
    {
        [Uri(Vocabularies.Rdf)]
        Type,
        [Uri(Vocabularies.Rdf)]
        Value,

        [Uri(Vocabularies.Rdfs)]
        Label,
        [Uri(Vocabularies.Rdfs)]
        SeeAlso,

        [Uri(Vocabularies.Owl)]
        SameAs,

        [Uri(Vocabularies.Dcterms)]
        Description,
        [Uri(Vocabularies.Dcterms)]
        HasFormat,
        [Uri(Vocabularies.Dcterms)]
        Extent,

        [Uri(Vocabularies.Cnt)]
        Bytes,
        [Uri(Vocabularies.Cnt)]
        Chars,
        [Uri(Vocabularies.Cnt)]
        CharacterEncoding,
        [Uri(Vocabularies.Cnt)]
        Rest,
        [Uri(Vocabularies.Cnt)]
        DoctypeName,
        [Uri(Vocabularies.Cnt)]
        PublicId,
        [Uri(Vocabularies.Cnt)]
        SystemId,
        [Uri(Vocabularies.Cnt, "version")]
        XmlVersion,
        [Uri(Vocabularies.Cnt, "declaredEncoding")]
        XmlEncoding,
        [Uri(Vocabularies.Cnt, "standalone")]
        XmlStandalone,
        [Uri(Vocabularies.Cnt)]
        DtDecl,

        [Uri(Vocabularies.Foaf)]
        Depicts,

        [Uri(Vocabularies.Sec)]
        CanonicalizationAlgorithm,
        [Uri(Vocabularies.Sec)]
        DigestAlgorithm,
        [Uri(Vocabularies.Sec)]
        DigestValue,

        [Uri(Vocabularies.Schema)]
        DownloadUrl,
        [Uri(Vocabularies.Schema)]
        Encoding,
        [Uri(Vocabularies.Schema)]
        EncodingFormat,
        [Uri(Vocabularies.Schema)]
        Version,
        [Uri(Vocabularies.Schema)]
        Thumbnail,
        [Uri(Vocabularies.Schema)]
        Image,

        [Uri(Vocabularies.Nie)]
        IsStoredAs,
        [Uri(Vocabularies.Nie)]
        Links,
        [Uri(Vocabularies.Nie)]
        HasPart,

        [Uri(Vocabularies.Nfo)]
        FileName,
        [Uri(Vocabularies.Nfo)]
        BelongsToContainer,
        [Uri(Vocabularies.Nfo)]
        FileCreated,
        [Uri(Vocabularies.Nfo)]
        FileLastAccessed,
        [Uri(Vocabularies.Nfo)]
        FileLastModified,
        [Uri(Vocabularies.Nfo)]
        FileSize,
        [Uri(Vocabularies.Nfo)]
        FreeSpace,
        [Uri(Vocabularies.Nfo)]
        OccupiedSpace,
        [Uri(Vocabularies.Nfo)]
        TotalSpace,
        [Uri(Vocabularies.Nfo)]
        FilesystemType,
        [Uri(Vocabularies.Nfo)]
        Width,
        [Uri(Vocabularies.Nfo)]
        Height,
        [Uri(Vocabularies.Nfo)]
        BitDepth,
        [Uri(Vocabularies.Nfo)]
        ColorDepth,
        [Uri(Vocabularies.Nfo)]
        HorizontalResolution,
        [Uri(Vocabularies.Nfo)]
        VerticalResolution,
        [Uri(Vocabularies.Nfo)]
        BitsPerSample,
        [Uri(Vocabularies.Nfo)]
        Channels,
        [Uri(Vocabularies.Nfo)]
        SampleCount,
        [Uri(Vocabularies.Nfo)]
        SampleRate,
        [Uri(Vocabularies.Nfo)]
        Duration,
        [Uri(Vocabularies.Nfo)]
        HasMediaStream,
        [Uri(Vocabularies.Nfo)]
        AverageBitrate,
        [Uri(Vocabularies.Nfo)]
        CompressionType,
        [Uri(Vocabularies.Nfo)]
        EncryptionStatus,

        [Uri(Vocabularies.Skos)]
        Broader,
        [Uri(Vocabularies.Skos)]
        ExactMatch,
        [Uri(Vocabularies.Skos)]
        CloseMatch,
        [Uri(Vocabularies.Skos)]
        PrefLabel,
        [Uri(Vocabularies.Skos)]
        AltLabel,

        [Uri(Vocabularies.Xis)]
        DocumentElement,
        [Uri(Vocabularies.Xis)]
        LocalName,
        [Uri(Vocabularies.Xis, "name")]
        XmlName,
        [Uri(Vocabularies.Xis, "prefix")]
        XmlPrefix,
        [Uri(Vocabularies.Xis)]
        NamespaceName,

        [Uri(Vocabularies.At)]
        Digest,
        [Uri(Vocabularies.At)]
        Source,
        [Uri(Vocabularies.At, "prefLabel")]
        AtPrefLabel,
        [Uri(Vocabularies.At, "altLabel")]
        AtAltLabel,
        [Uri(Vocabularies.At)]
        PathObject,
        [Uri(Vocabularies.At)]
        VolumeLabel,
    }
}
