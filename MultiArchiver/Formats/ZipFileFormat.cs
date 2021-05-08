using System;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFileFormat : SignatureFormat<ZipArchive>
    {
        public ZipFileFormat() : base(3, "PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult>(Stream stream, Func<ZipArchive, TResult> resultFactory)
        {
            using(var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                return resultFactory(archive);
            }
        }
    }
}
