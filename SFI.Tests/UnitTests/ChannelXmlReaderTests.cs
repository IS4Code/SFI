using IS4.SFI.Tools.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="ChannelXmlReader"/> and <see cref="XmlReaderState"/>.
    /// </summary>
    [TestClass]
    public class ChannelXmlReaderTests
    {
        const string sampleXml = @"<?xml version=""1.0""?>
<!-- comment -->
<element attribute=""value"">
  Text content
  <element2 xmlns=""urn:publicid:test"" xmlns:other=""urn:publicid:other"" other:attr=""value2"">
    <?pi content "" ""?>
    <!-- comment2 -->
  </element2>
</element>
";

        /// <summary>
        /// Tests that <see cref="ChannelXmlReader"/> produced from
        /// a sequence of <see cref="XmlReaderState"/> results
        /// in equivalent XML.
        /// </summary>
        [DataRow(sampleXml)]
        [TestMethod]
        public async Task ReconstructedXmlEquivalent(string xml)
        {
            ChannelXmlReader channelReader;

            using(var reader = XmlReader.Create(new StringReader(xml)))
            {
                channelReader = ChannelXmlReader.Create(reader, out var writer);

                foreach(var state in XmlReaderState.ReadFrom(reader))
                {
                    await writer.WriteAsync(state);
                }

                writer.Complete();
            }

            var expected = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            var actual = XDocument.Load(channelReader, LoadOptions.SetLineInfo);
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        /// <summary>
        /// Tests that <see cref="ChannelXmlReader"/> produced from
        /// a sequence of <see cref="XmlReaderState"/> results
        /// in equivalent XML, using asynchronous reading of XML.
        /// </summary>
        [DataRow(sampleXml)]
        [TestMethod]
        public async Task ReconstructedXmlEquivalentAsync(string xml)
        {
            ChannelXmlReader channelReader;

            using(var reader = XmlReader.Create(new StringReader(xml)))
            {
                channelReader = ChannelXmlReader.Create(reader, out var writer);

                foreach(var state in XmlReaderState.ReadFrom(reader))
                {
                    await writer.WriteAsync(state);
                }

                writer.Complete();
            }

            var expected = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            var actual = await XDocument.LoadAsync(channelReader, LoadOptions.SetLineInfo, CancellationToken.None);
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        /// <summary>
        /// Tests that <see cref="XmlReaderState.ReadFrom(XmlReader)"/> produces
        /// a correct sequence of nodes based on their types.
        /// </summary>
        [DataRow(sampleXml, new []{
            XmlNodeType.XmlDeclaration,
            XmlNodeType.Whitespace,
            XmlNodeType.Comment,
            XmlNodeType.Whitespace,
            XmlNodeType.Element,
            XmlNodeType.Text,
            XmlNodeType.Element,
            XmlNodeType.Whitespace,
            XmlNodeType.ProcessingInstruction,
            XmlNodeType.Whitespace,
            XmlNodeType.Comment,
            XmlNodeType.Whitespace,
            XmlNodeType.EndElement,
            XmlNodeType.Whitespace,
            XmlNodeType.EndElement,
            XmlNodeType.Whitespace,
            XmlNodeType.None
        })]
        [TestMethod]
        public void ReadFromTests(string xml, XmlNodeType[] nodeTypes)
        {
            using(var reader = XmlReader.Create(new StringReader(xml)))
            {
                var types = XmlReaderState.ReadFrom(reader).Select(state => state.NodeType).ToList();

                CollectionAssert.AreEqual(nodeTypes, types);
            }
        }
    }
}
