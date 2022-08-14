using SharpCompress.Common;
using SharpCompress.Readers;
using System.IO;
using System.Reflection;

namespace IS4.MultiArchiver.Formats.Archives
{
    /// <summary>
    /// Contains extension methods for SharpCompress types.
    /// </summary>
    public static class SharpCompressExtensions
    {
        static readonly ConstructorInfo entryStreamCtor = typeof(EntryStream).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IReader), typeof(Stream) }, null);

        public static EntryStream CreateEntryStream(this IReader reader, Stream stream)
        {
            return (EntryStream)entryStreamCtor.Invoke(new object[] { reader, stream });
        }
    }
}
