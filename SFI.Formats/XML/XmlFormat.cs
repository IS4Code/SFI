using IS4.SFI.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the XML file format, producing instances of <see cref="XmlReader"/>.
    /// </summary>
    [Description("Represents the XML file format.")]
    public class XmlFileFormat : BinaryFileFormat<XmlReader>
    {
        /// <summary>
        /// The default settings used with <see cref="XmlReader.Create(Stream, XmlReaderSettings)"/>.
        /// The updated values are the following:
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="XmlReaderSettings.CloseInput"/></term>
        ///     <description><see langword="false"/></description>
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
        ///     <term><see cref="XmlReaderSettings.MaxCharactersFromEntities"/></term>
        ///     <description>1024</description>
        /// </item>
        /// <item>
        ///     <term><see cref="XmlReaderSettings.Async"/></term>
        ///     <description><see langword="true"/></description>
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
            MaxCharactersFromEntities = 1024,
            Async = true,
            XmlResolver = new XmlPlaceholderResolver()
        };

        /// <summary>
        /// The processing of DTDs.
        /// </summary>
        [Description("The processing of DTDs.")]
        public DtdProcessing DtdProcessing {
            get => ReaderSettings.DtdProcessing;
            set => ReaderSettings.DtdProcessing = value;
        }

        /// <summary>
        /// Whether to perform validation or type assignment when reading.
        /// </summary>
        [Description("Whether to perform validation or type assignment when reading.")]
        public ValidationType ValidationType {
            get => ReaderSettings.ValidationType;
            set => ReaderSettings.ValidationType = value;
        }

        /// <summary>
        /// Whether to check invalid characters when reading.
        /// </summary>
        [Description("Whether to check invalid characters when reading.")]
        public bool CheckCharacters {
            get => ReaderSettings.CheckCharacters;
            set => ReaderSettings.CheckCharacters = value;
        }

        /// <summary>
        /// The maximum allowable number of characters from expanded entities.
        /// </summary>
        [Description("The maximum allowable number of characters from expanded entities.")]
        public long MaxCharactersFromEntities {
            get => ReaderSettings.MaxCharactersFromEntities;
            set => ReaderSettings.MaxCharactersFromEntities = value;
        }

        /// <summary>
        /// The maximum allowable number of characters in a document.
        /// </summary>
        [Description("The maximum allowable number of characters in a document.")]
        public long MaxCharactersInDocument {
            get => ReaderSettings.MaxCharactersInDocument;
            set => ReaderSettings.MaxCharactersInDocument = value;
        }

        /// <summary>
        /// Whether to prevent accepting files starting with <c>&lt;?php</c> as XML.
        /// </summary>
        [Description("Whether to prevent accepting files starting with '<?php' as XML.")]
        public bool IgnorePhp { get; set; } = true;

        /// <summary>
        /// Whether to prevent accepting <c>&lt;html&gt;</c> files as XML.
        /// </summary>
        [Description("Whether to prevent accepting '<html>' files as XML.")]
        public bool IgnoreHtml { get; set; } = true;

        static readonly byte[] phpSignaureBytes = Encoding.ASCII.GetBytes("?php");

        static readonly byte[] htmlElementBytes = Encoding.ASCII.GetBytes("html");

        static readonly byte[] htmlDoctypeBytes = Encoding.ASCII.GetBytes("!DOCTYPE html");

        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public XmlFileFormat(string mediaType = "application/xml", string extension = "xml") : base(DataTools.MaxBomLength + 1, mediaType, extension)
        {

        }

        /// <inheritdoc/>
        public override bool CheckHeader(ReadOnlySpan<byte> header, bool isBinary, IEncodingDetector? encodingDetector)
        {
            if(isBinary) return false;
            header = header.Slice(DataTools.FindBom(header));
            if(header.Length == 0) return false;
            bool maybe = false;
            for(int i = 0; i < header.Length; i++)
            {
                switch(header[i])
                {
                    // Whitespace
                    case (byte)'\t':
                    case (byte)'\r':
                    case (byte)'\n':
                    case (byte)' ':
                        maybe = true;
                        continue;
                    // XML declaration
                    case (byte)'<':
                        if(IgnorePhp)
                        {
                            if(FollowedByToken(header, i, phpSignaureBytes))
                            {
                                // PHP detected
                                return false;
                            }
                        }
                        if(IgnoreHtml)
                        {
                            if(FollowedByToken(header, i, htmlElementBytes) || FollowedByToken(header, i, htmlDoctypeBytes))
                            {
                                // HTML detected
                                return false;
                            }
                        }
                        return true;
                    default:
                        return false;
                }
            }
            return maybe;
        }

        static bool FollowedByToken(ReadOnlySpan<byte> header, int position, ReadOnlySpan<byte> span)
        {
            var next = header.Slice(position + 1);
            if(next.Length > span.Length && next.StartsWith(span))
            {
                // span follows with one byte after it
                var nextByte = next[span.Length];
                if(nextByte < 0x80 && !XmlConvert.IsNCNameChar((char)nextByte))
                {
                    // the byte is not a part of the name, so the name is complete
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<XmlReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = XmlReader.Create(stream, ReaderSettings);
            if(!await reader.ReadAsync()) return default;
            while(reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
            {
                if(!await reader.ReadAsync()) return default;
            }
            if(IgnorePhp)
            {
                if(reader.NodeType == XmlNodeType.ProcessingInstruction && reader.Name == "php")
                {
                    // PHP detected
                    return default;
                }
            }
            if(IgnoreHtml)
            {
                if(reader.NodeType == XmlNodeType.DocumentType && reader.Name.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    var publicId = reader.GetAttribute("PUBLIC");
                    var systemId = reader.GetAttribute("SYSTEM");
                    if(String.IsNullOrEmpty(publicId) && String.IsNullOrEmpty(systemId))
                    {
                        // plain HTML 5
                        return default;
                    }
                    if(systemId == "about:legacy-compat")
                    {
                        // legacy compatible HTML 5
                        return default;
                    }
                    if(publicId != null && (publicId.StartsWith("-//W3C//DTD HTML ", StringComparison.Ordinal) || publicId.StartsWith("-//IETF//DTD HTML ")))
                    {
                        // older HTML version
                        return default;
                    }
                }
                if(reader.NodeType == XmlNodeType.Element && String.IsNullOrEmpty(reader.NamespaceURI) && reader.Name.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    // HTML detected (no DOCTYPE)
                    return default;
                }
            }
            return await resultFactory(reader, args);
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
                throw new XmlException($"Only types {typeof(MemoryStream)} and {typeof(StringReader)} are supported.", new NotSupportedException());
            }

            public async override Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                return GetEntity(absoluteUri, role, ofObjectToReturn);
            }

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
