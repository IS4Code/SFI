using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Portable Document Format, producing instances of <see cref="PdfDocument"/>.
    /// </summary>
    public class PdfFormat : SignatureFormat<PdfDocument>
    {
        public PdfDocumentOpenMode OpenMode { get; } = PdfDocumentOpenMode.InformationOnly;

        public PdfFormat() : base("%PDF", "application/pdf", "pdf")
        {
        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<PdfDocument, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var doc = PdfReader.Open(stream, OpenMode))
            {
                return await resultFactory(doc, args);
            }
        }
    }
}
