using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using IS4.MultiArchiver.Services;
using SharpCompress.Archives.Zip;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class ZipFormat : ArchiveFormat<ZipArchive>
    {
        public ZipFormat() : base("PK", "application/zip", "zip")
        {

        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveFile, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var archive = ZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return default;
                return resultFactory(new ArchiveAdapter(archive), args);
            }
        }
    }
}
