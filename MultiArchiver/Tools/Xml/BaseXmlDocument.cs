using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    public class BaseXmlDocument : XmlDocument
    {
        string baseUri;

        public override string BaseURI => baseUri;

        public BaseXmlDocument(string baseUri)
        {
            this.baseUri = baseUri;
        }

        public BaseXmlDocument(string baseUri, XmlNameTable nameTable) : base(nameTable)
        {
            this.baseUri = baseUri;
        }

        public void SetBaseURI(string baseUri)
        {
            this.baseUri = baseUri;
        }
    }
}
