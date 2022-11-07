using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.SFI
{
    /// <summary>
    /// Stores utility methods for manipulating hierarchical structures.
    /// </summary>
    public static class DirectoryTools
    {
        /// <summary>
        /// Groups a collection of objects each having a path based on the initial directory
        /// specified in the path.
        /// </summary>
        /// <typeparam name="TEntry">The type of the objects.</typeparam>
        /// <param name="entries">A collection of objects. Each should have a path determined by <paramref name="pathSelector"/>.</param>
        /// <param name="pathSelector">A function which should return the path of each object in <paramref name="entries"/>.</param>
        /// <returns>
        /// A sequence of instances of <see cref="IGrouping{TKey, TElement}"/> of the objects,
        /// where the <see cref="IGrouping{TKey, TElement}.Key"/> is the path to the first directory
        /// storing the entry.
        /// </returns>
        public static IEnumerable<IGrouping<string?, EntryInfo<TEntry>>> GroupByDirectories<TEntry>(IEnumerable<TEntry> entries, Func<TEntry, string?> pathSelector)
        {
            return GroupByDirectories(entries, pathSelector, e => e);
        }

        /// <summary>
        /// Groups a collection of objects each having a path based on the initial directory
        /// specified in the path.
        /// </summary>
        /// <typeparam name="TEntry">The type of the objects.</typeparam>
        /// <typeparam name="TValue">The type of the result.</typeparam>
        /// <param name="entries">A collection of objects. Each should have a path determined by <paramref name="pathSelector"/>.</param>
        /// <param name="pathSelector">A function which should return the path of each object in <paramref name="entries"/>.</param>
        /// <param name="valueSelector">A function that transforms a <typeparamref name="TEntry"/> into a value stored in the resulting <see cref="EntryInfo{TEntry}"/>.</param>
        /// <returns>
        /// A sequence of instances of <see cref="IGrouping{TKey, TElement}"/> of the objects,
        /// where the <see cref="IGrouping{TKey, TElement}.Key"/> is the path to the first directory
        /// storing the entry.
        /// </returns>
        public static IEnumerable<IGrouping<string?, EntryInfo<TValue>>> GroupByDirectories<TEntry, TValue>(IEnumerable<TEntry> entries, Func<TEntry, string?> pathSelector, Func<TEntry, TValue> valueSelector)
        {
            return entries.Select(e => (d: GetFirstDir(pathSelector(e)), e)).GroupBy(p => p.d.dir, p => new EntryInfo<TValue>(p.d.subpath, valueSelector(p.e)));
        }

        /// <summary>
        /// Splits <paramref name="path"/> based on the first '/'.
        /// </summary>
        static (string? dir, string subpath) GetFirstDir(string? path)
        {
            if(path == null) return default;
            int index = path.IndexOf('/');
            if(index == -1) return (null, path);
            return (path.Substring(0, index), path.Substring(index + 1));
        }

        /// <summary>
        /// Stores an information about an entry in a hierarchical structure, such
        /// as a hierarchy of directories in a file system.
        /// </summary>
        /// <typeparam name="TEntry">The type of the entry.</typeparam>
        public struct EntryInfo<TEntry>
        {
            /// <summary>
            /// The path of the entry, relative to an initial directory.
            /// </summary>
            public string SubPath { get; }

            /// <summary>
            /// The value of the entry.
            /// </summary>
            public TEntry Entry { get; }

            /// <summary>
            /// Creates a new instance of the entry.
            /// </summary>
            /// <param name="subpath">The value of <see cref="SubPath"/>.</param>
            /// <param name="entry">The value of <see cref="Entry"/>.</param>
            public EntryInfo(string subpath, TEntry entry)
            {
                SubPath = subpath;
                Entry = entry;
            }
        }
    }
}
