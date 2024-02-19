using IS4.SFI.Services;
using IS4.SFI.Tools.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the XML document and/or parsed entity file format, producing instances of <see cref="XmlReader"/>.
    /// </summary>
    [Description("Represents the XML document and/or parsed entity file format.")]
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
        /// Whether to ignore comments.
        /// </summary>
        [Description("Whether to ignore comments.")]
        public bool IgnoreComments {
            get => ReaderSettings.IgnoreComments;
            set => ReaderSettings.IgnoreComments = value;
        }

        /// <summary>
        /// Whether to ignore processing instructions.
        /// </summary>
        [Description("Whether to ignore processing instructions.")]
        public bool IgnoreProcessingInstructions {
            get => ReaderSettings.IgnoreProcessingInstructions;
            set => ReaderSettings.IgnoreProcessingInstructions = value;
        }

        /// <summary>
        /// Whether to ignore insignificant white space.
        /// </summary>
        [Description("Whether to ignore insignificant white space.")]
        public bool IgnoreWhitespace {
            get => ReaderSettings.IgnoreWhitespace;
            set => ReaderSettings.IgnoreWhitespace = value;
        }

        /// <summary>
        /// The level of conformance which the reader will comply.
        /// </summary>
        [Description("The level of conformance which the reader will comply.")]
        public ConformanceLevel ConformanceLevel {
            get => ReaderSettings.ConformanceLevel;
            set => ReaderSettings.ConformanceLevel = value;
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

        /// <summary>
        /// The method used for the processing of XML data before it is analyzed.
        /// </summary>
        [Description("The method used for the processing of XML data before it is analyzed.")]
        public XmlProcessingMethod ProcessingMethod { get; set; }

        /// <summary>
        /// The default encoding to use for reading the texts.
        /// </summary>
        [Description("The default encoding to use for reading the texts.")]
        public Encoding? DefaultEncoding { get; set; }

        /// <summary>
        /// Whether to use the detected encoding of the text file, if there is any.
        /// </summary>
        [Description("Whether to use the detected encoding of the text file, if there is any.")]
        public bool UseDetectedEncoding { get; set; }

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
                var b = header[i];
                switch(b)
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
                        if(ReaderSettings.ConformanceLevel == ConformanceLevel.Document)
                        {
                            // Text characters not allowed outside before first tag
                            return false;
                        }
                        // Might be content
                        if(b < 0x80 && !XmlConvert.IsXmlChar((char)b))
                        {
                            // Is ASCII but not a character allowed in XML data
                            return false;
                        }
                        break;
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
            var encoding = DefaultEncoding;
            if(UseDetectedEncoding && context.GetService<IEncodingDetector>() is { Charset: var charset and not (null or "") })
            {
                try{
                    encoding = Encoding.GetEncoding(charset, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                }catch(ArgumentException e)
                {
                    // Bad detected encoding is not a format matching exception
                    throw new InternalApplicationException(e);
                }
            }
            var settings = ReaderSettings;
            if(settings.ConformanceLevel == ConformanceLevel.Auto)
            {
                // Clone to receive the updated ConformanceLevel
                settings = settings.Clone();
            }
            long startPosition =
                // Can be reset
                ProcessingMethod == XmlProcessingMethod.TwoPass && stream.CanSeek
                ? stream.Position : -1;
            using var reader = await CreateReader(settings);
            if(reader == null)
            {
                return default;
            }
            if(IgnorePhp && IsReaderAtPhp(reader))
            {
                return default;
            }
            if(IgnoreHtml && IsReaderAtHtml(reader))
            {
                return default;
            }
            switch(reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                    switch(settings.ConformanceLevel)
                    {
                        case ConformanceLevel.Document:
                            // Non-element data found in document
                            return default;
                        case ConformanceLevel.Auto:
                            // This is now a fragment
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            break;
                    }
                    break;
                case XmlNodeType.DocumentType:
                    switch(settings.ConformanceLevel)
                    {
                        case ConformanceLevel.Fragment:
                            // DOCTYPE found in fragment
                            return default;
                        case ConformanceLevel.Auto:
                            // This is now a document
                            settings.ConformanceLevel = ConformanceLevel.Document;
                            break;
                    }
                    break;
            }
            switch(ProcessingMethod)
            {
                case XmlProcessingMethod.PreLoad:
                    using(var preloadedReader = await PreloadXmlReader(reader, settings))
                    {
                        if(preloadedReader == null)
                        {
                            return default;
                        }
                        return await resultFactory(preloadedReader, args);
                    }
                case XmlProcessingMethod.TwoPass:
                    if(startPosition == -1)
                    {
                        // Cannot seek; fallback
                        goto case XmlProcessingMethod.PreLoad;
                    }
                    settings = await FirstPassXmlRead(reader, settings);
                    if(settings == null)
                    {
                        // Not matched
                        return default;
                    }
                    // Restart
                    stream.Position = startPosition;
                    using(var secondReader = await CreateReader(settings))
                    {
                        if(secondReader == null)
                        {
                            throw new InternalApplicationException(new ArgumentException("XML data was modified before the second pass.", nameof(stream)));
                        }
                        return await resultFactory(secondReader, args);
                    }
                default:
                    // Might remain at ConformanceLevel.Auto, effectively signalling a fragment
                    return await resultFactory(reader, args);
            }

            async ValueTask<XmlReader?> CreateReader(XmlReaderSettings settings)
            {
                var reader = encoding != null
                    ? XmlReader.Create(new StreamReader(stream, encoding, true, 1024, true), settings)
                    : XmlReader.Create(stream, settings);
                if(!await reader.ReadAsync()) return null;
                while(reader.NodeType is XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace)
                {
                    if(!await reader.ReadAsync()) return null;
                }
                return reader;
            }
        }

        /// <summary>
        /// Performs a one pass of reading from an <see cref="XmlReader"/> instance.
        /// </summary>
        /// <param name="reader">The input XML reader.</param>
        /// <param name="settings">The settings used for reading.</param>
        /// <returns>Modified <paramref name="settings"/> to use for second-pass reading, or <see langword="null"/> on failure.</returns>
        /// <exception cref="XmlException">An arbitrary exception from parsing the XML.</exception>
        protected async virtual ValueTask<XmlReaderSettings?> FirstPassXmlRead(XmlReader reader, XmlReaderSettings settings)
        {
            // A single root element is needed for documents
            bool foundElement = reader.NodeType == XmlNodeType.Element;
            // A presence of any markup is needed for non-documents to be interesting
            bool foundMarkup = foundElement || settings.ConformanceLevel == ConformanceLevel.Document;
            // Read to the end (verify conformance)
            while(await reader.ReadAsync())
            {
                if(reader.Depth != 0)
                {
                    // Nothing useful in an element
                    continue;
                }
                switch(settings.ConformanceLevel)
                {
                    case ConformanceLevel.Auto:
                        // Detecting conformance level
                        switch(reader.NodeType)
                        {
                            case XmlNodeType.CDATA:
                            case XmlNodeType.EntityReference:
                            case XmlNodeType.Text:
                                // Content found outside an element
                                settings.ConformanceLevel = ConformanceLevel.Fragment;
                                break;
                            case XmlNodeType.Element:
                                if(!foundElement)
                                {
                                    foundElement = true;
                                }else{
                                    // Found another element
                                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                                }
                                break;
                        }
                        goto case ConformanceLevel.Fragment;
                    case ConformanceLevel.Fragment:
                        if(foundMarkup)
                        {
                            break;
                        }
                        // Detecting the presence of any markup at all
                        switch(reader.NodeType)
                        {
                            case XmlNodeType.CDATA:
                            case XmlNodeType.EntityReference:
                            case XmlNodeType.Element:
                            case XmlNodeType.Comment:
                            case XmlNodeType.ProcessingInstruction:
                                foundMarkup = true;
                                break;
                        }
                        break;
                }
            }
            if(!foundMarkup)
            {
                // This is just text or whitespace, nothing useful at all
                return null;
            }
            if(!foundElement)
            {
                // No element whatsoever, so this is just a fragment
                settings.ConformanceLevel = ConformanceLevel.Fragment;
            }
            if(settings.ConformanceLevel == ConformanceLevel.Auto)
            {
                // No indication of this being a fragment, so treat it as a document
                settings.ConformanceLevel = ConformanceLevel.Document;
            }
            var nametable = reader.NameTable;
            // Old reader no longer needed
            reader.Close();
            if(nametable != null)
            {
                // Re-use nametable
                if(settings == ReaderSettings)
                {
                    // Clone if not already duplicated
                    settings = settings.Clone();
                }
                settings.NameTable = nametable;
            }
            return settings;
        }

        /// <summary>
        /// Loads fully the XML document from <paramref name="input"/> and returns
        /// a reader to the cached document.
        /// </summary>
        /// <param name="input">The input forward-only XML reader.</param>
        /// <param name="settings">The settings used for reading.</param>
        /// <returns>The resulting <see cref="XmlReader"/> instance reading from the document, or <see langword="null"/> on failure.</returns>
        /// <exception cref="XmlException">An arbitrary exception from parsing the XML.</exception>
        protected async virtual ValueTask<XmlReader?> PreloadXmlReader(XmlReader input, XmlReaderSettings settings)
        {
            var readerSequence = new List<XmlReader>();

            // Look for DOCTYPE or any content
            while(input.NodeType is not (XmlNodeType.DocumentType or XmlNodeType.Element or XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.EntityReference))
            {
                // Preserve everything up that point
                readerSequence.Add(new XmlReaderState(input));
                if(await input.ReadAsync())
                {
                    continue;
                }
                // At the end with no content text/element or DOCTYPE
                switch(settings.ConformanceLevel)
                {
                    case ConformanceLevel.Document:
                        // Document needs DOCTYPE or element
                        return null;
                    case ConformanceLevel.Auto:
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                        break;
                }
                if(!readerSequence.Any(state => state.NodeType is XmlNodeType.Comment or XmlNodeType.ProcessingInstruction))
                {
                    // Just whitespace, nothing useful
                    return null;
                }
                // Just retrieve what has been read so far
                var sequenceReader = new SequenceXmlReader(readerSequence);
                // Move to the original inital state
                if(!await sequenceReader.ReadAsync()) return null;
                return sequenceReader;
            }
            if(input.NodeType == XmlNodeType.DocumentType)
            {
                switch(settings.ConformanceLevel)
                {
                    case ConformanceLevel.Fragment:
                        // No DOCTYPE valid in a fragment
                        return null;
                    case ConformanceLevel.Auto:
                        settings.ConformanceLevel = ConformanceLevel.Document;
                        break;
                }
                // Preserve the DOCTYPE too
                readerSequence.Add(new XmlReaderState(input));
            }

            // readerSequence now contains all comments and PIs before the DOCTYPE or the first content node, and the declaration.
            // input is positioned either at DOCTYPE (since it affects the document reading) or the first content node.
            
            var doc = new XmlDocument(input.NameTable);
            // Base document reader
            XmlReader reader;
            if(settings.ConformanceLevel == ConformanceLevel.Document)
            {
                // Just load the rest into the document
                await doc.LoadAsync(input);
                reader = new XmlNodeReader(doc);
            }else{
                // Store it in a fragment node
                var fragment = doc.CreateDocumentFragment();
                using(var writer = fragment.CreateNavigator().AppendChild())
                {
                    await input.CopyToAsync(writer);
                }
                if(settings.ConformanceLevel == ConformanceLevel.Auto)
                {
                    // Now we can determine which it is
                    var childNodes = fragment.ChildNodes.OfType<XmlNode>();
                    if(childNodes.Where(n => n.NodeType is XmlNodeType.Element).Take(2).Count() != 1)
                    {
                        // No or multiple elements - fragment
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                    }else if(childNodes.Any(n => n.NodeType is XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.EntityReference))
                    {
                        // Character data outside the element
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                    }else{
                        // Valid document
                        settings.ConformanceLevel = ConformanceLevel.Document;
                    }
                }
                reader = new XmlNodeReader(fragment);
            }
            // Async support
            reader = new XmlReaderAsyncWrapper(reader);
            // Apply settings (if any)
            if(settings.ConformanceLevel is var conformanceLevel and not (ConformanceLevel.Auto or ConformanceLevel.Document))
            {
                // Briefly swap out the conformance level to prevent errors
                if(settings == ReaderSettings)
                {
                    settings = settings.Clone();
                }
                settings.ConformanceLevel = ConformanceLevel.Auto;
                try{
                    reader = XmlReader.Create(reader, settings);
                }finally{
                    settings.ConformanceLevel = conformanceLevel;
                }
            }else{
                reader = XmlReader.Create(reader, settings);
            }
            if(!await reader.ReadAsync()) return null;
            // Now positioned at anything following the DOCTYPE or at first content node
            readerSequence.Add(reader);
            // Concat with the preceding states
            reader = new SequenceXmlReader(readerSequence, settings);
            // Move to the original inital state
            if(!await reader.ReadAsync()) return null;
            return reader;
        }

        /// <summary>
        /// Checks whether <see cref="XmlReader"/> is at an HTML document.
        /// </summary>
        /// <param name="reader">The XML reader to check.</param>
        /// <returns><see langword="true"/> if <paramref name="reader"/> is located at HTML DOCTYPE or root element, <see langword="false"/> otherwise.</returns>
        protected virtual bool IsReaderAtHtml(XmlReader reader)
        {
            if(reader.NodeType == XmlNodeType.DocumentType && reader.Name.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
                var publicId = reader.GetAttribute("PUBLIC");
                var systemId = reader.GetAttribute("SYSTEM");
                if(String.IsNullOrEmpty(publicId) && String.IsNullOrEmpty(systemId))
                {
                    // plain HTML 5
                    return true;
                }
                if(systemId == "about:legacy-compat")
                {
                    // legacy compatible HTML 5
                    return true;
                }
                if(publicId != null && (publicId.StartsWith("-//W3C//DTD HTML ", StringComparison.Ordinal) || publicId.StartsWith("-//IETF//DTD HTML ")))
                {
                    // older HTML version
                    return true;
                }
            }
            if(reader.NodeType == XmlNodeType.Element && reader.Depth == 0 && String.IsNullOrEmpty(reader.NamespaceURI) && reader.Name.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
                // HTML detected (no DOCTYPE)
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether <see cref="XmlReader"/> is at PHP code.
        /// </summary>
        /// <param name="reader">The XML reader to check.</param>
        /// <returns><see langword="true"/> if <paramref name="reader"/> is located at a PHP declaration, <see langword="false"/> otherwise.</returns>
        protected virtual bool IsReaderAtPhp(XmlReader reader)
        {
            if(reader.NodeType == XmlNodeType.ProcessingInstruction && reader.Name == "php")
            {
                // PHP detected
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <c>application/xml-external-parsed-entity</c> is reported if the reader is not set to reading a document.
        /// </remarks>
        public override string? GetMediaType(XmlReader value)
        {
            return IsReadingFragment(value) ? "application/xml-external-parsed-entity" : base.GetMediaType(value);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <c>ent</c> is reported if the reader is not set to reading a document.
        /// </remarks>
        public override string? GetExtension(XmlReader value)
        {
            return IsReadingFragment(value) ? "ent" : base.GetExtension(value);
        }

        private bool IsReadingFragment(XmlReader reader)
        {
            return reader.Settings?.ConformanceLevel != ConformanceLevel.Document;
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

    /// <summary>
    /// Defines the method used for the processing of XML data before it is analyzed.
    /// </summary>
    public enum XmlProcessingMethod
    {
        /// <summary>
        /// No pre-parsing is used.
        /// </summary>
        [Description("No pre-parsing is used.")]
        None,

        /// <summary>
        /// If possible, the <see cref="XmlReader"/> is constructed twice: first to validate the input, then to analyze it.
        /// </summary>
        [Description("If possible, the XML reader is constructed twice: first to validate the input, then to analyze it.")]
        TwoPass,

        /// <summary>
        /// The XML data is loaded fully into memory before analysis.
        /// </summary>
        [Description("The XML data is loaded fully into memory before analysis.")]
        PreLoad,
    }
}
