using SharpCompress.Archives.Zip;
using System;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFormat : ArchiveFormat<ZipArchive>
    {
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<ZipArchive, TResult> resultFactory)
        {
            using(var archive = ZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return null;
                return resultFactory(archive);
            }
        }
    }
}
