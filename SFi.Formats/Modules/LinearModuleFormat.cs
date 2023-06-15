using IS4.SFI.Formats.Modules;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Linear Executable (LE) MZ module format.
    /// </summary>
    public class LinearModuleFormat : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public LinearModuleFormat() : base("L", "application/x-msdownload;format=le", null)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(new LeReader(stream), args);
        }
    }
}
