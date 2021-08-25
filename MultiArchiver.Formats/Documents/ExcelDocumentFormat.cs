using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;

namespace IS4.MultiArchiver.Formats
{
    public class ExcelDocumentFormat : OleDocumentFormat<HSSFWorkbook>
    {
        public ExcelDocumentFormat() : base("application/vnd.ms-excel", "xls")
        {

        }

        public override string GetExtension(HSSFWorkbook value)
        {
            return value?.SpreadsheetVersion?.DefaultExtension ?? Extension;
        }

        public override string GetMediaType(HSSFWorkbook value)
        {
            return value?.DocumentSummaryInformation?.ContentType ?? MediaType;
        }

        protected override HSSFWorkbook Open(NPOIFSFileSystem fileSystem)
        {
            return new HSSFWorkbook(fileSystem.Root, true);
        }
    }
}
