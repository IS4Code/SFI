using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.UserModel;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Excel (XLSX) document format, producing instances of <see cref="XSSFWorkbook"/>.
    /// </summary>
    [Description("Represents the Excel (XLSX) document format.")]
    public class ExcelXmlDocumentFormat : OpenXmlDocumentFormat<XSSFWorkbook>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string?, string?)"/>
        public ExcelXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx")
        {

        }

        /// <inheritdoc/>
        public override string? GetExtension(XSSFWorkbook value)
        {
            return value?.SpreadsheetVersion?.DefaultExtension ?? Extension;
        }

        /// <inheritdoc/>
        protected override XSSFWorkbook Open(OPCPackage package)
        {
            return new XSSFWorkbook(package);
        }
    }
}
