using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Media.Modules;
using IS4.MultiArchiver.Services;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class Win16ModuleFormat : WinModuleFormat
    {
        public Win16ModuleFormat() : base("NE", "application/x-msdownload;format=ne", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<IModule, TResult> resultFactory)
        {
            return resultFactory(new NeReader(stream));
        }
    }
}
