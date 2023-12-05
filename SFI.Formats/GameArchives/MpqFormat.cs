using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using War3Net.IO.Mpq;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the MPQ archive format, as an instance of <see cref="MpqArchive"/>.
    /// </summary>
    [Description("Represents the MPQ archive format used in Blizzard games.")]
    public class MpqFormat : SignatureFormat<MpqArchive>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public MpqFormat() : base("MPQ", "application/x-mpq-compressed", "mpq")
        {

        }

        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<MpqArchive, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var archive = new MpqArchive(stream, loadListFile: true);
            return await resultFactory(archive, args);
        }
    }
}
