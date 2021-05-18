using IS4.MultiArchiver.Windows;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class Win16ModuleFormat : ModuleFormat<NeReader>
    {
        public Win16ModuleFormat() : base("NE", "application/x-msdownload;format=ne", null)
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<NeReader, TResult> resultFactory)
        {
            return resultFactory(new NeReader(stream));
        }

        public override string GetExtension(NeReader value)
        {
            return (value.Flags & 0x8000) != 0 ? "dll" : "exe";
        }
    }
}
