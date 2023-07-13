using IS4.SFI.Formats.Archives;
using SharpCompress.Archives.SevenZip;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the 7zip archive format.
    /// </summary>
    [Description("Represents the 7zip archive format.")]
    public class SevenZipFormat : ArchiveFormat<SevenZipArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public SevenZipFormat() : base("7z", "application/x-7z-compressed", "7z")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var archive = SevenZipArchive.Open(stream);
            if(!CheckArchive(archive)) return default;
            return await resultFactory(new ArchiveAdapter(archive), args);
        }
    }
}
