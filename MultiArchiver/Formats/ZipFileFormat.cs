using IS4.MultiArchiver.Services;
using System.IO;
using System.IO.Compression;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFileFormat : SignatureFormat<ZipArchive>
    {
        public ZipFileFormat() : base("PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<ZipArchive, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                return resultFactory(archive, args);
            }
        }
    }
}
