using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.UserModel;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Excel (XSLT) document format, producing instances of <see cref="XSSFWorkbook"/>.
    /// </summary>
    public class ExcelXmlDocumentFormat : OpenXmlDocumentFormat<XSSFWorkbook>
    {
        public ExcelXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx")
        {

        }

        public override string GetExtension(XSSFWorkbook value)
        {
            return value?.SpreadsheetVersion?.DefaultExtension ?? Extension;
        }

        protected override XSSFWorkbook Open(OPCPackage package)
        {
            return new XSSFWorkbook(package);
        }
    }
}
