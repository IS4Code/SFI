using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using SharpCompress.Archives.SevenZip;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the 7zip archive format.
    /// </summary>
    public class SevenZipFormat : ArchiveFormat<SevenZipArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SevenZipFormat() : base("7z", "application/x-7z-compressed", "7z")
        {

        }

        public async override ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var archive = SevenZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return default;
                return await resultFactory(new ArchiveAdapter(archive), args);
            }
        }
    }
}
