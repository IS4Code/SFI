using IS4.MultiArchiver.Services;
using IS4.MultiArchiver.Windows;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class Win16ModuleFormat : ModuleFormat
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
