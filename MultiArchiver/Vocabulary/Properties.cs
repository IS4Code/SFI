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

        [Uri(Vocabularies.Nie)]
        IsStoredAs,
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
    }
}
