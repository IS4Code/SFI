namespace IS4.SFI.Vocabulary
{
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF properties, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Properties
    {
        /// <summary>
        /// <see cref="Rdf"/>:type.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Type;

        /// <summary>
        /// <see cref="Rdf"/>:value.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Value;

        /// <summary>
        /// <see cref="Rdf"/>:first.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri First;

        /// <summary>
        /// <see cref="Rdf"/>:rest.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Rest;

        /// <summary>
        /// <see cref="Rdfs"/>:label.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri Label;

        /// <summary>
        /// <see cref="Rdfs"/>:seeAlso.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri SeeAlso;

        /// <summary>
        /// <see cref="Rdfs"/>:isDefinedBy.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri IsDefinedBy;

        /// <summary>
        /// <see cref="Owl"/>:sameAs.
        /// </summary>
        [Uri(Owl)]
        public static readonly PropertyUri SameAs;

        /// <summary>
        /// <see cref="Owl"/>:equivalentProperty.
        /// </summary>
        [Uri(Owl)]
        public static readonly PropertyUri EquivalentProperty;

        /// <summary>
        /// <see cref="Dcterms"/>:description.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Description;

        /// <summary>
        /// <see cref="Dcterms"/>:hasFormat.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri HasFormat;

        /// <summary>
        /// <see cref="Dcterms"/>:extent.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Extent;

        /// <summary>
        /// <see cref="Dcterms"/>:creator.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Creator;

        /// <summary>
        /// <see cref="Dcterms"/>:creator.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Subject;

        /// <summary>
        /// <see cref="Dcterms"/>:identifier.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Identifier;

        /// <summary>
        /// <see cref="Dcterms"/>:title.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Title;

        /// <summary>
        /// <see cref="Dcterms"/>:language.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Language;

        /// <summary>
        /// <see cref="Dcterms"/>:modified.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Modified;

        /// <summary>
        /// <see cref="Dcterms"/>:created.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Created;

        /// <summary>
        /// <see cref="Dcterms"/>:date.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Date;

        /// <summary>
        /// <see cref="Dcterms"/>:provenance.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Provenance;

        /// <summary>
        /// <see cref="Dbo"/>:originalName.
        /// </summary>
        [Uri(Dbo)]
        public static readonly PropertyUri OriginalName;

        /// <summary>
        /// <see cref="Cnt"/>:bytes.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri Bytes;

        /// <summary>
        /// <see cref="Cnt"/>:chars.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri Chars;

        /// <summary>
        /// <see cref="Cnt"/>:characterEncoding.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri CharacterEncoding;

        /// <summary>
        /// <see cref="Cnt"/>:rest.
        /// </summary>
        [Uri(Cnt, "rest")]
        public static readonly PropertyUri RestXml;

        /// <summary>
        /// <see cref="Cnt"/>:doctypeName.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri DoctypeName;

        /// <summary>
        /// <see cref="Cnt"/>:publicId.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri PublicId;

        /// <summary>
        /// <see cref="Cnt"/>:systemId.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri SystemId;

        /// <summary>
        /// <see cref="Cnt"/>:version.
        /// </summary>
        [Uri(Cnt, "version")]
        public static readonly PropertyUri XmlVersion;

        /// <summary>
        /// <see cref="Cnt"/>:declaredEncoding.
        /// </summary>
        [Uri(Cnt, "declaredEncoding")]
        public static readonly PropertyUri XmlEncoding;

        /// <summary>
        /// <see cref="Cnt"/>:standalone.
        /// </summary>
        [Uri(Cnt, "standalone")]
        public static readonly PropertyUri XmlStandalone;

        /// <summary>
        /// <see cref="Cnt"/>:dtDecl.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri DtDecl;

        /// <summary>
        /// <see cref="Foaf"/>:depicts.
        /// </summary>
        [Uri(Foaf)]
        public static readonly PropertyUri Depicts;

        /// <summary>
        /// <see cref="Sec"/>:canonicalizationAlgorithm.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri CanonicalizationAlgorithm;

        /// <summary>
        /// <see cref="Sec"/>:digestAlgorithm.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri DigestAlgorithm;

        /// <summary>
        /// <see cref="Sec"/>:digestValue.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri DigestValue;

        /// <summary>
        /// <see cref="Sec"/>:expiration.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri Expiration;

        /// <summary>
        /// <see cref="Schema"/>:name.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Name;

        /// <summary>
        /// <see cref="Schema"/>:downloadUrl.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri DownloadUrl;

        /// <summary>
        /// <see cref="Schema"/>:encoding.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Encoding;

        /// <summary>
        /// <see cref="Schema"/>:encodingFormat.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri EncodingFormat;

        /// <summary>
        /// <see cref="Schema"/>:version.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Version;

        /// <summary>
        /// <see cref="Schema"/>:softwareVersion.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri SoftwareVersion;

        /// <summary>
        /// <see cref="Schema"/>:thumbnail.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Thumbnail;

        /// <summary>
        /// <see cref="Schema"/>:image.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Image;

        /// <summary>
        /// <see cref="Schema"/>:copyrightNotice.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri CopyrightNotice;

        /// <summary>
        /// <see cref="Schema"/>:keywords.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Keywords;

        /// <summary>
        /// <see cref="Schema"/>:category.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Category;

        /// <summary>
        /// <see cref="Nie"/>:interpretedAs.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri InterpretedAs;

        /// <summary>
        /// <see cref="Nie"/>:links.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri Links;

        /// <summary>
        /// <see cref="Nie"/>:hasPart.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri HasPart;

        /// <summary>
        /// <see cref="Nfo"/>:fileName.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileName;

        /// <summary>
        /// <see cref="Nfo"/>:belongsToContainer.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BelongsToContainer;

        /// <summary>
        /// <see cref="Nfo"/>:fileCreated.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileCreated;

        /// <summary>
        /// <see cref="Nfo"/>:fileLastAccessed.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileLastAccessed;

        /// <summary>
        /// <see cref="Nfo"/>:fileLastModified.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileLastModified;

        /// <summary>
        /// <see cref="Nfo"/>:fileSize.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileSize;

        /// <summary>
        /// <see cref="Nfo"/>:freeSpace.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FreeSpace;

        /// <summary>
        /// <see cref="Nfo"/>:occupiedSpace.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri OccupiedSpace;

        /// <summary>
        /// <see cref="Nfo"/>:totalSpace.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri TotalSpace;

        /// <summary>
        /// <see cref="Nfo"/>:filesystemType.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FilesystemType;

        /// <summary>
        /// <see cref="Nfo"/>:width.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Width;

        /// <summary>
        /// <see cref="Nfo"/>:height.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Height;

        /// <summary>
        /// <see cref="Nfo"/>:bitDepth.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BitDepth;

        /// <summary>
        /// <see cref="Nfo"/>:colorDepth.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri ColorDepth;

        /// <summary>
        /// <see cref="Nfo"/>:horizontalResolution.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri HorizontalResolution;

        /// <summary>
        /// <see cref="Nfo"/>:verticalResolution.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri VerticalResolution;

        /// <summary>
        /// <see cref="Nfo"/>:paletteSize.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri PaletteSize;

        /// <summary>
        /// <see cref="Nfo"/>:bitsPerSample.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BitsPerSample;

        /// <summary>
        /// <see cref="Nfo"/>:channels.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Channels;

        /// <summary>
        /// <see cref="Nfo"/>:sampleCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri SampleCount;

        /// <summary>
        /// <see cref="Nfo"/>:sampleRate.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri SampleRate;

        /// <summary>
        /// <see cref="Nfo"/>:duration.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Duration;

        /// <summary>
        /// <see cref="Nfo"/>:hasMediaStream.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri HasMediaStream;

        /// <summary>
        /// <see cref="Nfo"/>:averageBitrate.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri AverageBitrate;

        /// <summary>
        /// <see cref="Nfo"/>:compressionType.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri CompressionType;

        /// <summary>
        /// <see cref="Nfo"/>:encryptionStatus.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri EncryptionStatus;

        /// <summary>
        /// <see cref="Nfo"/>:characterCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri CharacterCount;

        /// <summary>
        /// <see cref="Nfo"/>:lineCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri LineCount;

        /// <summary>
        /// <see cref="Nfo"/>:wordCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri WordCount;

        /// <summary>
        /// <see cref="Nfo"/>:pageCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri PageCount;

        /// <summary>
        /// <see cref="Nfo"/>:frameCount.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FrameCount;

        /// <summary>
        /// <see cref="Nfo"/>:frameRate.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FrameRate;

        /// <summary>
        /// <see cref="Skos"/>:broader.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri Broader;

        /// <summary>
        /// <see cref="Skos"/>:exactMatch.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri ExactMatch;

        /// <summary>
        /// <see cref="Skos"/>:closeMatch.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri CloseMatch;

        /// <summary>
        /// <see cref="Skos"/>:prefLabel.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri PrefLabel;

        /// <summary>
        /// <see cref="Skos"/>:altLabel.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri AltLabel;

        /// <summary>
        /// <see cref="Skos"/>:notation.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri Notation;

        /// <summary>
        /// <see cref="Xis"/>:documentElement.
        /// </summary>
        [Uri(Xis)]
        public static readonly PropertyUri DocumentElement;

        /// <summary>
        /// <see cref="Xis"/>:localName.
        /// </summary>
        [Uri(Xis)]
        public static readonly PropertyUri LocalName;

        /// <summary>
        /// <see cref="Xis"/>:name.
        /// </summary>
        [Uri(Xis, "name")]
        public static readonly PropertyUri XmlName;

        /// <summary>
        /// <see cref="Xis"/>:prefix.
        /// </summary>
        [Uri(Xis, "prefix")]
        public static readonly PropertyUri XmlPrefix;

        /// <summary>
        /// <see cref="Xis"/>:namespaceName.
        /// </summary>
        [Uri(Xis)]
        public static readonly PropertyUri NamespaceName;

        /// <summary>
        /// <see cref="Http"/>:resp.
        /// </summary>
        [Uri(Http, "resp")]
        public static readonly PropertyUri HttpResponse;

        /// <summary>
        /// <see cref="Http"/>:methodName.
        /// </summary>
        [Uri(Http, "methodName")]
        public static readonly PropertyUri HttpMethodName;

        /// <summary>
        /// <see cref="Http"/>:mthd.
        /// </summary>
        [Uri(Http, "mthd")]
        public static readonly PropertyUri HttpMethod;

        /// <summary>
        /// <see cref="Http"/>:absoluteURI.
        /// </summary>
        [Uri(Http, "absoluteURI")]
        public static readonly PropertyUri HttpAbsoluteUri;

        /// <summary>
        /// <see cref="Http"/>:absolutePath.
        /// </summary>
        [Uri(Http, "absolutePath")]
        public static readonly PropertyUri HttpAbsolutePath;

        /// <summary>
        /// <see cref="Http"/>:authority.
        /// </summary>
        [Uri(Http, "authority")]
        public static readonly PropertyUri HttpAuthority;

        /// <summary>
        /// <see cref="Http"/>:httpVersion.
        /// </summary>
        [Uri(Http)]
        public static readonly PropertyUri HttpVersion;

        /// <summary>
        /// <see cref="Http"/>:statusCodeValue.
        /// </summary>
        [Uri(Http, "statusCodeValue")]
        public static readonly PropertyUri HttpStatusCodeValue;

        /// <summary>
        /// <see cref="Http"/>:sc.
        /// </summary>
        [Uri(Http, "sc")]
        public static readonly PropertyUri HttpStatusCode;

        /// <summary>
        /// <see cref="Http"/>:reasonPhrase.
        /// </summary>
        [Uri(Http, "reasonPhrase")]
        public static readonly PropertyUri HttpReasonPhrase;

        /// <summary>
        /// <see cref="Http"/>:headers.
        /// </summary>
        [Uri(Http, "headers")]
        public static readonly PropertyUri HttpHeaders;

        /// <summary>
        /// <see cref="Http"/>:fieldName.
        /// </summary>
        [Uri(Http, "fieldName")]
        public static readonly PropertyUri HttpFieldName;

        /// <summary>
        /// <see cref="Http"/>:hdrName.
        /// </summary>
        [Uri(Http, "hdrName")]
        public static readonly PropertyUri HttpHeaderName;

        /// <summary>
        /// <see cref="Http"/>:fieldValue.
        /// </summary>
        [Uri(Http, "fieldValue")]
        public static readonly PropertyUri HttpFieldValue;

        /// <summary>
        /// <see cref="Http"/>:body.
        /// </summary>
        [Uri(Http, "body")]
        public static readonly PropertyUri HttpBody;

        /// <summary>
        /// <see cref="At"/>:digest.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Digest;

        /// <summary>
        /// <see cref="At"/>:source.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Source;

        /// <summary>
        /// <see cref="At"/>:prefLabel.
        /// </summary>
        [Uri(At, "prefLabel")]
        public static readonly PropertyUri AtPrefLabel;

        /// <summary>
        /// <see cref="At"/>:altLabel.
        /// </summary>
        [Uri(At, "altLabel")]
        public static readonly PropertyUri AtAltLabel;

        /// <summary>
        /// <see cref="At"/>:pathObject.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri PathObject;

        /// <summary>
        /// <see cref="At"/>:extensionObject.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri ExtensionObject;

        /// <summary>
        /// <see cref="At"/>:volumeLabel.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri VolumeLabel;

        /// <summary>
        /// <see cref="At"/>:visited.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Visited;

        static Properties()
        {
            typeof(Properties).InitializeUris();
        }
    }
}
