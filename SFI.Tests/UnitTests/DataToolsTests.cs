using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;

namespace IS4.SFI.Tests
{
    using static DataTools;

    /// <summary>
    /// The tests for the static methods in <see cref="DataTools"/>.
    /// </summary>
    [TestClass]
    public class DataToolsTests
    {
        /// <summary>
        /// The tests for <see cref="FindBom(ReadOnlySpan{byte})"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", 0)]
        [DataRow("00-00", 0)]
        [DataRow("00-00-00-00-00-00", 0)]
        [DataRow("EF-BB-BF", 3)]
        [DataRow("EF-BB-BF-00-00-00", 3)]
        [DataRow("FE-FF", 2)]
        [DataRow("FF-FE", 2)]
        [DataRow("00-00-FE-FF", 4)]
        [DataRow("FF-FE-00-00", 4)]
        public void FindBomTests(string dataString, int expected)
        {
            var result = FindBom(Hex(dataString));
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="Base32{TList}(TList, StringBuilder, string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21", "ECJUXN9FRAEVH7DVR2JZZ")]
        public void Base32Tests(string bytesString, string expected)
        {
            var sb = new StringBuilder();
            Base32(Hex(bytesString), sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        /// The tests for <see cref="Base58{TList}(TList, StringBuilder, string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("00", "1")]
        [DataRow("00-00", "11")]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21", "72k1xXWG59fYdzSNoA")]
        [DataRow("48-65-6C-6C-6F-20-57-6F-72-6C-64-21", "2NEpo7TZRRrLZSi2U")]
        [DataRow("00-00-28-7F-B4-CD", "11233QC4")]
        public void Base58Tests(string bytesString, string expected)
        {
            var sb = new StringBuilder();
            Base58(Hex(bytesString), sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        /// The tests for <see cref="Base64Url(byte[], System.Text.StringBuilder)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21", "SGVsbG8sIFdvcmxkIQ")]
        public void Base64UrlTests(string bytesString, string expected)
        {
            var sb = new StringBuilder();
            Base64Url(Hex(bytesString), sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        /// The tests for <see cref="Varint(ulong)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(0uL, "00")]
        [DataRow(1uL, "01")]
        [DataRow(127uL, "7F")]
        [DataRow(128uL, "80-01")]
        [DataRow(255uL, "FF-01")]
        [DataRow(300uL, "AC-02")]
        [DataRow(16384uL, "80-80-01")]
        public void VarintTests(ulong value, string expected)
        {
            var result = Varint(value);
            CollectionAssert.AreEqual(Hex(expected), result.ToArray());
        }

        /// <summary>
        /// The tests for <see cref="EncodeMultihash(ulong, byte[], int?)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(0x11uL, "CA-FE", null, "11-02-CA-FE")]
        [DataRow(128uL, "CA-FE", null, "80-01-02-CA-FE")]
        [DataRow(128uL, "CA-FE", 0, "80-01-00-CA-FE")]
        [DataRow(128uL, "", 4, "80-01-04")]
        public void EncodeMultihashTests(ulong id, string hashString, int? hashLength, string expected)
        {
            var result = EncodeMultihash(id, Hex(hashString), hashLength);
            CollectionAssert.AreEqual(Hex(expected), result);
        }

        /// <summary>
        /// The tests for <see cref="ExtractSignature(ArraySegment{byte})"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", null)]
        [DataRow("Alone", null)]
        [DataRow("A", null)]
        [DataRow("AB\0", "AB")]
        [DataRow("AB\n", null)]
        [DataRow("AB\n\0", null)]
        [DataRow("ABCDEFGH\0", "ABCDEFGH")]
        [DataRow("ABCDEFGH\n", null)]
        [DataRow("ABCDEFGH\n\0", null)]
        [DataRow("ABCDEFGHI", null)]
        [DataRow("ABCDEFGHI\0", null)]
        public void ExtractSignatureTests(string headerString, string? expected)
        {
            var result = ExtractSignature(Encoding.ASCII.GetBytes(headerString));
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="ExtractFirstLine(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("First line", "First line")]
        [DataRow("First line\r", "First line")]
        [DataRow("First line\n", "First line")]
        [DataRow("First line\r\n", "First line")]
        [DataRow("First line\rstill first line", "First line\rstill first line")]
        [DataRow("First line\nSecond line", "First line")]
        [DataRow("First line\r\nSecond line", "First line")]
        public void ExtractFirstLineTests(string text, string expected)
        {
            var result = ExtractFirstLine(text);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="ExtractInterpreter(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", null)]
        [DataRow("#!/bin/sh", "sh")]
        [DataRow("#!/bin/sh args", "sh")]
        [DataRow("#!/usr/bin/perl", "perl")]
        [DataRow("#!/usr/bin/perl args", "perl")]
        [DataRow("#!/bin/env pwsh", "pwsh")]
        [DataRow("#!/bin/env pwsh args", "pwsh")]
        [DataRow("#!/usr/bin/env lua", "lua")]
        [DataRow("#!/usr/bin/env lua args", "lua")]
        [DataRow("#!/sh", "sh")]
        [DataRow("#!sh", "sh")]
        [DataRow(" #!sh", null)]
        [DataRow("#sh", null)]
        public void ExtractInterpreterTests(string text, string expected)
        {
            var result = ExtractInterpreter(text);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="ReplaceControlCharacters(string, System.Text.Encoding?)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("(\x00\x01\x02)", null, "(\u2400\u2401\u2402)")]
        [DataRow("(\x00\x01\x02)", "ascii", "(\u2400\u2401\u2402)")]
        [DataRow("(\x00\x01\x02)", "utf-8", "(\x00\x01\x02)")]
        [DataRow("(\x08\x09\x0A\x0D)", null, "(\u2408\t\n\r)")]
        [DataRow("(\x1F\x20\x7F)", null, "(\u241F \u2421)")]
        public void ReplaceControlCharactersTests(string str, string? originalEncodingName, string expected)
        {
            var originalEncoding =
                originalEncodingName == null ? null
                : Encoding.GetEncoding(originalEncodingName);
            var result = ReplaceControlCharacters(str, originalEncoding);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="IsBinary(ArraySegment{byte})"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", false)]
        [DataRow("00", true)]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21", false)]
        [DataRow("48-65-6C-6C-6F-2C-20-00-57-6F-72-6C-64-21", true)]
        [DataRow("00-48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21", true)]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21-00", false)]
        [DataRow("48-65-6C-6C-6F-2C-20-57-6F-72-6C-64-21-00-00-00-00-00-00", false)]
        public void IsBinaryTests(string dataString, bool expected)
        {
            var result = IsBinary(Hex(dataString));
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="GuidFromName(byte[], string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "", "e129f27c-5103-5c5c-844b-cdf0a15e160d")]
        [DataRow("00000000-0000-0000-0000-000000000000", "Hello, World!", "64dc4ac1-4a83-5b25-aabb-3603762ee2e3")]
        [DataRow("00000000-0000-0000-0000-000000000000", "☺", "985c38a8-c4c2-5aca-a958-309742893e14")]
        public void GuidFromNameTests(string namespaceGuid, string name, string expected)
        {
            var result = GuidFromName(Guid.Parse(namespaceGuid).ToByteArray(), name);
            Assert.AreEqual(Guid.Parse(expected), result);
        }

        /// <summary>
        /// The tests for <see cref="IsSafeString(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("", true)]
        [DataRow("Hello, World!", true)]
        [DataRow("Hello,\r\n\tWorld!", true)]
        [DataRow("\u200D ZWJ", false)]
        [DataRow(" \u200D ZWJ", true)]
        [DataRow("\u0301 Mn", false)]
        [DataRow(" \u0301 Mn", true)]
        [DataRow("\u0903 Mc", false)]
        [DataRow(" \u0903 Mc", true)]
        [DataRow("\u20DD Me", false)]
        [DataRow(" \u20DD Me", true)]
        [DataRow(" \0", false)]
        [DataRow(" \x7F", false)]
        [DataRow(" \x84", false)]
        [DataRow(" \x85", true)]
        [DataRow(" \x9F", false)]
        public void IsSafeStringTests(string str, bool expected)
        {
            var result = IsSafeString(str);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="IsSafeString(string)"/>, requiring
        /// a <see cref="char"/> array as input due to invalid surrogate characters.
        /// </summary>
        [TestMethod]
        [DataRow(new[] { '\uDC00' }, false)]
        [DataRow(new[] { '\0', '\uDC00' }, false)]
        [DataRow(new[] { '\uD80C' }, false)]
        [DataRow(new[] { '\uD80C', '\0' }, false)]
        [DataRow(new[] { '\uDC00', '\uD80C' }, false)]
        [DataRow(new[] { '\uD80C', '\uDC00' }, true)]
        public void IsSafeStringBinaryTests(char[] strChars, bool expected)
        {
            var result = IsSafeString(new String(strChars));
            Assert.AreEqual(expected, result);
        }

        static char[] splitChars = { '-' };

        static byte[] Hex(string input)
        {
            return input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Select(n => Convert.ToByte(n, 16)).ToArray();
        }
    }
}
