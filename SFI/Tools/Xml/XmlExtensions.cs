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
        public static async ValueTask LoadAsync(this XmlDocument document, XmlReader reader)
        {
            using XmlWriter writer = document.CreateNavigator().AppendChild();
            await CopyToAsync(reader, writer);
        }

        /// <summary>
        /// Asynchronously copies the contents of an <see cref="XmlReader"/>
        /// instance to an <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="reader">The XML reader to read the data from.</param>
        /// <param name="writer">The XML writer to copy the data to.</param>
        /// <remarks>Nodes of type <see cref="XmlNodeType.DocumentType"/> and <see cref="XmlNodeType.XmlDeclaration"/> are not copied.</remarks>
        public static async ValueTask CopyToAsync(this XmlReader reader, XmlWriter writer)
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
                        writer.WriteProcessingInstruction(reader.Name, await reader.GetValueAsync());
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


        /// <summary>
        /// Copies the contents of an <see cref="XmlReader"/>
        /// instance to an <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="reader">The XML reader to read the data from.</param>
        /// <param name="writer">The XML writer to copy the data to.</param>
        /// <remarks>Nodes of type <see cref="XmlNodeType.DocumentType"/> and <see cref="XmlNodeType.XmlDeclaration"/> are not copied.</remarks>
        public static void CopyTo(this XmlReader reader, XmlWriter writer)
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
                        writer.WriteString(reader.Value);
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
                        writer.WriteWhitespace(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        writer.WriteFullEndElement();
                        break;
                }
            }while(reader.Read());
        }
    }
}
