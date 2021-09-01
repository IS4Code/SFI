using IS4.MultiArchiver.Tools.Xml;
using Svg;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Formats
{
    public class SvgFormat : XmlDocumentFormat<SvgDocument>
    {
        static readonly Func<XmlReader, SvgDocument> open = (Func<XmlReader, SvgDocument>)Delegate.CreateDelegate(typeof(Func<XmlReader, SvgDocument>), null,
            typeof(SvgDocument).GetMethod(nameof(SvgDocument.Open), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(XmlReader) }, null )
            .MakeGenericMethod(typeof(SvgDocument)));

        static SvgFormat()
        {
            SvgDocument.SkipGdiPlusCapabilityCheck = true;
        }

        public SvgFormat() : base("-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", new Uri("http://www.w3.org/2000/svg", UriKind.Absolute), "image/svg+xml", "svg")
        {

        }

        public override bool CheckDocument(XDocumentType docType, XmlReader rootReader)
        {
            return rootReader.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase);
        }

        public override TResult Match<TResult, TArgs>(XmlReader reader, XDocumentType docType, MatchContext context, ResultFactory<SvgDocument, TResult, TArgs> resultFactory, TArgs args)
        {
            reader = new InitialXmlReader(reader);
            var doc = open(reader);
            if(doc == null) return default;
            return resultFactory(doc, args);
        }
    }
}
