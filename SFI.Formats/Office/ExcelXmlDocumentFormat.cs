﻿using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.UserModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Excel (XSLT) document format, producing instances of <see cref="XSSFWorkbook"/>.
    /// </summary>
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
