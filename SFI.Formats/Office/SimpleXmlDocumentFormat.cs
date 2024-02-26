using NPOI;
using NPOI.OpenXml4Net.OPC;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Provides a simple OOXML-based format implementation.
    /// </summary>
    public class SimpleXmlDocumentFormat : OpenXmlDocumentFormat<POIXMLDocument>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SimpleXmlDocumentFormat(string mediaType, string extension) : base(mediaType, extension)
        {

        }

        /// <inheritdoc/>
        protected override POIXMLDocument Open(OPCPackage package)
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

    /// <summary>
    /// Represents the PowerPoint (PPTX) document format.
    /// </summary>
    [Description("Represents the PowerPoint (PPTX) document format.")]
    public class PowerPointXmlDocumentFormat : SimpleXmlDocumentFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PowerPointXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx")
        {

        }
    }

    /// <summary>
    /// Represents the PowerPoint Show (PPSX) document format.
    /// </summary>
    [Description("Represents the PowerPoint Show (PPSX) document format.")]
    public class PowerPointShowXmlDocumentFormat : SimpleXmlDocumentFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PowerPointShowXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.presentationml.slideshow", "ppsx")
        {

        }
    }

    /// <summary>
    /// Represents the PowerPoint Template (POTX) document format.
    /// </summary>
    [Description("Represents the PowerPoint Template (POTX) document format.")]
    public class PowerPointTemplateXmlDocumentFormat : SimpleXmlDocumentFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PowerPointTemplateXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.presentationml.template", "potx")
        {

        }
    }

    /// <summary>
    /// Represents the Excel Template (XLTX) document format.
    /// </summary>
    [Description("Represents the Excel Template (XLTX) document format.")]
    public class ExcelTemplateXmlDocumentFormat : SimpleXmlDocumentFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ExcelTemplateXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.spreadsheetml.template", "xltx")
        {

        }
    }

    /// <summary>
    /// Represents the Word Template (XLTX) document format.
    /// </summary>
    [Description("Represents the Excel Template (XLTX) document format.")]
    public class WordTemplateXmlDocumentFormat : SimpleXmlDocumentFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WordTemplateXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.wordprocessingml.template", "dotx")
        {

        }
    }
}
