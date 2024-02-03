namespace IS4.SFI.Vocabulary
{
    using IS4.SFI.Services;
    using static Vocabularies.Uri;

    /// <summary>
    /// Contains common RDF properties, with vocabulary prefixes
    /// taken from <see cref="Vocabularies.Uri"/>.
    /// </summary>
    public static class Properties
    {
        #region rdf
        /// <summary>
        /// <c><see cref="Rdf"/>:type</c>.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Type;

        /// <summary>
        /// <c><see cref="Rdf"/>:value</c>.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Value;

        /// <summary>
        /// <c><see cref="Rdf"/>:first</c>.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri First;

        /// <summary>
        /// <c><see cref="Rdf"/>:rest</c>.
        /// </summary>
        [Uri(Rdf)]
        public static readonly PropertyUri Rest;

        /// <summary>
        /// <c><see cref="Rdf"/>:_nnn</c>.
        /// </summary>
        public static readonly IPropertyUriFormatter<int> MemberAt = new UriTools.PrefixFormatter<int>(Rdf + "_");
        #endregion

        #region rdfs
        /// <summary>
        /// <c><see cref="Rdfs"/>:label</c>.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri Label;

        /// <summary>
        /// <c><see cref="Rdfs"/>:seeAlso</c>.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri SeeAlso;

        /// <summary>
        /// <c><see cref="Rdfs"/>:isDefinedBy</c>.
        /// </summary>
        [Uri(Rdfs)]
        public static readonly PropertyUri IsDefinedBy;
        #endregion

        #region owl
        /// <summary>
        /// <c><see cref="Owl"/>:sameAs</c>.
        /// </summary>
        [Uri(Owl)]
        public static readonly PropertyUri SameAs;

        /// <summary>
        /// <c><see cref="Owl"/>:equivalentProperty</c>.
        /// </summary>
        [Uri(Owl)]
        public static readonly PropertyUri EquivalentProperty;
        #endregion

        #region dcterms
        /// <summary>
        /// <c><see cref="Dcterms"/>:description</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Description;

        /// <summary>
        /// <c><see cref="Dcterms"/>:hasFormat</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri HasFormat;

        /// <summary>
        /// <c><see cref="Dcterms"/>:extent</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Extent;

        /// <summary>
        /// <c><see cref="Dcterms"/>:creator</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Creator;

        /// <summary>
        /// <c><see cref="Dcterms"/>:subject</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Subject;

        /// <summary>
        /// <c><see cref="Dcterms"/>:identifier</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Identifier;

        /// <summary>
        /// <c><see cref="Dcterms"/>:title</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Title;

        /// <summary>
        /// <c><see cref="Dcterms"/>:language</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Language;

        /// <summary>
        /// <c><see cref="Dcterms"/>:modified</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Modified;

        /// <summary>
        /// <c><see cref="Dcterms"/>:created</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Created;

        /// <summary>
        /// <c><see cref="Dcterms"/>:date</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Date;

        /// <summary>
        /// <c><see cref="Dcterms"/>:provenance</c>.
        /// </summary>
        [Uri(Dcterms)]
        public static readonly PropertyUri Provenance;
        #endregion

        /// <summary>
        /// <c><see cref="Dbo"/>:originalName</c>.
        /// </summary>
        [Uri(Dbo)]
        public static readonly PropertyUri OriginalName;

        #region cnt
        /// <summary>
        /// <c><see cref="Cnt"/>:bytes</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri Bytes;

        /// <summary>
        /// <c><see cref="Cnt"/>:chars</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri Chars;

        /// <summary>
        /// <c><see cref="Cnt"/>:characterEncoding</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri CharacterEncoding;

        /// <summary>
        /// <c><see cref="Cnt"/>:rest</c>.
        /// </summary>
        [Uri(Cnt, "rest")]
        public static readonly PropertyUri RestXml;

        /// <summary>
        /// <c><see cref="Cnt"/>:doctypeName</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri DoctypeName;

        /// <summary>
        /// <c><see cref="Cnt"/>:publicId</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri PublicId;

        /// <summary>
        /// <c><see cref="Cnt"/>:systemId</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri SystemId;

        /// <summary>
        /// <c><see cref="Cnt"/>:version</c>.
        /// </summary>
        [Uri(Cnt, "version")]
        public static readonly PropertyUri XmlVersion;

        /// <summary>
        /// <c><see cref="Cnt"/>:declaredEncoding</c>.
        /// </summary>
        [Uri(Cnt, "declaredEncoding")]
        public static readonly PropertyUri XmlEncoding;

        /// <summary>
        /// <c><see cref="Cnt"/>:standalone</c>.
        /// </summary>
        [Uri(Cnt, "standalone")]
        public static readonly PropertyUri XmlStandalone;

        /// <summary>
        /// <c><see cref="Cnt"/>:dtDecl</c>.
        /// </summary>
        [Uri(Cnt)]
        public static readonly PropertyUri DtDecl;
        #endregion

        /// <summary>
        /// <c><see cref="Foaf"/>:depicts</c>.
        /// </summary>
        [Uri(Foaf)]
        public static readonly PropertyUri Depicts;

        #region sec
        /// <summary>
        /// <c><see cref="Sec"/>:canonicalizationAlgorithm</c>.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri CanonicalizationAlgorithm;

        /// <summary>
        /// <c><see cref="Sec"/>:digestAlgorithm</c>.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri DigestAlgorithm;

        /// <summary>
        /// <c><see cref="Sec"/>:digestValue</c>.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri DigestValue;

        /// <summary>
        /// <c><see cref="Sec"/>:expiration</c>.
        /// </summary>
        [Uri(Sec)]
        public static readonly PropertyUri Expiration;
        #endregion

        #region schema
        /// <summary>
        /// <c><see cref="Schema"/>:name</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Name;

        /// <summary>
        /// <c><see cref="Schema"/>:downloadUrl</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri DownloadUrl;

        /// <summary>
        /// <c><see cref="Schema"/>:encoding</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Encoding;

        /// <summary>
        /// <c><see cref="Schema"/>:encodingFormat</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri EncodingFormat;

        /// <summary>
        /// <c><see cref="Schema"/>:version</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Version;

        /// <summary>
        /// <c><see cref="Schema"/>:softwareVersion</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri SoftwareVersion;

        /// <summary>
        /// <c><see cref="Schema"/>:thumbnail</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Thumbnail;

        /// <summary>
        /// <c><see cref="Schema"/>:image</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Image;

        /// <summary>
        /// <c><see cref="Schema"/>:copyrightNotice</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri CopyrightNotice;

        /// <summary>
        /// <c><see cref="Schema"/>:keywords</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Keywords;

        /// <summary>
        /// <c><see cref="Schema"/>:category</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri Category;

        /// <summary>
        /// <c><see cref="Schema"/>:serialNumber</c>.
        /// </summary>
        [Uri(Schema)]
        public static readonly PropertyUri SerialNumber;
        #endregion

        #region nie
        /// <summary>
        /// <c><see cref="Nie"/>:interpretedAs</c>.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri InterpretedAs;

        /// <summary>
        /// <c><see cref="Nie"/>:links</c>.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri Links;

        /// <summary>
        /// <c><see cref="Nie"/>:hasPart</c>.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri HasPart;

        /// <summary>
        /// <c><see cref="Nie"/>:mimeType</c>.
        /// </summary>
        [Uri(Nie)]
        public static readonly PropertyUri MimeType;
        #endregion

        #region nfo
        /// <summary>
        /// <c><see cref="Nfo"/>:fileName</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileName;

        /// <summary>
        /// <c><see cref="Nfo"/>:belongsToContainer</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BelongsToContainer;

        /// <summary>
        /// <c><see cref="Nfo"/>:fileCreated</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileCreated;

        /// <summary>
        /// <c><see cref="Nfo"/>:fileLastAccessed</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileLastAccessed;

        /// <summary>
        /// <c><see cref="Nfo"/>:fileLastModified</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileLastModified;

        /// <summary>
        /// <c><see cref="Nfo"/>:fileSize</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FileSize;

        /// <summary>
        /// <c><see cref="Nfo"/>:freeSpace</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FreeSpace;

        /// <summary>
        /// <c><see cref="Nfo"/>:occupiedSpace</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri OccupiedSpace;

        /// <summary>
        /// <c><see cref="Nfo"/>:totalSpace</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri TotalSpace;

        /// <summary>
        /// <c><see cref="Nfo"/>:filesystemType</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FilesystemType;

        /// <summary>
        /// <c><see cref="Nfo"/>:width</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Width;

        /// <summary>
        /// <c><see cref="Nfo"/>:height</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Height;

        /// <summary>
        /// <c><see cref="Nfo"/>:bitDepth</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BitDepth;

        /// <summary>
        /// <c><see cref="Nfo"/>:colorDepth</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri ColorDepth;

        /// <summary>
        /// <c><see cref="Nfo"/>:horizontalResolution</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri HorizontalResolution;

        /// <summary>
        /// <c><see cref="Nfo"/>:verticalResolution</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri VerticalResolution;

        /// <summary>
        /// <c><see cref="Nfo"/>:paletteSize</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri PaletteSize;

        /// <summary>
        /// <c><see cref="Nfo"/>:bitsPerSample</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri BitsPerSample;

        /// <summary>
        /// <c><see cref="Nfo"/>:channels</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Channels;

        /// <summary>
        /// <c><see cref="Nfo"/>:sampleCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri SampleCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:sampleRate</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri SampleRate;

        /// <summary>
        /// <c><see cref="Nfo"/>:duration</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri Duration;

        /// <summary>
        /// <c><see cref="Nfo"/>:hasMediaStream</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri HasMediaStream;

        /// <summary>
        /// <c><see cref="Nfo"/>:averageBitrate</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri AverageBitrate;

        /// <summary>
        /// <c><see cref="Nfo"/>:compressionType</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri CompressionType;

        /// <summary>
        /// <c><see cref="Nfo"/>:encryptionStatus</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri EncryptionStatus;

        /// <summary>
        /// <c><see cref="Nfo"/>:characterCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri CharacterCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:lineCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri LineCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:wordCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri WordCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:pageCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri PageCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:frameCount</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FrameCount;

        /// <summary>
        /// <c><see cref="Nfo"/>:frameRate</c>.
        /// </summary>
        [Uri(Nfo)]
        public static readonly PropertyUri FrameRate;
        #endregion

        #region nmo
        /// <summary>
        /// <c><see cref="Nmo"/>:messageId</c>.
        /// </summary>
        [Uri(Nmo)]
        public static readonly PropertyUri MessageId;

        /// <summary>
        /// <c><see cref="Nmo"/>:inReplyTo</c>.
        /// </summary>
        [Uri(Nmo)]
        public static readonly PropertyUri InReplyTo;

        /// <summary>
        /// <c><see cref="Nmo"/>:messageHeader</c>.
        /// </summary>
        [Uri(Nmo)]
        public static readonly PropertyUri MessageHeader;
        #endregion

        #region skos
        /// <summary>
        /// <c><see cref="Skos"/>:broader</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri Broader;

        /// <summary>
        /// <c><see cref="Skos"/>:exactMatch</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri ExactMatch;

        /// <summary>
        /// <c><see cref="Skos"/>:closeMatch</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri CloseMatch;

        /// <summary>
        /// <c><see cref="Skos"/>:prefLabel</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri PrefLabel;

        /// <summary>
        /// <c><see cref="Skos"/>:altLabel</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri AltLabel;

        /// <summary>
        /// <c><see cref="Skos"/>:notation</c>.
        /// </summary>
        [Uri(Skos)]
        public static readonly PropertyUri Notation;
        #endregion

        #region xis
        /// <summary>
        /// <c><see cref="Xis"/>:attributes</c>.
        /// </summary>
        [Uri(Xis, "attributes")]
        public static readonly PropertyUri XmlAttributes;

        /// <summary>
        /// <c><see cref="Xis"/>:attributeType</c>.
        /// </summary>
        [Uri(Xis, "attributeType")]
        public static readonly PropertyUri XmlAttributeType;

        /// <summary>
        /// <c><see cref="Xis"/>:baseURI</c>.
        /// </summary>
        [Uri(Xis, "baseURI")]
        public static readonly PropertyUri XmlBaseUri;

        /// <summary>
        /// <c><see cref="Xis"/>:characterCode</c>.
        /// </summary>
        [Uri(Xis, "characterCode")]
        public static readonly PropertyUri XmlCharacterCode;

        /// <summary>
        /// <c><see cref="Xis"/>:characterEncodingScheme</c>.
        /// </summary>
        [Uri(Xis, "characterEncodingScheme")]
        public static readonly PropertyUri XmlCharacterEncodingScheme;

        /// <summary>
        /// <c><see cref="Xis"/>:children</c>.
        /// </summary>
        [Uri(Xis, "children")]
        public static readonly PropertyUri XmlChildren;

        /// <summary>
        /// <c><see cref="Xis"/>:content</c>.
        /// </summary>
        [Uri(Xis, "content")]
        public static readonly PropertyUri XmlContent;

        /// <summary>
        /// <c><see cref="Xis"/>:namespaceAttributes</c>.
        /// </summary>
        [Uri(Xis, "namespaceAttributes")]
        public static readonly PropertyUri XmlNamespaceAttributes;

        /// <summary>
        /// <c><see cref="Xis"/>:declarationBaseURI</c>.
        /// </summary>
        [Uri(Xis, "declarationBaseURI")]
        public static readonly PropertyUri XmlDeclarationBaseURI;

        /// <summary>
        /// <c><see cref="Xis"/>:documentElement</c>.
        /// </summary>
        [Uri(Xis, "documentElement")]
        public static readonly PropertyUri XmlDocumentElement;

        /// <summary>
        /// <c><see cref="Xis"/>:elementContentWhitespace</c>.
        /// </summary>
        [Uri(Xis, "elementContentWhitespace")]
        public static readonly PropertyUri XmlElementContentWhitespace;

        /// <summary>
        /// <c><see cref="Xis"/>:unparsedEntities</c>.
        /// </summary>
        [Uri(Xis, "unparsedEntities")]
        public static readonly PropertyUri XmlUnparsedEntities;

        /// <summary>
        /// <c><see cref="Xis"/>:inScopeNamespaces</c>.
        /// </summary>
        [Uri(Xis, "inScopeNamespaces")]
        public static readonly PropertyUri XmlInScopeNamespaces;

        /// <summary>
        /// <c><see cref="Xis"/>:localName</c>.
        /// </summary>
        [Uri(Xis, "localName")]
        public static readonly PropertyUri XmlLocalName;

        /// <summary>
        /// <c><see cref="Xis"/>:name</c>.
        /// </summary>
        [Uri(Xis, "name")]
        public static readonly PropertyUri XmlName;

        /// <summary>
        /// <c><see cref="Xis"/>:namespaceName</c>.
        /// </summary>
        [Uri(Xis, "namespaceName")]
        public static readonly PropertyUri XmlNamespaceName;

        /// <summary>
        /// <c><see cref="Xis"/>:normalizedValue</c>.
        /// </summary>
        [Uri(Xis, "normalizedValue")]
        public static readonly PropertyUri XmlNormalizedValue;

        /// <summary>
        /// <c><see cref="Xis"/>:notation</c>.
        /// </summary>
        [Uri(Xis, "notation")]
        public static readonly PropertyUri XmlNotation;

        /// <summary>
        /// <c><see cref="Xis"/>:notationName</c>.
        /// </summary>
        [Uri(Xis, "notationName")]
        public static readonly PropertyUri XmlNotationName;

        /// <summary>
        /// <c><see cref="Xis"/>:notations</c>.
        /// </summary>
        [Uri(Xis, "notations")]
        public static readonly PropertyUri XmlNotations;

        /// <summary>
        /// <c><see cref="Xis"/>:ownerElement</c>.
        /// </summary>
        [Uri(Xis, "ownerElement")]
        public static readonly PropertyUri XmlOwnerElement;

        /// <summary>
        /// <c><see cref="Xis"/>:parent</c>.
        /// </summary>
        [Uri(Xis, "parent")]
        public static readonly PropertyUri XmlParent;

        /// <summary>
        /// <c><see cref="Xis"/>:prefix</c>.
        /// </summary>
        [Uri(Xis, "prefix")]
        public static readonly PropertyUri XmlPrefix;

        /// <summary>
        /// <c><see cref="Xis"/>:publicIdentifier</c>.
        /// </summary>
        [Uri(Xis, "publicIdentifier")]
        public static readonly PropertyUri XmlPublicIdentifier;

        /// <summary>
        /// <c><see cref="Xis"/>:references</c>.
        /// </summary>
        [Uri(Xis, "references")]
        public static readonly PropertyUri XmlReferences;

        /// <summary>
        /// <c><see cref="Xis"/>:specified</c>.
        /// </summary>
        [Uri(Xis, "specified")]
        public static readonly PropertyUri XmlSpecified;

        /// <summary>
        /// <c><see cref="Xis"/>:standalone</c>.
        /// </summary>
        [Uri(Xis, "standalone")]
        public static readonly PropertyUri XmlDocumentStandalone;

        /// <summary>
        /// <c><see cref="Xis"/>:systemIdentifier</c>.
        /// </summary>
        [Uri(Xis, "systemIdentifier")]
        public static readonly PropertyUri XmlSystemIdentifier;

        /// <summary>
        /// <c><see cref="Xis"/>:target</c>.
        /// </summary>
        [Uri(Xis, "target")]
        public static readonly PropertyUri XmlTarget;

        /// <summary>
        /// <c><see cref="Xis"/>:version</c>.
        /// </summary>
        [Uri(Xis, "version")]
        public static readonly PropertyUri XmlDocumentVersion;
        #endregion

        #region http
        /// <summary>
        /// <c><see cref="Http"/>:resp</c>.
        /// </summary>
        [Uri(Http, "resp")]
        public static readonly PropertyUri HttpResponse;

        /// <summary>
        /// <c><see cref="Http"/>:methodName</c>.
        /// </summary>
        [Uri(Http, "methodName")]
        public static readonly PropertyUri HttpMethodName;

        /// <summary>
        /// <c><see cref="Http"/>:mthd</c>.
        /// </summary>
        [Uri(Http, "mthd")]
        public static readonly PropertyUri HttpMethod;

        /// <summary>
        /// <c><see cref="Http"/>:absoluteURI</c>.
        /// </summary>
        [Uri(Http, "absoluteURI")]
        public static readonly PropertyUri HttpAbsoluteUri;

        /// <summary>
        /// <c><see cref="Http"/>:absolutePath</c>.
        /// </summary>
        [Uri(Http, "absolutePath")]
        public static readonly PropertyUri HttpAbsolutePath;

        /// <summary>
        /// <c><see cref="Http"/>:authority</c>.
        /// </summary>
        [Uri(Http, "authority")]
        public static readonly PropertyUri HttpAuthority;

        /// <summary>
        /// <c><see cref="Http"/>:httpVersion</c>.
        /// </summary>
        [Uri(Http)]
        public static readonly PropertyUri HttpVersion;

        /// <summary>
        /// <c><see cref="Http"/>:statusCodeValue</c>.
        /// </summary>
        [Uri(Http, "statusCodeValue")]
        public static readonly PropertyUri HttpStatusCodeValue;

        /// <summary>
        /// <c><see cref="Http"/>:sc</c>.
        /// </summary>
        [Uri(Http, "sc")]
        public static readonly PropertyUri HttpStatusCode;

        /// <summary>
        /// <c><see cref="Http"/>:reasonPhrase</c>.
        /// </summary>
        [Uri(Http, "reasonPhrase")]
        public static readonly PropertyUri HttpReasonPhrase;

        /// <summary>
        /// <c><see cref="Http"/>:headers</c>.
        /// </summary>
        [Uri(Http, "headers")]
        public static readonly PropertyUri HttpHeaders;

        /// <summary>
        /// <c><see cref="Http"/>:fieldName</c>.
        /// </summary>
        [Uri(Http, "fieldName")]
        public static readonly PropertyUri HttpFieldName;

        /// <summary>
        /// <c><see cref="Http"/>:hdrName</c>.
        /// </summary>
        [Uri(Http, "hdrName")]
        public static readonly PropertyUri HttpHeaderName;

        /// <summary>
        /// <c><see cref="Http"/>:fieldValue</c>.
        /// </summary>
        [Uri(Http, "fieldValue")]
        public static readonly PropertyUri HttpFieldValue;

        /// <summary>
        /// <c><see cref="Http"/>:body</c>.
        /// </summary>
        [Uri(Http, "body")]
        public static readonly PropertyUri HttpBody;
        #endregion

        #region at
        /// <summary>
        /// <c><see cref="At"/>:digest</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Digest;

        /// <summary>
        /// <c><see cref="At"/>:source</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Source;

        /// <summary>
        /// <c><see cref="At"/>:prefLabel</c>.
        /// </summary>
        [Uri(At, "prefLabel")]
        public static readonly PropertyUri AtPrefLabel;

        /// <summary>
        /// <c><see cref="At"/>:altLabel</c>.
        /// </summary>
        [Uri(At, "altLabel")]
        public static readonly PropertyUri AtAltLabel;

        /// <summary>
        /// <c><see cref="At"/>:pathObject</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri PathObject;

        /// <summary>
        /// <c><see cref="At"/>:extensionObject</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri ExtensionObject;

        /// <summary>
        /// <c><see cref="At"/>:volumeLabel</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri VolumeLabel;

        /// <summary>
        /// <c><see cref="At"/>:visited</c>.
        /// </summary>
        [Uri(At)]
        public static readonly PropertyUri Visited;
        #endregion

        static Properties()
        {
            typeof(Properties).InitializeUris();
        }
    }
}
