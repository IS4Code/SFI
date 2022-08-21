using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Media.Modules;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    /// <summary>
    /// Represents the Win16 New Executable (NE) module format.
    /// </summary>
    public class Win16ModuleFormat : WinModuleFormat
    {
        /// <inheritdoc cref="FileFormat{T}.FileFormat(string, string)"/>
        public Win16ModuleFormat() : base("NE", "application/x-msdownload;format=ne", null)
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args)
        {
            return await resultFactory(new NeReader(stream), args);
        }
    }
}
