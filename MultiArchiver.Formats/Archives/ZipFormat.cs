using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using SharpCompress.Archives.Zip;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the ZIP archive format.
    /// </summary>
    public class ZipFormat : ArchiveFormat<ZipArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var archive = ZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return default;
                return await resultFactory(new ArchiveAdapter(archive), args);
            }
        }
    }
}
