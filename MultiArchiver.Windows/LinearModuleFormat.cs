using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class LinearModuleFormat : WinModuleFormat
    {
        public LinearModuleFormat() : base("L", "application/x-msdownload;format=le", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<IModule, TResult> resultFactory)
        {
            return resultFactory(new LeReader(stream));
        }
    }
}
