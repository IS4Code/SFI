using IS4.SFI.Tools.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="XmlExtensions"/>.
    /// </summary>
    [TestClass]
    public class XmlExtensionsTests
    {
        /// <summary>
        /// The tests for <see cref="XmlExtensions.LoadAsync(XmlDocument, XmlReader)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("<empty/>")]
        [DataRow(" <!-- comment --> <e a='x'><?target pi?>&amp;<![CDATA[ &amp; ]]> </e>")]
        public async Task LoadAsyncTests(string xml)
        {
            var settings = new XmlReaderSettings()
            {
                Async = true
            };
            var reader = XmlReader.Create(new StringReader(xml), settings);
            var expectedDocument = new XmlDocument();
            expectedDocument.Load(reader);

            reader = XmlReader.Create(new StringReader(xml), settings);
            var actualDocument = new XmlDocument();
            await XmlExtensions.LoadAsync(actualDocument, reader);

            Assert.AreEqual(expectedDocument.OuterXml, actualDocument.OuterXml);
        }
    }
}
