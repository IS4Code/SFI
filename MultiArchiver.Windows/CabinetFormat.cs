using IS4.MultiArchiver.Windows;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Cabinet archive format, producing instances of <see cref="CabinetFile"/>.
    /// </summary>
    public class CabinetFormat : SignatureFormat<CabinetFile>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public CabinetFormat() : base("MSCF", "application/vnd.ms-cab-compressed", "cab")
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<CabinetFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var file = new CabinetFile(stream))
            {
                return await resultFactory(file, args);
            }
        }
    }
}
