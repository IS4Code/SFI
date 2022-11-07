using NPOI.OpenXml4Net.OPC;
using NPOI.XWPF.UserModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Word (DOCX) document format, producing instances of <see cref="XWPFDocument"/>.
    /// </summary>
    public class WordXmlDocumentFormat : OpenXmlDocumentFormat<XWPFDocument>
    {
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
