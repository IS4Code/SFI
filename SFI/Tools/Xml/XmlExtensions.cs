using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tools.Xml
{
    /// <summary>
    /// Stores useful methods for handling XML objects.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Asynchronously initializes an instance of <see cref="XmlDocument"/>
        /// from an XML reader, similarly to <see cref="XmlDocument.Load(XmlReader)"/>.
        /// </summary>
        /// <param name="document">The XML document instance to use.</param>
        /// <param name="reader">The XML reader to read the data from.</param>
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
