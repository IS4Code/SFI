using IS4.MultiArchiver.Services;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace IS4.MultiArchiver.Formats
{
    public class XmlFileFormat : FileFormat, IFileReader
    {
        static readonly XmlReaderSettings readerSettings = new XmlReaderSettings
        {
            CloseInput = false,
            DtdProcessing = DtdProcessing.Parse,
            ValidationType = ValidationType.None,
            XmlResolver = new XmlPlaceholderResolver()
        };

        public XmlFileFormat(string mediaType = "application/xml", string extension = "xml") : base(0, mediaType, extension)
        {

        }

        public override bool Match(Span<byte> header)
        {
            return true;
        }

        public IDisposable Match(Stream stream)
        {
            var reader = XmlReader.Create(stream, readerSettings);
            try{
                if(!reader.Read())
                {
                    reader.Dispose();
                    return null;
                }
            }catch{
                reader.Dispose();
                throw;
            }
            return reader;
        }

        public ILinkedNode Match(Stream stream, ILinkedNodeFactory nodeFactory)
        {
            using(var reader = XmlReader.Create(stream, readerSettings))
            {
                return nodeFactory.Create(reader);
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
                    return CreatePublicId(relativeUri);
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

            const string publicid = "publicid:";

            static readonly Regex pubIdRegex = new Regex(@"(^\s+|\s+$)|(\s+)|(\/\/)|(::)|([+:\/;'?#%])", RegexOptions.Compiled);

            static Uri CreatePublicId(string id)
            {
                return new Uri("urn:" + publicid + TranscribePublicId(id));
            }

            static string TranscribePublicId(string id)
            {
                return pubIdRegex.Replace(id, m => {
                    if(m.Groups[1].Success)
                    {
                        return "";
                    }else if(m.Groups[2].Success)
                    {
                        return "+";
                    }else if(m.Groups[3].Success)
                    {
                        return ":";
                    }else if(m.Groups[4].Success)
                    {
                        return ";";
                    }else{
                        return Uri.EscapeDataString(m.Value);
                    }
                });
            }
        }
    }
}
