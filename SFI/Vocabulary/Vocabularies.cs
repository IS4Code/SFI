﻿using IS4.SFI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.SFI.Vocabulary
{
    /// <summary>
    /// Contains common RDF vocabularies.
    /// </summary>
    public static class Vocabularies
    {
        /// <summary>
        /// The vocabulary storing hash-identified objects identified
        /// using <see cref="AdHashedContentUriFormatter"/>.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> Ad = new VocabularyUri(Uri.Ad);

        /// <summary>
        /// The vocabulary of all local file: URIs.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> File = new VocabularyUri(Uri.File);

        /// <summary>
        /// The vocabulary for MIME types from uri4uri.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> Urim = new VocabularyUri(Uri.Urim);

        /// <summary>
        /// The vocabulary for suffixes/file extensions from uri4uri.
        /// </summary>
        public static readonly IIndividualUriFormatter<string> Uris = new VocabularyUri(Uri.Uris);

        /// <summary>
        /// Stores raw URIs of all used vocabularies.
        /// </summary>
        public static class Uri
        {
            /// <summary>
            /// RDF Vocabulary
            /// </summary>
            public const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// RDF Schema Vocabulary
            /// </summary>
            public const string Rdfs = "http://www.w3.org/2000/01/rdf-schema#";

            /// <summary>
            /// Simple Knowledge Organization System
            /// </summary>
            public const string Skos = "http://www.w3.org/2004/02/skos/core#";

            /// <summary>
            /// The Security Vocabulary
            /// </summary>
            public const string Sec = "https://w3id.org/security#";

            /// <summary>
            /// DCMI Metadata Terms
            /// </summary>
            public const string Dcterms = "http://purl.org/dc/terms/";

            /// <summary>
            /// NEPOMUK File Ontology
            /// </summary>
            public const string Nfo = "http://www.semanticdesktop.org/ontologies/2007/03/22/nfo#";

            /// <summary>
            /// NEPOMUK Information Element Ontology
            /// </summary>
            public const string Nie = "http://www.semanticdesktop.org/ontologies/2007/01/19/nie#";

            /// <summary>
            /// NEPOMUK ID3 Ontology
            /// </summary>
            public const string Nid3 = "http://www.semanticdesktop.org/ontologies/2007/05/10/nid3#";

            /// <summary>
            /// Schema.org
            /// </summary>
            public const string Schema = "http://schema.org/";

            /// <summary>
            /// Representing Content in RDF
            /// </summary>
            public const string Cnt = "http://www.w3.org/2011/content#";

            /// <summary>
            /// Schema for XML Signatures
            /// </summary>
            public const string Ds = "http://www.w3.org/2000/09/xmldsig#";

            /// <summary>
            /// Additional XML Digital Signature URIs
            /// </summary>
            public const string Dsm = "http://www.w3.org/2001/04/xmldsig-more#";

            /// <summary>
            /// Additional XML Digital Signature URIs 2
            /// </summary>
            public const string Dsm2 = "http://www.w3.org/2007/05/xmldsig-more#";

            /// <summary>
            /// XML Encryption Syntax and Processing
            /// </summary>
            public const string Enc = "http://www.w3.org/2001/04/xmlenc#";

            /// <summary>
            /// FOAF Vocabulary
            /// </summary>
            public const string Foaf = "http://xmlns.com/foaf/0.1/";

            /// <summary>
            /// EXIF RDF Schema
            /// </summary>
            public const string Exif = "http://www.w3.org/2003/12/exif/ns#";

            /// <summary>
            /// XML Schema Datatypes
            /// </summary>
            public const string Xsd = "http://www.w3.org/2001/XMLSchema#";

            /// <summary>
            /// XML Infoset
            /// </summary>
            public const string Xis = "http://www.w3.org/2001/04/infoset#";

            /// <summary>
            /// OWL
            /// </summary>
            public const string Owl = "http://www.w3.org/2002/07/owl#";

            /// <summary>
            /// DBPedia Datatypes
            /// </summary>
            public const string Dt = "http://dbpedia.org/datatype/";

            /// <summary>
            /// DBPedia Ontology
            /// </summary>
            public const string Dbo = "http://dbpedia.org/ontology/";

            /// <summary>
            /// uri4uri MIME types
            /// </summary>
            public const string Urim = "http://purl.org/uri4uri/mime/";

            /// <summary>
            /// uri4uri suffixes
            /// </summary>
            public const string Uris = "http://purl.org/uri4uri/suffix/";

            /// <summary>
            /// The Cert Ontology
            /// </summary>
            public const string Cert = "http://www.w3.org/ns/auth/cert#";

            /// <summary>
            /// file: URIs
            /// </summary>
            public const string File = "file:///";

            /// <summary>
            /// Archive terms
            /// </summary>
            public const string At = "http://archive.data.is4.site/terms/";

            /// <summary>
            /// Archive data
            /// </summary>
            public const string Ad = "http://archive.data.is4.site/data/";
        }

        /// <summary>
        /// Contains a collection of all URIs from <see cref="Uri"/> and their common prefix.
        /// </summary>
        public static readonly IReadOnlyCollection<KeyValuePair<System.Uri, string>> Prefixes =
            typeof(Uri).GetFields()
            .Where(f => f.IsLiteral)
            .Select(f => new KeyValuePair<System.Uri, string>(new System.Uri(f.GetRawConstantValue().ToString(), UriKind.Absolute), f.Name.ToLowerInvariant()))
            .ToList();
    }
}