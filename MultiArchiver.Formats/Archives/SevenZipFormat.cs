using IS4.MultiArchiver.Services;
using SharpCompress.Archives.SevenZip;
using System.IO;

namespace IS4.MultiArchiver.Formats
{
    public class SevenZipFormat : ArchiveFormat<SevenZipArchive>
    {
        public SevenZipFormat() : base("7z", "application/x-7z-compressed", "7z")
        {

        }

        public override TResult Match<TResult>(Stream stream, ResultFactory<SevenZipArchive, TResult> resultFactory)
        {
            using(var archive = SevenZipArchive.Open(stream))
            {
                if(!CheckArchive(archive)) return null;
                return resultFactory(archive);
            }
        }
    }
}
