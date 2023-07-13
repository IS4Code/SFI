using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the ZIP archive format, creating an instance of <see cref="ZipArchive"/>.
    /// </summary>
    [Description("Represents the ZIP archive format.")]
    public class ZipFileFormat : SignatureFormat<ZipArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ZipFileFormat() : base("PK", "application/zip", "zip")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<ZipArchive, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            return await resultFactory(archive, args);
        }
    }
}
