using IS4.SFI.Analyzers;
using IS4.SFI.Services;
using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="Extensions"/>.
    /// </summary>
    [TestClass]
    public class ExtensionsTests
    {
        /// <summary>
        /// The tests for <see cref="Extensions.IsEntityAnalyzerType(Type)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(object), false)]
        [DataRow(typeof(int), false)]
        [DataRow(typeof(IEntityAnalyzer<object>), false)]
        [DataRow(typeof(DataAnalyzer), true)]
        [DataRow(typeof(DataObjectAnalyzer), true)]
        public void IsEntityAnalyzerTypeTests(Type type, bool expected)
        {
            Assert.AreEqual(expected, type.IsEntityAnalyzerType());
        }

        /// <summary>
        /// The tests for <see cref="Extensions.GetEntityAnalyzerTypes(Type)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(object))]
        [DataRow(typeof(int))]
        [DataRow(typeof(IEntityAnalyzer<object>))]
        [DataRow(typeof(DataAnalyzer), typeof(IStreamFactory), typeof(byte[]))]
        [DataRow(typeof(DataObjectAnalyzer), typeof(IDataObject))]
        public void GetEntityAnalyzerTypesTests(Type type, params Type[] expectedTypes)
        {
            CollectionAssert.AreEquivalent(type.GetEntityAnalyzerTypes().ToArray(), expectedTypes);
        }

        /// <summary>
        /// The tests for <see cref="Extensions.JoinAsLabel(System.Collections.Generic.IEnumerable{string?}, string)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow(null, "", "")]
        [DataRow("a", "a")]
        [DataRow("a|b", "a", "", "b")]
        public void JoinAsLabelTests(string? expected, params string[] components)
        {
            Assert.AreEqual(expected, components.JoinAsLabel("|"));
        }
    }
}
