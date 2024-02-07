using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IS4.SFI.Tests
{
    using static TextTools;
    using static BindingFlags;
    using static MemberIdFormatOptions;

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
        /// The tests for <see cref="GetImpliedMediaTypeFromXml(Uri?, string?, string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(null, null, "elem", "root=elem")]
        [DataRow("http://localhost/test", null, "elem", "root=elem;ns=\"http://localhost/test\"")]
        [DataRow("http://localhost/test", "PUBLIC ID", "elem", "root=elem;ns=\"http://localhost/test\";public=\"PUBLIC ID\"")]
        [DataRow(null, "PUBLIC ID", "elem", "root=elem;public=\"PUBLIC ID\"")]
        public void GetImpliedMediaTypeFromXmlTests(string? nsUri, string? publicId, string rootName, string expected)
        {
            string prefix = ImpliedMediaTypePrefix + "document+xml;";

            Uri? ns = nsUri == null ? null : new Uri(nsUri);
            var result = GetImpliedMediaTypeFromXml(ns, publicId, rootName);
            Assert.AreEqual(prefix + expected, result);
        }

        /// <summary>
        /// The tests for <see cref="GetImpliedMediaTypeFromJson(IReadOnlyDictionary{string, string})"/>
        /// and <see cref="GetImpliedMediaTypeFromJsonSequence(IReadOnlyDictionary{string, string})"/>.
        /// </summary>
        [TestMethod]
        [DataRow(new string[0], new string[0], "")]
        [DataRow(new[] { "a" }, new[] { "string" }, "a=string")]
        [DataRow(new[] { "a" }, new[] { "null" }, "")]
        [DataRow(new[] { "c", "B", "a" }, new[] { "string", "number", "ARRAY" }, "a=array;b=number;c=string")]
        [DataRow(new[] { "c", "B", "a" }, new[] { "string", "null", "ARRAY" }, "a=array;c=string")]
        [DataRow(new[] { "a", "A" }, new[] { "string", "string" }, "a=string")]
        [DataRow(new[] { "a", "A" }, new[] { "string", "number" }, null)]
        [DataRow(new[] { "a", "A" }, new[] { "string", "null" }, null)]
        [DataRow(new[] { "a" }, new[] { "undefined" }, null)]
        public void GetImpliedMediaTypeFromJsonTests(string[] keys, string[] values, string? expected)
        {
            string prefix = ImpliedMediaTypePrefix + "object+json";
            string prefixSeq = ImpliedMediaTypePrefix + "object+json-seq";

            if(expected is not (null or ""))
            {
                prefix += ";";
                prefixSeq += ";";
            }

            var pairs = keys.Zip(values, (a, b) => new KeyValuePair<string, string>(a, b));

            foreach(var sorted in new[] { false, true })
            {
                IReadOnlyDictionary<string, string> dict;
                if(sorted)
                {
                    var sortedDict = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach(var pair in pairs)
                    {
                        if(!sortedDict.TryAdd(pair.Key, pair.Value))
                        {
                            // deduplication affects the result
                            return;
                        }
                    }
                    dict = sortedDict;
                }else{
                    dict = new Dictionary<string, string>(pairs);
                }
                
                if(expected != null)
                {
                    var result = GetImpliedMediaTypeFromJson(dict);
                    Assert.AreEqual(prefix + expected, result);
                    var resultSeq = GetImpliedMediaTypeFromJsonSequence(dict);
                    Assert.AreEqual(prefixSeq + expected, resultSeq);
                }else{
                    Assert.ThrowsException<ArgumentException>(() => GetImpliedMediaTypeFromJson(dict));
                    Assert.ThrowsException<ArgumentException>(() => GetImpliedMediaTypeFromJsonSequence(dict));
                }
            }
        }

        /// <summary>
        /// The tests for <see cref="GetImpliedMediaTypeFromYaml(IReadOnlyDictionary{string, Uri})"/>.
        /// </summary>
        [TestMethod]
        [DataRow(new string[0], new string[0], "")]
        [DataRow(new[] { "a" }, new[] { "tag:yaml.org,2002:str" }, "a=str")]
        [DataRow(new[] { "a" }, new[] { "tag:yaml.org,2002:null" }, "")]
        [DataRow(new[] { "c", "B", "a" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:int", "tag:yaml.org,2002:SEQ" }, "a=SEQ;b=int;c=str")]
        [DataRow(new[] { "c", "B", "a" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:null", "tag:yaml.org,2002:SEQ" }, "a=SEQ;c=str")]
        [DataRow(new[] { "a", "A" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:str" }, "a=str")]
        [DataRow(new[] { "a", "A" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:int" }, null)]
        [DataRow(new[] { "a", "A" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:STR" }, null)]
        [DataRow(new[] { "a", "A" }, new[] { "tag:yaml.org,2002:str", "tag:yaml.org,2002:null" }, null)]
        [DataRow(new[] { "a" }, new[] { "about:test" }, "a=\"about:test\"")]
        [DataRow(new[] { "a" }, new[] { "tag:yaml.org,2002:about:test" }, "a=\"tag:yaml.org,2002:about:test\"")]
        public void GetImpliedMediaTypeFromYamlTests(string[] keys, string[] values, string? expected)
        {
            string prefix = ImpliedMediaTypePrefix + "object+yaml";

            if(expected is not (null or ""))
            {
                prefix += ";";
            }

            var pairs = keys.Zip(values, (a, b) => new KeyValuePair<string, Uri>(a, new Uri(b)));

            foreach(var sorted in new[] { false, true })
            {
                IReadOnlyDictionary<string, Uri> dict;
                if(sorted)
                {
                    var sortedDict = new SortedDictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);
                    foreach(var pair in pairs)
                    {
                        if(!sortedDict.TryAdd(pair.Key, pair.Value))
                        {
                            // deduplication affects the result
                            return;
                        }
                    }
                    dict = sortedDict;
                }else{
                    dict = new Dictionary<string, Uri>(pairs);
                }
                
                if(expected != null)
                {
                    var result = GetImpliedMediaTypeFromYaml(dict);
                    Assert.AreEqual(prefix + expected, result);
                }else{
                    Assert.ThrowsException<ArgumentException>(() => GetImpliedMediaTypeFromYaml(dict));
                }
            }
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

        /// <summary>
        /// The tests for <see cref="FormatMemberId(MemberInfo, MemberIdFormatOptions)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(object), null, 0, 0, "Object")]
        [DataRow(typeof(object), null, 0, IncludeDeclaringMembersAndNamespace, "System.Object")]
        [DataRow(typeof(object[]), null, 0, 0, "Object[]")]
        [DataRow(typeof(object[]), null, 0, IncludeDeclaringMembersAndNamespace, "System.Object[]")]
        [DataRow(typeof(object), "GetHashCode", Instance, 0, "GetHashCode")]
        [DataRow(typeof(object), "GetHashCode", Instance, IncludeDeclaringMembers, "Object.GetHashCode")]
        [DataRow(typeof(object), "GetHashCode", Instance, IncludeDeclaringMembersAndNamespace, "System.Object.GetHashCode")]
        [DataRow(typeof(object), "Equals", Instance, 0, "Equals(System.Object)")]
        [DataRow(typeof(object), "Equals", Instance, IncludeDeclaringMembers, "Object.Equals(System.Object)")]
        [DataRow(typeof(object), "Equals", Instance, IncludeDeclaringMembersAndNamespace, "System.Object.Equals(System.Object)")]
        [DataRow(typeof(Func<,>), null, 0, IncludeDeclaringMembers, "Func`2")]
        [DataRow(typeof(Func<,>), null, 0, UriEscaping | IncludeDeclaringMembers, "Func%602")]
        [DataRow(typeof(Func<,>), "Invoke", Instance, IncludeDeclaringMembers, "Func`2.Invoke(`0)")]
        [DataRow(typeof(Func<byte,char>), "Invoke", Instance, IncludeDeclaringMembers, "Func{System.Byte,System.Char}.Invoke(System.Byte)")]
        [DataRow(typeof(Array), "Resize", Static, IncludeDeclaringMembers, "Array.Resize``1(``0[]@,System.Int32)")]
        [DataRow(typeof(Pointer), "Box", Static, IncludeDeclaringMembers, "Pointer.Box(System.Void*,System.Type)")]
        [DataRow(typeof(object[,]), null, 0, IncludeDeclaringMembers, "Object[0:,0:]")]
        public void FormatMemberIdTests(Type type, string? memberName, BindingFlags bindingFlags, MemberIdFormatOptions options, string expected)
        {
            var member = memberName == null ? type : type.GetMember(memberName, Public | NonPublic | bindingFlags).Single();
            var result = FormatMemberId(member, options);
            Assert.AreEqual(expected, result);
        }
    }
}
