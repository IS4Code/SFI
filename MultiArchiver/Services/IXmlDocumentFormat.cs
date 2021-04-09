using System.Xml;
using System.Xml.Linq;

namespace IS4.MultiArchiver.Services
{
    public interface IXmlDocumentFormat
    {
        ILinkedNode Match(XmlReader reader, XDocumentType docType, ILinkedNodeFactory nodeFactory);
    }
}
