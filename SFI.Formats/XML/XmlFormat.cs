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

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<XmlReader, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var reader = XmlReader.Create(stream, ReaderSettings);
            if(!await reader.ReadAsync()) return default;
            while(reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
            {
                if(!await reader.ReadAsync()) return default;
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
