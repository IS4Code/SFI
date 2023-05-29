using HtmlAgilityPack;
using IS4.SFI.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the HTML format, producing instances of <see cref="HtmlDocument"/>.
    /// </summary>
    public class HtmlFormat : BinaryFileFormat<HtmlDocument>
    {
        /// <summary>
        /// Contains the encoding picked for HTML by default.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public HtmlFormat() : base(1, "text/html", "html")
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return header.Length > 0 && !isBinary;
        }

        /// <inheritdoc/>
        public override bool CheckHeader(ArraySegment<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            return header.Count > 0 && !isBinary;
        }

        /// <inheritdoc/>
        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<HtmlDocument, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            var doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = DefaultEncoding;
            doc.Load(stream, true);
            if(doc.DocumentNode.Element("html") == null)
            {
                return default;
            }
            return await resultFactory(doc, args);
        }
    }
}
