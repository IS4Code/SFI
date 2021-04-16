using System;
using System.Collections.Generic;
using System.Linq;

namespace IS4.MultiArchiver
{
    public static class DirectoryTools
    {
        public static IEnumerable<IGrouping<string, EntryInfo<TEntry>>> GroupByDirectories<TEntry>(IEnumerable<TEntry> entries, Func<TEntry, string> pathSelector)
        {
            return GroupByDirectories(entries, pathSelector, e => e);
        }

        public static IEnumerable<IGrouping<string, EntryInfo<TValue>>> GroupByDirectories<TEntry, TValue>(IEnumerable<TEntry> entries, Func<TEntry, string> pathSelector, Func<TEntry, TValue> valueSelector)
        {
            return entries.Select(e => (d: GetFirstDir(pathSelector(e)), e)).GroupBy(p => p.d.dir, p => new EntryInfo<TValue>(p.d.subpath, valueSelector(p.e)));
        }

        static (string dir, string subpath) GetFirstDir(string path)
        {
            int index = path.IndexOf('/');
            if(index == -1) return (null, path);
            return (path.Substring(0, index), path.Substring(index + 1));
        }

        public struct EntryInfo<TEntry>
        {
            public string SubPath { get; }
            public TEntry Entry { get; }

            public EntryInfo(string subpath, TEntry entry)
            {
                SubPath = subpath;
                Entry = entry;
            }
        }
    }
}
