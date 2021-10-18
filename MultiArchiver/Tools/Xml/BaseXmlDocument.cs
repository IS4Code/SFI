using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    public class BaseXmlDocument : XmlDocument
    {
        public override string BaseURI { get; }

        public BaseXmlDocument(string baseUri)
        {
            BaseURI = baseUri;
        }

        public BaseXmlDocument(string baseUri, XmlNameTable nameTable) : base(nameTable)
        {
            BaseURI = baseUri;
        }
    }
}
