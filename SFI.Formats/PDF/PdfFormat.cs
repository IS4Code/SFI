using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Portable Document Format, producing instances of <see cref="PdfDocument"/>.
    /// </summary>
    [Description("Represents the Portable Document Format.")]
    public class PdfFormat : SignatureFormat<PdfDocument>
    {
        /// <summary>
        /// The opening mode used for matched PDF documents.
        /// </summary>
        [Description("The opening mode used for matched PDF documents.")]
        public PdfDocumentOpenMode OpenMode { get; set; } = PdfDocumentOpenMode.InformationOnly;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string?, string?)"/>
        public PdfFormat() : base("%PDF", "application/pdf", "pdf")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<PdfDocument, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var doc = PdfReader.Open(stream, OpenMode);
            return await resultFactory(doc, args);
        }
    }
}
