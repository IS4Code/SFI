﻿using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Tools;
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

        public SvgFormat() : base("-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", new Uri("http://www.w3.org/2000/svg", UriKind.Absolute), "image/svg+xml", "svg")
        {

        }

        public override TResult Match<TResult>(XmlReader reader, XDocumentType docType, Func<SvgDocument, TResult> resultFactory)
        {
            if(!reader.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase)) return null;
            reader = new InitialXmlReader(reader);
            var doc = open(reader);
            if(doc == null) return null;
            return resultFactory(doc);
        }
    }
}