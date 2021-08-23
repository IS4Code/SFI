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

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IModule, TResult, TArgs> resultFactory, TArgs args)
        {
            return resultFactory(new NeReader(stream), args);
        }
    }
}
