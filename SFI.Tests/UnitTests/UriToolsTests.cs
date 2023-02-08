using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IS4.SFI.Tests
{
    using static UriTools;

    /// <summary>
    /// The tests for the static methods in <see cref="UriTools"/>.
    /// </summary>
    [TestClass]
    public class UriToolsTests
    {
        /// <summary>
        /// The tests for <see cref="UriTools.CreateDataUri(string?, string?, ArraySegment{byte})"/>.
        /// </summary>
        [TestMethod]
        [DataRow(null, null, "", "data:application/octet-stream,")]
        [DataRow(null, null, "dGVzdA==", "data:application/octet-stream,test")]
        [DataRow(null, "ascii", "", "data:,")]
        [DataRow(null, "us-ascii", "", "data:,")]
        [DataRow(null, "utf-8", "", "data:;charset=utf-8,")]
        [DataRow("text/plain", "ascii", "", "data:text/plain,")]
        [DataRow("text/plain", "utf-8", "", "data:text/plain;charset=utf-8,")]
        [DataRow(null, null, "", "data:application/octet-stream,")]
        [DataRow(null, "ascii", "dGVzdA==", "data:,test")]
        [DataRow(null, "ascii", "JSUlJQ==", "data:,%25%25%25%25")]
        [DataRow(null, "ascii", "JSUlJSU=", "data:,%25%25%25%25%25")]
        [DataRow(null, "ascii", "JSUlJSUl", "data:;base64,JSUlJSUl")]
        [DataRow(null, "ascii", "JSUlJSUlJQ==", "data:;base64,JSUlJSUlJQ==")]
        public void CreateDataUriTest(string? mediaType, string? charset, string bytesBase64, string expectedUri)
        {
            var bytes = Convert.FromBase64String(bytesBase64);
            var result = CreateDataUri(mediaType, charset, bytes);
            Assert.AreEqual(expectedUri, result.OriginalString);
        }

        /// <summary>
        /// The tests for <see cref="CreatePublicId(string)"/> and <see cref="ExtractPublicId(Uri)"/>,
        /// taken from RFC 3151 (https://www.rfc-editor.org/rfc/rfc3151).
        /// </summary>
        [TestMethod]
        [DataRow("ISO/IEC 10179:1996//DTD DSSSL Architecture//EN", "urn:publicid:ISO%2FIEC+10179%3A1996:DTD+DSSSL+Architecture:EN")]
        [DataRow("ISO 8879:1986//ENTITIES Added Latin 1//EN", "urn:publicid:ISO+8879%3A1986:ENTITIES+Added+Latin+1:EN")]
        [DataRow("-//OASIS//DTD DocBook XML V4.1.2//EN", "urn:publicid:-:OASIS:DTD+DocBook+XML+V4.1.2:EN")]
        [DataRow("+//IDN example.org//DTD XML Bookmarks 1.0//EN//XML", "urn:publicid:%2B:IDN+example.org:DTD+XML+Bookmarks+1.0:EN:XML")]
        [DataRow("-//ArborText::prod//DTD Help Document::19970708//EN", "urn:publicid:-:ArborText;prod:DTD+Help+Document;19970708:EN")]
        [DataRow("foo", "urn:publicid:foo")]
        [DataRow("3+3=6", "urn:publicid:3%2B3=6")]
        [DataRow("-//Acme, Inc.//DTD Book Version 1.0", "urn:publicid:-:Acme,+Inc.:DTD+Book+Version+1.0")]
        [DataRow(null, "data:,")]
        [DataRow(null, "urn:oid:1")]
        public void CreateAndExtractPublicIdTest(string? id, string uriString)
        {
            Uri result;
            if(id == null)
            {
                result = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }else{
                result = CreatePublicId(id);
                Assert.AreEqual(uriString, result.OriginalString);
            }
            var extracted = ExtractPublicId(result);
            Assert.AreEqual(id, extracted);
        }

        /// <summary>
        /// The tests for <see cref="EscapeDataBytes(ArraySegment{byte})"/>.
        /// </summary>
        [TestMethod]
        [DataRow("YSAv", "a%20%2F")]
        [DataRow("AB8=", "%00%1F")]
        [DataRow("", "")]
        public void EscapeDataBytesTest(string bytesBase64, string expected)
        {
            var bytes = Convert.FromBase64String(bytesBase64);
            var result = EscapeDataBytes(bytes);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="CreateUuid(Guid)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("00000000000000000000000000000000", "urn:uuid:00000000-0000-0000-0000-000000000000")]
        [DataRow("a09f3676aab844d3b9d20ff5e13f35c3", "urn:uuid:a09f3676-aab8-44d3-b9d2-0ff5e13f35c3")]
        [DataRow("5A5F8CEBBF8F4494BC3956DB4B917096", "urn:uuid:5a5f8ceb-bf8f-4494-bc39-56db4b917096")]
        public void CreateUuidTest(string guidString, string expectedUri)
        {
            var guid = Guid.Parse(guidString);
            var result = CreateUuid(guid);
            Assert.AreEqual(expectedUri, result.OriginalString);
        }

        /// <summary>
        /// The tests for <see cref="ShortenUri(Uri, int, string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("http://localhost/23456", 6, false, "http://localhost/23456")]
        [DataRow("http://localhost/234567", 6, false, "http://localhost/23…67")]
        [DataRow("http://localhost/23456", 6, true, "http://localhost/23456#")]
        [DataRow("http://localhost/234567", 6, true, "http://localhost/23…67#")]
        [DataRow("http://localhost/23456?23456#23456", 6, false, "http://localhost/23456?23456#23456")]
        [DataRow("http://localhost/234567?234567#234567", 6, false, "http://localhost/23…67?23…67#23…67")]
        [DataRow("http://localhost/23456?23456#23456", 6, true, "http://localhost/23456?23456#23456")]
        [DataRow("http://localhost/234567?234567#234567", 6, true, "http://localhost/23…67?23…67#23…67")]
        public void ShortenUriTest(string uriString, int maxPartLength, bool addFragment, string expectedUri)
        {
            string additionalFragment = addFragment ? "(shortened)" : "";
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            var result = ShortenUri(uri, maxPartLength, additionalFragment);
            Assert.AreEqual(expectedUri + additionalFragment, result.ToString());
        }

        /// <summary>
        /// The tests for <see cref="UuidFromUri(Uri)"/> and <see cref="UriToUuidUri(Uri)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("http://localhost/", "d276ae2148075c28bd4905b37d06f624")]
        [DataRow("urn:oid:1", "ef1e8081ddad51bab7459568fd18e4c7")]
        [DataRow("data:,%2F%23%00#", "0e96c3c1cece587bbba709bc803d76e7")]
        [DataRow("relative/path", "800ccdf0e0a15f969015f7cde16261c6")]
        public void UuidFromUriTest(string uriString, string expectedGuid)
        {
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            var result = UuidFromUri(uri);
            var expected = Guid.Parse(expectedGuid);
            Assert.AreEqual(expected, result);
            Assert.AreEqual(CreateUuid(expected).OriginalString, UriToUuidUri(uri).OriginalString);
        }

        /// <summary>
        /// The tests for <see cref="MakeSubUri(Uri, string, bool)"/>.
        /// The individual cases are whether the URI has an authority,
        /// whether it has a query, whether it has a fragment, and whether
        /// the component starts with / or # or not.
        /// The last parameter is the expected part appended to the URI.
        /// </summary>
        [TestMethod]
        [DataRow("http://localhost/r", "c", false, "/c")]
        [DataRow("http://localhost/r?q", "c", false, "#c")]
        [DataRow("http://localhost/r#f", "c", false, "/c")]
        [DataRow("http://localhost/r?q#f", "c", false, "/c")]
        [DataRow("urn:publicid:r", "c", false, "#c")]
        [DataRow("urn:publicid:r?q", "c", false, "#c")]
        [DataRow("urn:publicid:r#f", "c", false, "/c")]
        [DataRow("urn:publicid:r?q#f", "c", false, "/c")]
        [DataRow("http://localhost/r", "/c", false, "/c")]
        [DataRow("http://localhost/r?q", "/c", false, "#/c")]
        [DataRow("http://localhost/r#f", "/c", false, "/c")]
        [DataRow("http://localhost/r?q#f", "/c", false, "/c")]
        [DataRow("urn:publicid:r", "/c", false, "#/c")]
        [DataRow("urn:publicid:r?q", "/c", false, "#/c")]
        [DataRow("urn:publicid:r#f", "/c", false, "/c")]
        [DataRow("urn:publicid:r?q#f", "/c", false, "/c")]
        [DataRow("http://localhost/r", "#c", false, "#c")]
        [DataRow("http://localhost/r?q", "#c", false, "#c")]
        [DataRow("http://localhost/r#f", "#c", false, "/c")]
        [DataRow("http://localhost/r?q#f", "#c", false, "/c")]
        [DataRow("urn:publicid:r", "#c", false, "#c")]
        [DataRow("urn:publicid:r?q", "#c", false, "#c")]
        [DataRow("urn:publicid:r#f", "#c", false, "/c")]
        [DataRow("urn:publicid:r?q#f", "#c", false, "/c")]
        [DataRow("http://localhost/r", "c", true, "/c")]
        [DataRow("http://localhost/r?q", "c", true, "#/c")]
        [DataRow("http://localhost/r#f", "c", true, "/c")]
        [DataRow("http://localhost/r?q#f", "c", true, "/c")]
        [DataRow("urn:publicid:r", "c", true, "#/c")]
        [DataRow("urn:publicid:r?q", "c", true, "#/c")]
        [DataRow("urn:publicid:r#f", "c", true, "/c")]
        [DataRow("urn:publicid:r?q#f", "c", true, "/c")]
        [DataRow("http://localhost/r", "/c", true, "/c")]
        [DataRow("http://localhost/r?q", "/c", true, "#/c")]
        [DataRow("http://localhost/r#f", "/c", true, "/c")]
        [DataRow("http://localhost/r?q#f", "/c", true, "/c")]
        [DataRow("urn:publicid:r", "/c", true, "#/c")]
        [DataRow("urn:publicid:r?q", "/c", true, "#/c")]
        [DataRow("urn:publicid:r#f", "/c", true, "/c")]
        [DataRow("urn:publicid:r?q#f", "/c", true, "/c")]
        [DataRow("http://localhost/r", "#c", true, "#c")]
        [DataRow("http://localhost/r?q", "#c", true, "#c")]
        [DataRow("http://localhost/r#f", "#c", true, "/c")]
        [DataRow("http://localhost/r?q#f", "#c", true, "/c")]
        [DataRow("urn:publicid:r", "#c", true, "#c")]
        [DataRow("urn:publicid:r?q", "#c", true, "#c")]
        [DataRow("urn:publicid:r#f", "#c", true, "/c")]
        [DataRow("urn:publicid:r?q#f", "#c", true, "/c")]
        public void MakeSubUriTest(string uriString, string component, bool formRoot, string expectedAppended)
        {
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            var result = MakeSubUri(uri, component, formRoot);
            Assert.AreEqual(uriString + expectedAppended, result.OriginalString);
        }
    }
}