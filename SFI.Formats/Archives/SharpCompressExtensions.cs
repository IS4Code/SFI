using SharpCompress.Common;
using SharpCompress.Readers;
using System.IO;
using System.Reflection;

namespace IS4.SFI.Formats.Archives
{
    /// <summary>
    /// Contains extension methods for SharpCompress types.
    /// </summary>
    public static class SharpCompressExtensions
    {
        static readonly ConstructorInfo entryStreamCtor = typeof(EntryStream).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IReader), typeof(Stream) }, null);

        /// <summary>
        /// Creates an instance of <see cref="EntryStream"/> from the supplied arguments.
        /// </summary>
        /// <param name="reader">The parent archive reader.</param>
        /// <param name="stream">The inner data stream.</param>
        /// <returns>A new instance of <see cref="EntryStream"/> from the arguments.</returns>
        public static EntryStream CreateEntryStream(this IReader reader, Stream stream)
        {
            return (EntryStream)entryStreamCtor.Invoke(new object[] { reader, stream });
        }
    }
}
