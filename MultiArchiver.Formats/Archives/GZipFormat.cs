using IS4.MultiArchiver.Formats.Archives;
using IS4.MultiArchiver.Media;
using SharpCompress.Readers.GZip;
using System.IO;
using System.Threading.Tasks;

namespace IS4.MultiArchiver.Formats
{
    public class GZipFormat : SignatureFormat<IArchiveReader>
    {
        public GZipFormat() : base(new byte[] { 0x1F, 0x8B }, "application/gzip", "gz")
        {

        }

        public override async ValueTask<TResult> Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<IArchiveReader, TResult, TArgs> resultFactory, TArgs args)
        {
            using(var reader = GZipReader.Open(stream))
            {
                return await resultFactory(new ArchiveReaderAdapter(reader), args);
            }
        }
    }
}
