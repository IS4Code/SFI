using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Excel (XLSX) document format, producing instances of <see cref="XSSFWorkbook"/>.
    /// </summary>
    [Description("Represents the Excel (XLSX) document format.")]
    public class ExcelXmlDocumentFormat : OpenXmlDocumentFormat<XSSFWorkbook>
    {
        static readonly Dictionary<string, (string, string)> recognized = BuildOfficeFormats(
            new[]
            {
                ("spreadsheetml.sheet", "xlsx"),
                ("spreadsheetml.template", "xltx")
            },
            new[]
            {
                ("excel.sheet", "xlsm"),
                ("excel.template", "xltm"),
                ("excel.sheet.binary", "xlsb")
            }
        );

        /// <inheritdoc/>
        public override IReadOnlyDictionary<string, (string MediaType, string Extension)> RecognizedTypes => recognized;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string?, string?)"/>
        public ExcelXmlDocumentFormat() : base("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx")
        {

        }

        /// <inheritdoc/>
        protected override XSSFWorkbook Open(OPCPackage package)
        {
            return new XSSFWorkbook(package);
        }
    }
}
