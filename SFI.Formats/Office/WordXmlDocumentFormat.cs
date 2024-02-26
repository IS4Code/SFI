using NPOI.OpenXml4Net.OPC;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Word (DOCX) document format, producing instances of <see cref="XWPFDocument"/>.
    /// </summary>
    [Description("Represents the Word (DOCX) document format.")]
    public class WordXmlDocumentFormat : OpenXmlDocumentFormat<XWPFDocument>
    {
        static readonly Dictionary<string, (string, string)> recognized = BuildOfficeFormats(
            new[]
            {
                ("wordprocessingml.document", "docx"),
                ("wordprocessingml.template", "dotx")
            },
            new[]
            {
                ("word.document", "docm"),
                ("word.template", "dotm")
            }
        );

        /// <inheritdoc/>
        public override IReadOnlyDictionary<string, (string MediaType, string Extension)> RecognizedTypes => recognized;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WordXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx")
        {

        }

        /// <inheritdoc/>
        protected override XWPFDocument Open(OPCPackage package)
        {
            return new XWPFDocument(package);
        }
    }
}
