using IS4.MultiArchiver.Services;
using MetadataExtractor.Formats.Xmp;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using XmpCore.Impl;
using XmpCore.Options;

namespace IS4.MultiArchiver.Formats.Metadata.MetadataReaders
{
    /// <summary>
    /// An implementation of <see cref="IMetadataReader{T}"/>
    /// of <see cref="XmpDirectory"/>.
    /// </summary>
    public class XmpReader : MetadataReader<XmpDirectory>
    {
        static readonly XmlQualifiedName MetaName = new XmlQualifiedName("xmpmeta", "adobe:ns:meta/");
        static readonly XmlQualifiedName RdfName = new XmlQualifiedName("RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        public SerializeOptions XmpSerializeOptions = new SerializeOptions
        {
            Indent = "",
            Newline = " "
        };

        public async override ValueTask<string> Describe(ILinkedNode node, XmpDirectory directory, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var serializer = new XmpSerializerRdf();
            using(var stream = new MemoryStream())
            {
                // Store as RDF/XML in <x:xmpmeta>
                serializer.Serialize(directory.XmpMeta, stream, XmpSerializeOptions);
                stream.Position = 0;
                using(var reader = XmlReader.Create(stream))
                {
                    if(reader.MoveToContent() == XmlNodeType.Element)
                    {
                        if(reader.NamespaceURI == MetaName.Namespace && reader.LocalName == MetaName.Name)
                        {
                            // The root element is <x:xmpmeta>
                            while(reader.Read())
                            {
                                if(reader.MoveToContent() == XmlNodeType.Element)
                                {
                                    if(reader.NamespaceURI == RdfName.Namespace && reader.LocalName == RdfName.Name)
                                    {
                                        // Found an <rdf:RDF> element
                                        node.Describe(reader);
                                        continue;
                                    }
                                    // Found an unknown element, skip
                                    reader.Skip();
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
