using BisUtils.PBO;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the PBO archive format, as an instance of <see cref="PboFile"/>.
    /// </summary>
    [Description("Represents the PBO archive format used in Bohemia Interactive games.")]
    public class PboFormat : SignatureFormat<PboFile>
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public PboFormat() : base("\0sreV", "application/x-pbo", "pbo")
        {

        }

        public override async ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<PboFile, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            using var tmpPath = FileTools.GetTemporaryFile("pbo");
            using(var file = new FileStream(tmpPath, FileMode.CreateNew))
            {
                stream.CopyTo(file);
            }
            using var pbo = new PboFile(tmpPath);
            return await resultFactory(pbo, args);
        }
    }
}
