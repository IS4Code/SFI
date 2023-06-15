using IS4.SFI.Tools.Xml;
using Svg;
using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the SVG image format, producing instances of <see cref="SvgDocument"/>.
    /// </summary>
    public class SvgFormat : XmlDocumentFormat<SvgDocument>
    {
        static SvgFormat()
        {
            SvgDocument.SkipGdiPlusCapabilityCheck = true;
        }

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string?, string?)"/>
        public SvgFormat() : base("-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", new Uri("http://www.w3.org/2000/svg", UriKind.Absolute), "image/svg+xml", "svg")
        {

        }

        /// <inheritdoc/>
        public override bool CheckDocument(XDocumentType? docType, XmlReader rootReader)
        {
            return rootReader.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(XmlReader reader, XDocumentType? docType, MatchContext context, ResultFactory<SvgDocument, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            reader = new InitialXmlReader(reader);
            var doc = SvgDocument.Open<SvgDocument>(reader);
            if(doc == null) return default;
            return await resultFactory(doc, args);
        }
    }
}
