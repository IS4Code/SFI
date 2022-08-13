using IS4.MultiArchiver.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.MultiArchiver.Vocabulary
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
            public const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            public const string Rdfs = "http://www.w3.org/2000/01/rdf-schema#";
            public const string Skos = "http://www.w3.org/2004/02/skos/core#";
            public const string Sec = "https://w3id.org/security#";
            public const string Dcterms = "http://purl.org/dc/terms/";
            public const string Nfo = "http://www.semanticdesktop.org/ontologies/2007/03/22/nfo#";
            public const string Nie = "http://www.semanticdesktop.org/ontologies/2007/01/19/nie#";
            public const string Nid3 = "http://www.semanticdesktop.org/ontologies/2007/05/10/nid3#";
            public const string Schema = "http://schema.org/";
            public const string Cnt = "http://www.w3.org/2011/content#";
            public const string Ds = "http://www.w3.org/2000/09/xmldsig#";
            public const string Dsm = "http://www.w3.org/2001/04/xmldsig-more#";
            public const string Dsm2 = "http://www.w3.org/2007/05/xmldsig-more#";
            public const string Enc = "http://www.w3.org/2001/04/xmlenc#";
            public const string Foaf = "http://xmlns.com/foaf/0.1/";
            public const string Exif = "http://www.w3.org/2003/12/exif/ns#";
            public const string Xsd = "http://www.w3.org/2001/XMLSchema#";
            public const string Xis = "http://www.w3.org/2001/04/infoset#";
            public const string Owl = "http://www.w3.org/2002/07/owl#";
            public const string Dt = "http://dbpedia.org/datatype/";
            public const string Dbo = "http://dbpedia.org/ontology/";
            public const string Urim = "http://purl.org/uri4uri/mime/";
            public const string Uris = "http://purl.org/uri4uri/suffix/";
            public const string Cert = "http://www.w3.org/ns/auth/cert#";
            public const string File = "file:///";
            public const string At = "http://archive.data.is4.site/terms/";
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
