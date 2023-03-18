using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IS4.SFI.Tests
{
    using static DirectoryTools;

    /// <summary>
    /// The tests for the static methods in <see cref="DirectoryTools"/>.
    /// </summary>
    [TestClass]
    public class DirectoryToolsTests
    {
        /// <summary>
        /// The tests for <see cref="GroupByDirectories{TEntry}(IEnumerable{TEntry}, Func{TEntry, string?})"/>.
        /// </summary>
        [TestMethod]
        [DataRow(new[] { "a" }, new string?[] { null }, new[] { "a" })]
        [DataRow(
            new[] { null, "a", "b", "b/", "b/a", "b/c", "d/e" },
            new[] { null, "b", "d" },
            new[] { null, "a", "b" },
            new[] { "", "a", "c" },
            new[] { "e" }
        )]
        public void GroupByDirectoriesTests(string?[] entries, string?[] keys, params string?[][] values)
        {
            var result = GroupByDirectories(entries, s => s).Select(g => new KeyValuePair<string?, string?[]>(g.Key, g.Select(e => e.SubPath).ToArray())).ToArray();
            var expected = keys.Select((k, i) => new KeyValuePair<string?, string?[]>(k, values[i])).ToArray();

            CollectionAssert.AreEqual(expected, result, new KeyValueComparer());
        }

        class KeyValueComparer : IComparer
        {
            public int Compare(object? x, object? y)
            {
                if(x is not KeyValuePair<string?, string?[]> x2 || y is not KeyValuePair<string?, string?[]> y2)
                {
                    throw new NotSupportedException();
                }
                int keyCompare = Comparer<string>.Default.Compare(x2.Key, y2.Key);
                if(keyCompare != 0) return keyCompare;
                return x2.Value.SequenceEqual(y2.Value) ? 0 : x2.Value.GetHashCode().CompareTo(y2.Value.GetHashCode());
            }
        }
    }
}
