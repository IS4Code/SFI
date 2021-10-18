using System.Threading.Tasks;
using System.Xml;

namespace IS4.MultiArchiver.Tools.Xml
{
    public static class XmlExtensions
    {
        public static async Task LoadAsync(this XmlDocument document, XmlReader reader)
        {
            using(XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                do{
                    switch(reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            writer.WriteAttributes(reader, true);
                            if(reader.IsEmptyElement)
                            {
                                writer.WriteEndElement();
                            }
                            break;
                        case XmlNodeType.Text:
                            writer.WriteString(await reader.GetValueAsync());
                            break;
                        case XmlNodeType.CDATA:
                            writer.WriteCData(reader.Value);
                            break;
                        case XmlNodeType.EntityReference:
                            writer.WriteEntityRef(reader.Name);
                            break;
                        case XmlNodeType.ProcessingInstruction:
                            writer.WriteProcessingInstruction(reader.Name, reader.Value);
                            break;
                        case XmlNodeType.Comment:
                            writer.WriteComment(reader.Value);
                            break;
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            writer.WriteWhitespace(await reader.GetValueAsync());
                            break;
                        case XmlNodeType.EndElement:
                            writer.WriteFullEndElement();
                            break;
                    }
                }while(await reader.ReadAsync());
            }
        }
    }
}
