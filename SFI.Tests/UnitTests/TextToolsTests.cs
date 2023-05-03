using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.SFI.Tests
{
    using static TextTools;

    /// <summary>
    /// The tests for the static methods in <see cref="TextTools"/>.
    /// </summary>
    [TestClass]
    public class TextToolsTests
    {
        /// <summary>
        /// The tests for <see cref="SizeSuffix(long, int)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(1, 2, "1 B")]
        [DataRow(1024, 2, "1 KiB")]
        [DataRow(1024 * 3 / 2, 2, "1.5 KiB")]
        [DataRow(-1024 * 3 / 2, 2, "-1.5 KiB")]
        [DataRow(1024 * 1024, 2, "1 MiB")]
        [DataRow(1024 * 1024 * 1111 / 1000, 2, "1.11 MiB")]
        [DataRow(1024 * 1024 * 1119 / 1000, 2, "1.12 MiB")]
        [DataRow(1024 * 1024 * 1024, 2, "1 GiB")]
        [DataRow(1024L * 1024 * 1024 * 1024, 2, "1 TiB")]
        public void SizeSuffixTests(long value, int decimalPlaces, string expected)
        {
            var result = SizeSuffix(value, decimalPlaces);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="GetIdentifierFromType(Type)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(object), "object")]
        [DataRow(typeof(int), "int32")]
        [DataRow(typeof(ICloneable), "cloneable")]
        [DataRow(typeof(ApplicationException), "application-exception")]
        [DataRow(typeof(Dictionary<string, object>.Enumerator), "dictionary.string.object.enumerator")]
        [DataRow(typeof(TextTools), "text-tools")]
        [DataRow(typeof(VDS.RDF.Graph), "dot-net-rdf.graph")]
        public void GetIdentifierFromTypeTests(Type type, string expected)
        {
            var result = GetIdentifierFromType(type);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="FormatComponentName(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("NormalTestName", "normal-test-name")]
        [DataRow("NameWithUPPERChars", "name-with-upper-chars")]
        [DataRow("NameWith1digit", "name-with1digit")]
        [DataRow("NameWith1Digit", "name-with1-digit")]
        [DataRow("abc-123_.-xyz", "abc-123_.-xyz")]
        [DataRow("%&+/", "%&+/")]
        public void FormatComponentNameTests(string name, string expected)
        {
            var result = FormatComponentName(name);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="FormatMimeParameter(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("token", "token")]
        [DataRow("with space", "\"with space\"")]
        [DataRow("with\nnewline", "\"with\nnewline\"")]
        [DataRow("with\"quote", "\"with\\\"quote\"")]
        [DataRow("with\rreturn", "\"with\\\rreturn\"")]
        [DataRow("with\u00B7unicode", "\"=?utf-8?Q?with=C2=B7unicode?=\"")]
        [DataRow("with\u00B7unicode and space", "\"=?utf-8?Q?with=C2=B7unicode_and_space?=\"")]
        [DataRow("=?.?.?.?=", "\"=?utf-8?B?PT8uPy4/Lj89?=\"")]
        [DataRow("=?.?.?very long text to warrant Q?=", "\"=?utf-8?Q?=3D=3F.=3F.=3Fvery_long_text_to_warrant_Q=3F=3D?=\"")]
        public void FormatMimeParameterTests(string value, string expected)
        {
            var result = FormatMimeParameter(value);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="GetUserFriendlyName{T}(T)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(null, "<null>")]
        [DataRow(typeof(object), "Object")]
        [DataRow(typeof(int), "Int32")]
        [DataRow(typeof(ICloneable), "ICloneable")]
        [DataRow(typeof(Dictionary<string, object>), "Dictionary<String, Object>")]
        [DataRow(typeof(Dictionary<string, object>.Enumerator), "Enumerator<String, Object>")]
        [DataRow("str", "str")]
        [DataRow(123, "123")]
        [DataRow(1.12, "1.12")]
        [DataRow("", "String")]
        [DataRow("   ", "String")]
        public void GetUserFriendlyNameTests(object entity, string expected)
        {
            string result = entity == null ? GetUserFriendlyName(entity) : GetUserFriendlyName((dynamic)entity);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="ConvertWildcardToRegex(string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow("abc", "abc", true)]
        [DataRow("b", "abc", false)]
        [DataRow("?bc", "abc", true)]
        [DataRow("ab?", "abc", true)]
        [DataRow("ab?", "ab", false)]
        [DataRow("ab?", "abcd", false)]
        [DataRow("ab*", "abcd", true)]
        [DataRow("ab**", "abcd", true)]
        [DataRow("ab*", "ab", true)]
        [DataRow("ab**", "ab", true)]
        [DataRow("ab[x]", "abx", false)]
        [DataRow("ab[x]", "ab[x]", true)]
        public void ConvertWildcardToRegexTests(string pattern, string test, bool matches)
        {
            var regex = ConvertWildcardToRegex(pattern);
            var result = regex.IsMatch(test);
            Assert.AreEqual(matches, result);
        }

        /// <summary>
        /// The tests for <see cref="SubstituteVariables(string, IEnumerable{KeyValuePair{string, object?}})"/>.
        /// </summary>
        [TestMethod]
        [DataRow("${a},${b}", new[] { "a:1", "b:2" }, "1,2")]
        [DataRow("${a},${b}", new[] { "a:" }, ",${b}")]
        [DataRow("${a}${A}", new[] { "A:1" }, "${a}1")]
        [DataRow("text with ${a}, ${b}, ${c}", new[] { "b:2", "c:3", "d:4" }, "text with ${a}, 2, 3")]
        public void SubstituteVariablesTests(string text, string[] variablesArray, string expected)
        {
            var variables = variablesArray.Select(s => s.Split(':')).Select(s => new KeyValuePair<string, object?>(s[0], s[1]));
            var result = SubstituteVariables(text, variables);
            Assert.AreEqual(expected, result);
        }
    }
}
