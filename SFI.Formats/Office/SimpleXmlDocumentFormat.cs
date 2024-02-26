using NPOI;
using NPOI.OpenXml4Net.OPC;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Provides a simple OOXML-based format implementation.
    /// </summary>
    [Description("Provides a simple OOXML-based format implementation.")]
    public class SimpleXmlDocumentFormat : OpenXmlDocumentFormat<POIXMLDocument>
    {
        static readonly Dictionary<string, (string, string)> recognized = BuildOfficeFormats(
            new[]
            {
                ("presentationml.presentation", "pptx"),
                ("presentationml.slideshow", "ppsx"),
                ("presentationml.template", "potx")
            },
            new[]
            {
                ("powerpoint.presentation", "pptm"),
                ("powerpoint.template", "potm")
            }
        );

        /// <inheritdoc/>
        public override IReadOnlyDictionary<string, (string MediaType, string Extension)> RecognizedTypes => recognized;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SimpleXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument", "ooxml")
        {
            
        }

        /// <inheritdoc/>
        protected override POIXMLDocument? Open(OPCPackage package)
        {
            return new Document(package);
        }

        class Document : POIXMLDocument
        {
            public Document(OPCPackage package) : base(package)
            {

            }

            public override List<PackagePart> GetAllEmbedds()
            {
                var embeds = new List<PackagePart>();
                var part = GetPackagePart();

                foreach(var rel in part.GetRelationshipsByType(OLE_OBJECT_REL_TYPE))
                {
                    embeds.Add(part.GetRelatedPart(rel));
                }

                foreach(var rel in part.GetRelationshipsByType(PACK_OBJECT_REL_TYPE))
                {
                    embeds.Add(part.GetRelatedPart(rel));
                }

                return embeds;
            }
        }
    }
}
