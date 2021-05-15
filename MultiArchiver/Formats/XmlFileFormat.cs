using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace IS4.MultiArchiver.Formats
{
    public class XmlFileFormat : BinaryFileFormat<XmlReader>
    {
        public XmlReaderSettings ReaderSettings { get; } = new XmlReaderSettings
        {
            CloseInput = false,
            DtdProcessing = DtdProcessing.Parse,
            ValidationType = ValidationType.None,
            XmlResolver = new XmlPlaceholderResolver()
        };

        public XmlFileFormat(string mediaType = "application/xml", string extension = "xml") : base(DataTools.MaxBomLength + 1, mediaType, extension)
        {

        }

        public override bool CheckHeader(Span<byte> header)
        {
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

        public override TResult Match<TResult>(Stream stream, Func<XmlReader, TResult> resultFactory)
        {
            using(var reader = XmlReader.Create(stream, ReaderSettings))
            {
                if(!reader.Read()) return null;
                return resultFactory(reader);
            }
        }

        class XmlPlaceholderResolver : XmlResolver
        {
            public string InstructionTarget { get; }

            readonly string resourcestring;
            readonly byte[] resource;

            public XmlPlaceholderResolver()
            {
                InstructionTarget = $"entity{Guid.NewGuid():N}";
                resourcestring = $"<?{InstructionTarget}?>";
                resource = Encoding.UTF8.GetBytes(resourcestring);
            }

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
                } else if(type.IsAssignableFrom(typeof(StringReader)))
                {
                    return GetEntityAsReader(absoluteUri, role);
                }
                throw new XmlException(null, new NotSupportedException());
            }

            public virtual MemoryStream GetEntityAsStream(Uri absoluteUri, string role)
            {
                return new MemoryStream(resource, false);
            }

            public virtual StringReader GetEntityAsReader(Uri absoluteUri, string role)
            {
                return new StringReader(resourcestring);
            }
        }
    }
}
