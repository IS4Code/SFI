using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace IS4.SFI.RDF
{
    /// <summary>
    /// Provides the RDFa Core Initial Context to assign default namespaces.
    /// </summary>
    public static class RdfAInitialContext
    {
        static readonly Task<Context> loadedContext;

        /// <summary>
        /// Stores the context as the value of the RDFa "prefix" attribute.
        /// </summary>
        public static readonly Task<string> PrefixString;

        /// <summary>
        /// Specifies the JSON-LD document used to load the context.
        /// </summary>
        public const string ContextSource = "https://www.w3.org/2013/json-ld-context/rdfa11";

        /// <summary>
        /// Adds the namespaces to an instance of <see cref="INamespaceMapper"/>.
        /// </summary>
        /// <param name="namespaces">The instance to use for defining the namespaces.</param>
        public static async ValueTask Add(INamespaceMapper namespaces)
        {
            var context = await loadedContext;
            foreach(var pair in context.Mapping)
            {
                namespaces.AddNamespace(pair.Key, new Uri(pair.Value, UriKind.Absolute));
            }
        }

        static RdfAInitialContext()
        {
            loadedContext = Context.Load();
            PrefixString = ConcatContext(loadedContext);
        }

        static async Task<string> ConcatContext(Task<Context> contextTask)
        {
            var context = await contextTask;
            return String.Join(" ", context.Mapping.Select(p => $"{p.Key}: {p.Value}"));
        }

        class Context
        {
            [JsonProperty("@context")]
            public Dictionary<string, string> Mapping { get; set; } = new();

            public static async Task<Context> Load()
            {
                Context? context;
                try{
                    using var client = new HttpClient();
                    using var stream = await client.GetStreamAsync(ContextSource);
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var serializer = new JsonSerializer();
                    context = serializer.Deserialize(reader, typeof(Context)) as Context;
                }catch{
                    context = null;
                }
                if(context == null)
                {
                    context = new()
                    {
                        Mapping = new Dictionary<string, string>
                        {
                            ["as"] = "https://www.w3.org/ns/activitystreams#",
                            ["cc"] = "http://creativecommons.org/ns#",
                            ["csvw"] = "http://www.w3.org/ns/csvw#",
                            ["ctag"] = "http://commontag.org/ns#",
                            ["dc"] = "http://purl.org/dc/terms/",
                            ["dc11"] = "http://purl.org/dc/elements/1.1/",
                            ["dcat"] = "http://www.w3.org/ns/dcat#",
                            ["dcterms"] = "http://purl.org/dc/terms/",
                            ["dqv"] = "http://www.w3.org/ns/dqv#",
                            ["duv"] = "https://www.w3.org/ns/duv#",
                            ["foaf"] = "http://xmlns.com/foaf/0.1/",
                            ["gr"] = "http://purl.org/goodrelations/v1#",
                            ["grddl"] = "http://www.w3.org/2003/g/data-view#",
                            ["ical"] = "http://www.w3.org/2002/12/cal/icaltzd#",
                            ["jsonld"] = "http://www.w3.org/ns/json-ld#",
                            ["ldp"] = "http://www.w3.org/ns/ldp#",
                            ["ma"] = "http://www.w3.org/ns/ma-ont#",
                            ["oa"] = "http://www.w3.org/ns/oa#",
                            ["odrl"] = "http://www.w3.org/ns/odrl/2/",
                            ["og"] = "http://ogp.me/ns#",
                            ["org"] = "http://www.w3.org/ns/org#",
                            ["owl"] = "http://www.w3.org/2002/07/owl#",
                            ["prov"] = "http://www.w3.org/ns/prov#",
                            ["qb"] = "http://purl.org/linked-data/cube#",
                            ["rdf"] = "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                            ["rdfa"] = "http://www.w3.org/ns/rdfa#",
                            ["rdfs"] = "http://www.w3.org/2000/01/rdf-schema#",
                            ["rev"] = "http://purl.org/stuff/rev#",
                            ["rif"] = "http://www.w3.org/2007/rif#",
                            ["rr"] = "http://www.w3.org/ns/r2rml#",
                            ["schema"] = "http://schema.org/",
                            ["sd"] = "http://www.w3.org/ns/sparql-service-description#",
                            ["sioc"] = "http://rdfs.org/sioc/ns#",
                            ["skos"] = "http://www.w3.org/2004/02/skos/core#",
                            ["skosxl"] = "http://www.w3.org/2008/05/skos-xl#",
                            ["sosa"] = "http://www.w3.org/ns/sosa/",
                            ["ssn"] = "http://www.w3.org/ns/ssn/",
                            ["time"] = "http://www.w3.org/2006/time#",
                            ["v"] = "http://rdf.data-vocabulary.org/#",
                            ["vcard"] = "http://www.w3.org/2006/vcard/ns#",
                            ["void"] = "http://rdfs.org/ns/void#",
                            ["wdr"] = "http://www.w3.org/2007/05/powder#",
                            ["wdrs"] = "http://www.w3.org/2007/05/powder-s#",
                            ["xhv"] = "http://www.w3.org/1999/xhtml/vocab#",
                            ["xml"] = "http://www.w3.org/XML/1998/namespace",
                            ["xsd"] = "http://www.w3.org/2001/XMLSchema#",
                            ["describedby"] = "http://www.w3.org/2007/05/powder-s#describedby",
                            ["license"] = "http://www.w3.org/1999/xhtml/vocab#license",
                            ["role"] = "http://www.w3.org/1999/xhtml/vocab#role"
                        }
                    };
                }
                return context;
            }
        }
    }
}
