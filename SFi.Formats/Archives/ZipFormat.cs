using IS4.SFI.Formats.Archives;
using SharpCompress.Archives.Zip;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the ZIP archive format.
    /// </summary>
    [Description("Represents the ZIP archive format.")]
    public class ZipFormat : ArchiveFormat<ZipArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var archive = ZipArchive.Open(stream);
            if(!CheckArchive(archive)) return default;
            return await resultFactory(new ArchiveAdapter(archive), args);
        }
    }
}
