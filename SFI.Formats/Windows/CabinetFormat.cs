using IS4.SFI.Windows;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Cabinet archive format, producing instances of <see cref="CabinetFile"/>.
    /// </summary>
    [Description("Represents the Cabinet archive format.")]
    public class CabinetFormat : SignatureFormat<CabinetFile>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public CabinetFormat() : base("MSCF", "application/vnd.ms-cab-compressed", "cab")
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CabinetFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var file = new CabinetFile(stream);
            return await resultFactory(file, args);
        }
    }
}
