﻿using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the XML file format, producing instances of <see cref="XmlReader"/>.
    /// </summary>
    public class XmlFileFormat : BinaryFileFormat<XmlReader>
    {
        /// <summary>
        /// The default settings used with <see cref="XmlReader.Create(Stream, XmlReaderSettings)"/>.
        /// The updated values are the following:
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="XmlReaderSettings.CloseInput"/></term>
        ///     <description>false</description>
        /// </item>
        /// <item>
        ///     <term><see cref="XmlReaderSettings.DtdProcessing"/></term>
        ///     <description><see cref="DtdProcessing.Parse"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="XmlReaderSettings.ValidationType"/></term>
        ///     <description><see cref="ValidationType.None"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="XmlReaderSettings.Async"/></term>
        ///     <description>true</description>
        /// </item>
        /// <item>
        ///     <term><see cref="XmlReaderSettings.XmlResolver"/></term>
        ///     <description>A custom resolver which does not open any external resources.</description>
        /// </item>
        /// </list>
        /// </summary>
        public XmlReaderSettings ReaderSettings { get; } = new XmlReaderSettings
        {
            CloseInput = false,
            DtdProcessing = DtdProcessing.Parse,
            ValidationType = ValidationType.None,
            Async = true,
            XmlResolver = new XmlPlaceholderResolver()
        };

        /// <summary>
        /// Creates a new instance of the format.
        /// </summary>
        /// <param name="mediaType">The common media type of the format.</param>
        /// <param name="extension">The common extension of the format.</param>
        public XmlFileFormat(string mediaType = "application/xml", string extension = "xml") : base(DataTools.MaxBomLength + 1, mediaType, extension)
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            if(isBinary) return false;
            header = header.Slice(DataTools.FindBom(header));
            if(header.Length == 0) return false;
            switch(header[0])
            {
                // Whitespace
                case (byte)'\t':
                case (byte)'\r':
                case (byte)'\n':
                case (byte)' ':
                // XML declaration
                case (byte)'<':
                    return true;
                default: return false;
            }
        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<XmlReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = XmlReader.Create(stream, ReaderSettings))
            {
                if(!await reader.ReadAsync()) return default;
                while(reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
                {
                    if(!await reader.ReadAsync()) return default;
                }
                return await resultFactory(reader, args);
            }
        }

        class XmlPlaceholderResolver : XmlResolver
        {
            public override bool SupportsType(Uri absoluteUri, Type type)
            {
                return type == null || type.IsAssignableFrom(typeof(MemoryStream)) || type.IsAssignableFrom(typeof(StringReader));
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                if(relativeUri.IndexOf("//", StringComparison.Ordinal) > 0 && !new Uri(relativeUri, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                {
                    return UriTools.CreatePublicId(relativeUri);
                }
                return base.ResolveUri(baseUri, relativeUri);
            }

            public override object GetEntity(Uri absoluteUri, string role, Type type)
            {
                if(absoluteUri == null) throw new ArgumentNullException(nameof(absoluteUri));

                if(type == null || type.IsAssignableFrom(typeof(MemoryStream)))
                {
                    return GetEntityAsStream(absoluteUri, role);
                }else if(type.IsAssignableFrom(typeof(StringReader)))
                {
                    return GetEntityAsReader(absoluteUri, role);
                }
                throw new XmlException(null, new NotSupportedException());
            }

#pragma warning disable 1998
            public override async Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                return GetEntity(absoluteUri, role, ofObjectToReturn);
            }
#pragma warning restore 1998

            public virtual MemoryStream GetEntityAsStream(Uri absoluteUri, string role)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(GetEntityAsString(absoluteUri, role)), false);
            }

            public virtual StringReader GetEntityAsReader(Uri absoluteUri, string role)
            {
                return new StringReader(GetEntityAsString(absoluteUri, role));
            }

            public virtual string GetEntityAsString(Uri absoluteUri, string role)
            {
                var target = $"entity{UriTools.UuidFromUri(absoluteUri):N}";
                return $"<?{target}?>";
            }
        }
    }
}
