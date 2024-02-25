using NPOI;
using NPOI.OpenXml4Net.OPC;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the PowerPoint (PPTX) document format, producing instances of <see cref="POIXMLDocument"/>.
    /// </summary>
    [Description("Represents the PowerPoint (PPTX) document format.")]
    public class PowerPointXmlDocumentFormat : OpenXmlDocumentFormat<POIXMLDocument>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PowerPointXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx")
        {

        }

        /// <inheritdoc/>
        protected override POIXMLDocument Open(OPCPackage package)
        {
            return new Presentation(package);
        }

        class Presentation : POIXMLDocument
        {
            public Presentation(OPCPackage package) : base(package)
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
