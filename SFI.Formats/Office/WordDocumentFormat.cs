using NPOI.HWPF;
using NPOI.POIFS.FileSystem;
using System.ComponentModel;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Word (DOC) document format, producing instances of <see cref="HWPFDocument"/>.
    /// </summary>
    [Description("Represents the Word (DOC) document format.")]
    public class WordDocumentFormat : OleDocumentFormat<HWPFDocument>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public WordDocumentFormat() : base("application/msword", "doc")
        {
            
        }

        /// <inheritdoc/>
        protected override HWPFDocument Open(NPOIFSFileSystem fileSystem)
        {
            return new HWPFDocument(fileSystem.Root);
        }
    }
}
