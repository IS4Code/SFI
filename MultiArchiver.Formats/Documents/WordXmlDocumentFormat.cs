using NPOI.OpenXml4Net.OPC;
using NPOI.XWPF.UserModel;

namespace IS4.MultiArchiver.Formats
{
    public class WordXmlDocumentFormat : OpenXmlDocumentFormat<XWPFDocument>
    {
        public WordXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx")
        {

        }

        protected override XWPFDocument Open(OPCPackage package)
        {
            return new XWPFDocument(package);
        }
    }
}
