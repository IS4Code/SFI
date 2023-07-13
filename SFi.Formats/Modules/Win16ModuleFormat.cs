using IS4.SFI.Formats.Modules;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace IS4.SFI.Formats
{
    /// <summary>
    /// Represents the Win16 New Executable (NE) module format.
    /// </summary>
    [Description("Represents the Win16 New Executable (NE) module format.")]
    public class Win16ModuleFormat : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Win16ModuleFormat() : base("NE", "application/x-msdownload;format=ne", null)
        {

        }

        /// <inheritdoc/>
        public async override ValueTask<TResult?> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args) where TResult : default
        {
            return await resultFactory(new NeReader(stream), args);
        }
    }
}
